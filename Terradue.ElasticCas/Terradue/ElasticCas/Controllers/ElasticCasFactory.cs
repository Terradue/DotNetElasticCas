using System;
using System.Xml;
using ServiceStack.Text;
using System.Collections.Generic;
using Mono.Addins;
using Terradue.ElasticCas.Model;
using ServiceStack.ServiceHost;
using ServiceStack.WebHost.Endpoints.Utils;
using ServiceStack.WebHost.Endpoints;
using Terradue.ElasticCas.Request;
using log4net;
using Terradue.OpenSearch.Schema;
using Terradue.OpenSearch.Engine;
using System.Collections.Specialized;
using System.Web;
using System.Linq;
using Terradue.ElasticCas.Exceptions;
using System.Diagnostics;
using Terradue.ElasticCas.Types;
using Nest;
using Terradue.ElasticCas.Responses;
using Elasticsearch.Net;
using System.Threading.Tasks;

namespace Terradue.ElasticCas.Controllers {

    public class ElasticCasFactory {
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        ElasticClientWrapper client;

        public System.Configuration.Configuration RootWebConfig { get; set; }

        public System.Configuration.KeyValueConfigurationElement EsHost { get; set; }

        public System.Configuration.KeyValueConfigurationElement EsPort { get; set; }

        public readonly ILog Logger;

        public ElasticCasFactory(string name) {

            if (Settings.Exists()) {
                log.DebugFormat("Using ElasticSearch Host : {0}", Settings.ElasticSearchServer);
            } else {
                log.DebugFormat("No ElasticSearch Host specified, using default : {0}", Settings.ElasticSearchServer);
            }

            client = new ElasticClientWrapper();
            log.DebugFormat("New ElasticSearch Connection from {0}", name);

        }

        public ElasticClientWrapper Client {
            get {
                return client;
            }
        }

        internal void CreateOrUpdateAlias(string alias, string index, string oldindex = null) {

            // check that an alias with the same name does not exists
            if (client.AliasExists(alias).Exists) {
                // In case of update
                if (oldindex != null) {
                    // Alias exists but there is another index linked
                    if (!client.AliasExists(alias, oldindex).Exists)
                        throw new InvalidOperationException(string.Format("'{0}' alias already exists but is not link to the index '{1}'", alias, oldindex));
                    // we can update!
                    else {
                        client.Alias(a => a.Remove(r => r.Alias(alias).Index(oldindex)).Add(s => s.Alias(alias).Index(index)));
                        return;
                    }
                }
                
            }

            // Create the alias to the newly created index
            client.Alias(a => a.Add(s => s.Alias(alias).Index(index)));

        }


