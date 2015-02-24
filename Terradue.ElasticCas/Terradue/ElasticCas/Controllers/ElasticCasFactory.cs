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

namespace Terradue.ElasticCas.Controllers {

    public class ElasticCasFactory {
        ElasticClientWrapper client;

        public System.Configuration.Configuration RootWebConfig { get; set; }

        public System.Configuration.KeyValueConfigurationElement EsHost { get; set; }

        public System.Configuration.KeyValueConfigurationElement EsPort { get; set; }

        public readonly ILog Logger;

        internal ElasticCasFactory(string name) {

            // Init Log
            Logger = LogManager.GetLogger(name);

            if (Settings.Exists()) {
                Logger.InfoFormat("Using ElasticSearch Host : {0}", Settings.ElasticSearchServer);
            } else {
                Logger.InfoFormat("No ElasticSearch Host specified, using default : {0}", Settings.ElasticSearchServer);
            }

            client = new ElasticClientWrapper();
            Logger.InfoFormat("New ElasticSearch Connection from {0}", name);
        }

        public ElasticClientWrapper Client {
            get {
                return client;
            }
        }

        internal IndexInformation CreateCatalogueIndex(Terradue.ElasticCas.Request.CreateIndexRequest createRequest, bool destroy = false) {

            if (client.IndexExists(i => i.Index(createRequest.IndexName)).Exists) {

                if (destroy) {
                    client.DeleteIndex(d => d.Index(createRequest.IndexName));
                } else {
                    throw new InvalidOperationException(string.Format("'{0}' index already exists and cannot be overriden without data loss", createRequest.IndexName));
                }
            }
           
            var response = client.CreateIndex(c => c.Index(createRequest.IndexName));
         
            IndexInformation indexInformation = new IndexInformation();
            var status = client.Status(s => s.Index(createRequest.IndexName));
            indexInformation.Name = createRequest.IndexName;
            indexInformation.Shards = status.Shards;
            indexInformation.Mappings = new Dictionary<string, ICollection<PropertyNameMarker>>();

            // Init mappings for each types declared
            if (createRequest.TypeNames == null || createRequest.TypeNames.Length == 0) {
                List<string> typeNames = new List<string>();
                foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IOpenSearchableElasticType))) {
                    IOpenSearchableElasticType type = (IOpenSearchableElasticType)node.CreateInstance();
                    if (type is GenericJsonOpenSearchable)
                        continue;

                    IndexNameMarker indexName = new IndexNameMarker();
                    indexName.Name = createRequest.IndexName;
                    PutMappingRequest putMappingRequest = new PutMappingRequest(indexName, type.Type);
                    ((IPutMappingRequest)putMappingRequest).Mapping = type.GetRootMapping();

                    client.Map(putMappingRequest);

                    indexInformation.Mappings.Add(type.Identifier, ((IPutMappingRequest)putMappingRequest).Mapping.Properties.Keys);

                    typeNames.Add(type.Type.Name);
                }
                createRequest.TypeNames = typeNames.ToArray();

            } else {
                foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IElasticItem))) {
                    IOpenSearchableElasticType type = (IOpenSearchableElasticType)node.CreateInstance();
                    foreach (string typeName in createRequest.TypeNames) {
                        if (typeName == type.Type.Name) {

                            PutMappingRequest putMappingRequest = new PutMappingRequest(type.Index, type.Type);
                            ((IPutMappingRequest)putMappingRequest).Mapping = type.GetRootMapping();

                            client.Map(putMappingRequest);

                            indexInformation.Mappings.Add(type.Identifier, ((IPutMappingRequest)putMappingRequest).Mapping.Properties.Keys);

                        }
                    }
                }
            }

            return indexInformation;

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
                type = new GenericJsonOpenSearchable(indexName, typeName);
            }

            return type;
        }

        public IOpenSearchableElasticType GetOpenSearchableElasticTypeByName(string indexName, string typeName, Dictionary<string, object> parameters = null) {
            foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IOpenSearchableElasticType))) {
                IOpenSearchableElasticType etype = (IOpenSearchableElasticType)node.CreateInstance();
                if (etype.Identifier == typeName) {
                    Type type = node.Type;
                    var ctor = type.GetConstructor(new Type[2]{ typeof(IndexNameMarker), typeof(TypeNameMarker) });
                    var indexNameMarker = new IndexNameMarker();
                    indexNameMarker.Name = indexName;
                    var typeNameMarker = new TypeNameMarker();
                    typeNameMarker.Name = typeName;
                    etype = (IOpenSearchableElasticType)ctor.Invoke(new object[2]{ indexNameMarker, typeNameMarker });
                    etype.Parameters = parameters;
                    return etype;
                }
            }
            return null;
        }

        public static OpenSearchDescription GetDefaultOpenSearchDescription(IOpenSearchableElasticType type) {

            OpenSearchDescription osd = new OpenSearchDescription();

            osd.ShortName = type + " Elastic Catalogue";
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

            UriBuilder searchUrl = new UriBuilder(string.Format("{0}/catalogue/{1}/{2}/search", Settings.BaseUrl, type.Index.Name, type.Type.Name));
            NameValueCollection queryString = HttpUtility.ParseQueryString("?format=format");
            parameters.AllKeys.FirstOrDefault(k => {
                queryString.Add(parameters[k], "{" + k + "?}");
                return false;
            });

            foreach (int code in searchExtensions.Keys) {

                queryString.Set("format", searchExtensions[code].Identifier);
                string[] queryStrings = Array.ConvertAll(parameters.AllKeys, key => string.Format("{1}={{{0}?}}", key, parameters[key]));
                searchUrl.Query = string.Join("&", queryStrings);
                urls.Add(new OpenSearchDescriptionUrl(searchExtensions[code].DiscoveryContentType, 
                                                      searchUrl.ToString(),
                                                      "results"));

            }
            searchUrl = new UriBuilder(string.Format("{0}/catalogue/{1}/{2}/description", Settings.BaseUrl, type.Index.Name, type.Type.Name));
            urls.Add(new OpenSearchDescriptionUrl("application/opensearchdescription+xml", 
                                                  searchUrl.ToString(),
                                                  "self"));
            osd.Url = urls.ToArray();

            return osd;
        }
    }
}