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

namespace Terradue.ElasticCas.Controller {

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

            var settings = new IndexSettings();
            settings.Analysis.Analyzers.Add(new KeyValuePair<string, AnalyzerBase>("default", new StandardAnalyzer()));

            var response = client.CreateIndex(c => c.Index(createRequest.IndexName).InitializeUsing(settings));
         
            IndexInformation indexInformation = new IndexInformation();
            var status = client.Status(s => s.Index(createRequest.IndexName));
            indexInformation.Name = createRequest.IndexName;
            indexInformation.Shards = status.Shards;
            indexInformation.Mappings = new List<RootObjectMapping>();

            // Init mappings for each types declared
            if (createRequest.TypeNames == null || createRequest.TypeNames.Length == 0) {
                List<string> typeNames = new List<string>();
                foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IElasticDocument))) {
                    IElasticDocument doc = (IElasticDocument)node.CreateInstance();
                    if (doc is GenericJson)
                        continue;

                    IPutMappingRequest putMapping = new PutMappingRequest(createRequest.IndexName, doc.TypeName);
                    putMapping.Mapping = doc.GetMapping();

                    client.Map(putMapping);
                    indexInformation.Mappings.Add(putMapping.Mapping);

                    typeNames.Add(doc.TypeName);
                }
                createRequest.TypeNames = typeNames.ToArray();

            } else {
                foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IElasticDocument))) {
                    IElasticDocument doc = (IElasticDocument)node.CreateInstance();
                    foreach (string type in createRequest.TypeNames) {
                        if (type == doc.TypeName) {

                            IPutMappingRequest putMapping = new PutMappingRequest(createRequest.IndexName, doc.TypeName);
                            putMapping.Mapping = doc.GetMapping();

                            client.Map(putMapping);
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

        public static IElasticDocument GetElasticDocumentByTypeName(string typeName, Dictionary<string, object> parameters = null) {
            foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IElasticDocument))) {
                IElasticDocument doc = (IElasticDocument)node.CreateInstance();
                doc.Parameters = parameters;
                if (doc.TypeName == typeName) {
                    return doc;
                }
            }
            return null;
        }

        public OpenSearchDescription GetDefaultOpenSearchDescription(IElasticDocument document) {

            OpenSearchDescription osd = new OpenSearchDescription();

            osd.ShortName = document.IndexName + " Elastic Catalogue";
            osd.Attribution = "Terradue";
            osd.Contact = "info@terradue.com";
            osd.Developer = "Terradue GeoSpatial Development Team";
            osd.SyndicationRight = "open";
            osd.AdultContent = "false";
            osd.Language = "en-us";
            osd.OutputEncoding = "UTF-8";
            osd.InputEncoding = "UTF-8";
            osd.Description = string.Format("This Search Service performs queries in the index {0}. There are several URL templates that return the results in different formats." +
            "This search service is in accordance with the OGC 10-032r3 specification.", document.IndexName);

            OpenSearchEngine ose = document.GetOpenSearchEngine(new NameValueCollection());

            var osee = ose.GetFirstExtensionByTypeAbility(document.GetType());
            if (osee == null) {
                throw new InvalidTypeSearchException(document.TypeName, string.Format("OpenSearch Engine for Type '{0}' is not found in the extensions. Check that plugins are loaded", document.TypeName));
            }

            var searchExtensions = ose.Extensions;
            List<OpenSearchDescriptionUrl> urls = new List<OpenSearchDescriptionUrl>();

            NameValueCollection parameters = document.GetOpenSearchParameters(document.DefaultMimeType);

            UriBuilder searchUrl = new UriBuilder(string.Format("{0}/catalogue/{1}/{2}/search", Settings.BaseUrl, document.IndexName, document.TypeName));
            NameValueCollection queryString = HttpUtility.ParseQueryString("?format=format");
            parameters.AllKeys.FirstOrDefault(k => {
                queryString.Add(parameters[k], "{" + k + "?}");
                return false;
            });

            foreach (int code in searchExtensions.Keys) {

                queryString.Set("format", searchExtensions[code].Identifier);
                searchUrl.Query = queryString.ToString();
                urls.Add(new OpenSearchDescriptionUrl(searchExtensions[code].DiscoveryContentType, 
                                                      searchUrl.ToString(),
                                                      "results"));

            }
            searchUrl = new UriBuilder(string.Format("{0}/catalogue/{1}/{2}/description", Settings.BaseUrl, document.IndexName, document.TypeName));
            urls.Add(new OpenSearchDescriptionUrl("application/opensearchdescription+xml", 
                                                  searchUrl.ToString(),
                                                  "self"));
            osd.Url = urls.ToArray();

            return osd;
        }
    }
}