        internal IndexInformation CreateCatalogueIndex(Terradue.ElasticCas.Request.CreateIndexRequest createRequest, bool destroy = false) {

            // check that another index with the same name does not exists
            if (client.IndexExists(i => i.Index(createRequest.IndexName)).Exists) {
                if (destroy) {
                    client.DeleteIndex(d => d.Index(createRequest.IndexName));
                } else {
                    throw new InvalidOperationException(string.Format("'{0}' index already exists and cannot be overriden without data loss", createRequest.IndexName));
                }
            }

            // check that an alias with the same name does not exists
            if (client.AliasExists(createRequest.IndexName).Exists) {
                if (destroy) {
                    client.Alias(a => a.Remove(d => d.Alias(createRequest.IndexName)));
                } else {
                    throw new InvalidOperationException(string.Format("'{0}' alias already exists and cannot be overriden without data loss", createRequest.IndexName));
                }
            }
           
            // set the analyzers
            var htmlAnalyzer = new CustomAnalyzer();
            htmlAnalyzer.Tokenizer = "standard";
            htmlAnalyzer.Filter = new List<string>(){ "standard" };
            htmlAnalyzer.CharFilter = new List<string>(){ "html_strip" };

            // index creation
            var response = client.CreateIndex(c => c.Index(createRequest.IndexName + "_v" + createRequest.Version).Analysis(a => a.Analyzers(an => an.Add(
                               "htmlAnalyzer", htmlAnalyzer))));
         

            // Retrieve index info
            IndexInformation indexInformation = new IndexInformation();
            var status = client.Status(s => s.Index(createRequest.IndexName + "_v" + createRequest.Version));
            indexInformation.Name = createRequest.IndexName;
            indexInformation.Shards = status.Shards;
            indexInformation.Types = new List<TypeInformation>();

            // Set the metadata
            if (createRequest.Created.Ticks == 0)
                createRequest.Created = DateTime.UtcNow;
            client.Index<Index>(createRequest, i => i.Index(createRequest.IndexName).Type("meta").Id("general"));


            // Init mappings for each types declared
            List<string> typeNames = new List<string>();
            foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IOpenSearchableElasticType))) {
                IOpenSearchableElasticType type = (IOpenSearchableElasticType)node.CreateInstance();

                var enAttribute = type.GetType().GetCustomAttributes(
                                      typeof(ExtensionNodeAttribute), true).FirstOrDefault() as ExtensionNodeAttribute;

                if (type is GenericJsonOpenSearchable)
                    continue;

                IndexNameMarker indexName = new IndexNameMarker();
                indexName.Name = createRequest.IndexName;
                PutMappingRequest putMappingRequest = new PutMappingRequest(indexName, type.Type);
                ((IPutMappingRequest)putMappingRequest).Mapping = type.GetRootMapping();

                var responseM = client.Map(putMappingRequest);

                indexInformation.Types.Add(new TypeInformation() {
                    Name = type.Type.Name, 
                    Version = putMappingRequest.Mapping.Meta.ContainsKey("version") ? (int)putMappingRequest.Mapping.Meta["version"] : 1,
                    Description = enAttribute.Description
                });

                typeNames.Add(type.Type.Name);
            }


            return indexInformation;

        }


        /// <summary>
        /// Migrates all indices.
        /// </summary>
        /// <returns>The all indices.</returns>
        /// <param name="scanOnly">If set to <c>true</c> scan only.</param>
        public List<MigrationResponse> MigrateAllIndices() {

            List<MigrationResponse> response = new List<MigrationResponse>();

            var aliases = client.CatAliases();

            foreach (var alias in aliases.Records) {
                var status = MigrateIndex(alias);
                if (status != null)
                    response.Add(status);
            }

            return response;
        }

        /// <summary>
        /// Scans all indices.
        /// </summary>
        /// <returns>The all indices.</returns>
        public List<MigrationResponse> ScanAllIndices() {

            List<MigrationResponse> response = new List<MigrationResponse>();

            var aliases = client.CatAliases();

            foreach (var alias in aliases.Records) {
                var status = ScanIndex(alias);
                if (status != null)
                    response.Add(status);
            }

            return response;
        }


        /// <summary>
        /// Migrates an index.
        /// </summary>
        /// <returns>The index.</returns>
        /// <param name="alias">Alias.</param>
        /// <param name="scanOnly">If set to <c>true</c> scan only.</param>
        public MigrationResponse ScanIndex(CatAliasesRecord alias) {

            MigrationResponse resp = new MigrationResponse();
            resp.AliasName = alias.Alias;
            resp.IndexName = alias.Index;

            var mappingResponse = client.GetMapping(new GetMappingRequest(alias.Index, "*"));

            if (mappingResponse.Mappings.Count == 0) {
                resp.Error = "Not a Geosquare index";
                return null;
            }

            List<TypeMapping> mappingCanditates = new List<TypeMapping>();
            List<TypeInformation> typeStatuses = new List<TypeInformation>();

            foreach (var mapping in mappingResponse.Mappings) {
                foreach (var rootmap in mapping.Value) {

                    TypeInformation typeStatus = new TypeInformation();
                    typeStatus.Name = rootmap.TypeName;

                    typeStatuses.Add(typeStatus);

                    if (rootmap.Mapping.Meta != null && rootmap.Mapping.Meta.ContainsKey("type") && String.Equals(rootmap.Mapping.Meta["type"], "elasticCas")) {

                        mappingCanditates.Add(rootmap);

                        if (rootmap.Mapping.Meta.ContainsKey("version"))
                            typeStatus.Version = (int)rootmap.Mapping.Meta["version"];
                        else
                            typeStatus.Version = 1;

                        IOpenSearchableElasticType osetype = GetOpenSearchableElasticTypeByNameOrDefault(alias.Index, rootmap.TypeName);

                        if (osetype == null) {
                            typeStatus.Message = "Plugin for this type not found!";
                            continue;
                        }

                        RootObjectMapping curMap = osetype.GetRootMapping();

                        if (curMap.Meta != null && curMap.Meta.ContainsKey("version")) {
                            var currentVersion = (int)curMap.Meta["version"];
                            if (currentVersion > typeStatus.Version) {
                                typeStatus.Message = string.Format("This type must be updated to the lastest version ({0})", currentVersion);
                            }
                        } else {
                            typeStatus.Message = "the plugin for this type do not support versioning!";
                        }
                    } else {
                        typeStatus.Message = "No plugin found for this type";
                    }
                }
            }


            if (mappingCanditates.Count() == 0) {
                resp.Error = "Not a Geosquare index";
                return resp;
            } else {
                resp.TypeStatus = typeStatuses;
            }

            // if scan only, we get out
            return resp;

        }

        /// <summary>
        /// Locks the index.
        /// </summary>
        /// <returns><c>true</c>, if index was locked, <c>false</c> otherwise.</returns>
        /// <param name="indexName">Index name.</param>
        public bool LockIndex(string indexName) {

            try {
                client.Index<object>(new object(), i => i.Index(indexName).Type("lock").Id("global").OpType(Elasticsearch.Net.OpType.Create));
                return true;
            } catch (Exception) {
                return false;
            }

        }

        /// <summary>
        /// Removes the lock of the index.
        /// </summary>
        /// <param name="indexName">Index name.</param>
        public void RemoveIndexLock(string indexName) {
            client.Delete<object>(d => d.Index(indexName).Type("lock").Id("global"));
        }

        public MigrationResponse MigrateIndex(CatAliasesRecord alias) {

            MigrationResponse report = new MigrationResponse();
            report.TypeStatus = new List<TypeInformation>();

            // Lock the index
            if (!LockIndex(alias.Index)) {
                report.Error = "Cannot lock the index. Migration aborted";
                return report;
            }

            // Get the current index metadata
            var curIndexMeta = client.Get<Index>(i => i.Index(alias.Index).Type("meta").Id("general"));

            if (curIndexMeta == null) {
                report.Error = "No metadata found for index! Migration aborted.";
                RemoveIndexLock(alias.Index);
                return report;
            }

            // create the new index request
            Terradue.ElasticCas.Request.CreateIndexRequest newIndexMeta = new Terradue.ElasticCas.Request.CreateIndexRequest(curIndexMeta.Source);
            // increment the version
            newIndexMeta.Version++;
            // update the migrated date
            newIndexMeta.LastMigrated = DateTime.UtcNow;

            IndexInformation newIndexInfo = new IndexInformation();
           
            // Create the new index
            try {
                // with the version as suffix
                newIndexInfo = this.CreateCatalogueIndex(newIndexMeta);
            } catch (InvalidOperationException e) {
                report.Error = "Cannot create the new index : " + e.Message;
                return report;
            }

            // Get the current index mappings
            var mappingResponse = client.GetMapping(new GetMappingRequest(alias.Index, "*"));

            if (mappingResponse.Mappings.Count == 0) {
                report.Error = "No mapping found";
                return report;
            }

            // migrate all of them
            foreach (var mapping in mappingResponse.Mappings) {
                foreach (var rootmap in mapping.Value) {

                    TypeInformation typeStatus = new TypeInformation();
                    typeStatus.Name = rootmap.TypeName;
                    report.TypeStatus.Add(typeStatus);

                    // save the Type information as a document in the index
                    client.Index(typeStatus, i => i.Index(alias.Index).Type("migration").Id(rootmap.TypeName));

                    if (rootmap.Mapping.Meta != null && rootmap.Mapping.Meta.ContainsKey("type") && String.Equals(rootmap.Mapping.Meta["type"], "elasticCas")) {
                        
                        if (rootmap.Mapping.Meta.ContainsKey("version"))
                            typeStatus.Version = (int)rootmap.Mapping.Meta["version"];
                        else
                            typeStatus.Version = 1;

                        // search the plugins's type
                        IOpenSearchableElasticType osetype = GetOpenSearchableElasticTypeByNameOrDefault(alias.Index, rootmap.TypeName);

                        if (osetype == null) {
                            typeStatus.Message = "Simple reindex in progress";
                            // Start the reindex as a task
                            Task.Factory.StartNew(() => {this.ReindexTypeSimple(alias.Index, newIndexInfo.Name, typeStatus);});
                        }

                        RootObjectMapping curMap = osetype.GetRootMapping();

                        if (curMap.Meta != null && curMap.Meta.ContainsKey("version")) {
                            var currentVersion = (int)curMap.Meta["version"];
                            if (currentVersion > typeStatus.Version) {
                                // reindex with migration
                            } else {
                                // Start the reindex as a task
                                Task.Factory.StartNew(() => {this.ReindexTypeSimple(alias.Index, newIndexInfo.Name, typeStatus);});
                            }
                        } else {
                            typeStatus.Message = "the plugin for this type do not support versioning!";
                            // Start the reindex as a task
                            Task.Factory.StartNew(() => {this.ReindexTypeSimple(alias.Index, newIndexInfo.Name, typeStatus);});
                        }

                    } else {
                        // Start the reindex as a task
                        Task.Factory.StartNew(() => {this.ReindexTypeSimple(alias.Index, newIndexInfo.Name, typeStatus);});
                    }
                }
            }

            // Remove the lock
            RemoveIndexLock(alias.Index);

            return report;
        }

        internal void ReindexTypeSimple(string currentIndexName, string nextIndexName, TypeInformation type) {

            // Start a scan and scroll for the reindex
            var searchResult = client.Search<object>(s => s.Index(currentIndexName).Type(type.Name).From(0).Size(100).Query(q => q.MatchAll()).SearchType(SearchType.Scan).Scroll("2m"));

            if (searchResult.Total <= 0) {
                type.Message = "Existing index has no documents, nothing to reindex.";
                // save the Type information as a document in the index
                client.Index(type, i => i.Index(currentIndexName).Type("migration").Id(type.Name));
            } else {
                var page = 0;
                IBulkResponse bulkResponse = null;
                do {
                    var result = searchResult;
                    searchResult = client.Scroll<object>(s => s.Scroll("2m").ScrollId(result.ScrollId));
                    if (searchResult.Documents != null && searchResult.Documents.Any()) {
                        searchResult.ThrowOnError(type, currentIndexName, "reindex scroll " + page, this);
                        bulkResponse = client.Bulk(b => {
                            foreach (var hit in searchResult.Hits) {
                                b.Index<object>(bi => bi.Document(hit.Source).Type(hit.Type).Index(nextIndexName).Id(hit.Id));
                            }
                            return b;
                        }).ThrowOnError(type, currentIndexName, "reindex page " + page, this);
                        type.Message = "Reindexing progress: " + (page + 1) * 100;
                        // save the Type information as a document in the index
                        client.Index(type, i => i.Index(currentIndexName).Type("migration").Id(type.Name));
                    }
                    ++page;
                } while (searchResult.IsValid && bulkResponse != null && bulkResponse.IsValid && searchResult.Documents != null && searchResult.Documents.Any());
                type.Message = "Reindexing complete!";
                // save the Type information as a document in the index
                client.Index(type, i => i.Index(currentIndexName).Type("migration").Id(type.Name));
            }
        }



        public static void LoadPlugins(AppHost application) {

            //foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IElasticDocument))) {
            //    IElasticDocument doc = (IElasticDocument)node.CreateInstance();
            //}

        }

        public IOpenSearchableElasticType GetOpenSearchableElasticTypeByNameOrDefault(string indexName, string typeName, Dictionary<string, object> parameters = null) {
            var indexNameMarker = new IndexNameMarker();
            indexNameMarker.Name = indexName;
            var typeNameMarker = new TypeNameMarker();
            typeNameMarker.Name = typeName;
            IOpenSearchableElasticType type = GetOpenSearchableElasticTypeByName(indexName, typeName);

            if (type == null) {
                type = new GenericJsonOpenSearchable(indexName);
            }

            return type;
        }

        public IOpenSearchableElasticType GetOpenSearchableElasticTypeByName(string indexName, string typeName, Dictionary<string, object> parameters = null) {
            foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IOpenSearchableElasticType))) {
                IOpenSearchableElasticType etype = (IOpenSearchableElasticType)node.CreateInstance();
                if (etype.Type.Name == typeName) {
                    Type type = node.Type;
                    var ctor = type.GetConstructor(new Type[1]{ typeof(IndexNameMarker) });
                    var indexNameMarker = new IndexNameMarker();
                    indexNameMarker.Name = indexName;
                    var typeNameMarker = new TypeNameMarker();
                    typeNameMarker.Name = typeName;
                    etype = (IOpenSearchableElasticType)ctor.Invoke(new object[1]{ indexNameMarker });
                    etype.Parameters = parameters;
                    return etype;
                }
            }
            return null;
        }

        public static OpenSearchDescription GetDefaultOpenSearchDescription(IOpenSearchableElasticType type, bool withParam = true) {

            OpenSearchDescription osd = new OpenSearchDescription();

            osd.ShortName = "Terradue Catalogue";
            osd.Attribution = "Terradue";
            osd.Contact = "info@terradue.com";
            osd.Developer = "Terradue GeoSpatial Development Team";
            osd.SyndicationRight = "open";
            osd.AdultContent = "false";
            osd.Language = "en-us";
            osd.OutputEncoding = "UTF-8";
            osd.InputEncoding = "UTF-8";
            osd.Description = string.Format("This Search Service performs queries in the index {0}. There are several URL templates that return the results in different formats." +
            "This search service is in accordance with the OGC 10-032r3 specification.", type.Index.Name);

            OpenSearchEngine ose = type.GetOpenSearchEngine(new NameValueCollection());

            var searchExtensions = ose.Extensions;
            List<OpenSearchDescriptionUrl> urls = new List<OpenSearchDescriptionUrl>();

            NameValueCollection parameters = type.GetOpenSearchParameters(type.DefaultMimeType);
            OpenSearchDescriptionUrlParameter[] parameters2 = null;
            if (withParam)
                parameters2 = type.DescribeParameters().ToArray();

            UriBuilder searchUrl = new UriBuilder(string.Format("{0}/catalogue/{1}/{2}/search", Settings.BaseUrl, type.Index.Name, type.Type.Name));
            NameValueCollection queryString = HttpUtility.ParseQueryString("?format=format");
            parameters.AllKeys.FirstOrDefault(k => {
                queryString.Add(k, parameters[k]);
                return false;
            });

            OpenSearchDescriptionUrl url = null;

            foreach (int code in searchExtensions.Keys) {

                queryString.Set("format", searchExtensions[code].Identifier);
                string[] queryStrings = Array.ConvertAll(queryString.AllKeys, key => string.Format("{0}={1}", key, queryString[key]));
                searchUrl.Query = string.Join("&", queryStrings);
                url = new OpenSearchDescriptionUrl(searchExtensions[code].DiscoveryContentType, 
                                                   searchUrl.ToString(),
                                                   "results");
                url.Parameters = parameters2;
                urls.Add(url);

            }
            searchUrl = new UriBuilder(string.Format("{0}/catalogue/{1}/{2}/description", Settings.BaseUrl, type.Index.Name, type.Type.Name));


            url = new OpenSearchDescriptionUrl("application/opensearchdescription+xml", 
                                               searchUrl.ToString(),
                                               "self");


            urls.Add(url);
            osd.Url = urls.ToArray();

            return osd;
        }

        public static string GetMappingName(IElasticObject obj) {
            var mapAttribute = obj.GetType().GetCustomAttributes(
                                   typeof(ElasticTypeAttribute), true
                               ).FirstOrDefault() as ElasticTypeAttribute;
            if (mapAttribute != null) {
                return mapAttribute.Name;
            }
            return null;
        }
    }
}