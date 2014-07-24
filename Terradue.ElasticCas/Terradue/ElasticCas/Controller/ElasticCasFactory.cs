using System;
using PlainElastic.Net;
using System.Xml;
using ServiceStack.Text;
using PlainElastic.Net.Serialization;
using PlainElastic.Net.Mappings;
using System.Collections.Generic;
using Mono.Addins;
using Terradue.ElasticCas.Model;
using ServiceStack.ServiceHost;
using ServiceStack.WebHost.Endpoints.Utils;
using ServiceStack.WebHost.Endpoints;
using Terradue.ElasticCas.Request;
using PlainElastic.Net.IndexSettings;
using log4net;
using Terradue.OpenSearch.Schema;
using Terradue.OpenSearch.Engine;
using System.Collections.Specialized;
using System.Web;
using System.Linq;

namespace Terradue.ElasticCas {

    public class ElasticCasFactory {
        ElasticConnection esConnection;

        public System.Configuration.Configuration RootWebConfig { get; set; }

        public System.Configuration.KeyValueConfigurationElement EsHost { get; set; }

        public System.Configuration.KeyValueConfigurationElement EsPort { get; set; }

        public readonly ILog Logger;

        internal ElasticCasFactory(string name) {

            // Init Log
            Logger = LogManager.GetLogger(name);

            // Get web config
            RootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(null);
            if (RootWebConfig.AppSettings.Settings.Count > 0) {
                EsHost = RootWebConfig.AppSettings.Settings["esHost"];
                EsPort = RootWebConfig.AppSettings.Settings["esPort"];
                if (EsHost != null)
                    Logger.InfoFormat("Using ElasticSearch Host : {0}", EsHost);
                else {
                    EsPort.Value = "localhost";
                    Logger.InfoFormat("No ElasticSearch Host specified, using default : {0}", EsHost);
                }
            }

            esConnection = new ElasticConnection(EsHost.Value, int.Parse(EsPort.Value));
            Logger.InfoFormat("New ElasticSearch Connection from {0}", name);
        }

        public ElasticConnection EsConnection {
            get {
                return esConnection;
            }
        }

        internal OperationResult CreateCatalogueIndex(string indexName, string[] typeNames, bool destroy = false) {

            if (IsIndexExists(indexName, esConnection)) {

                if (destroy) {
                    esConnection.Delete(Commands.Delete(indexName));
                } else {
                    throw new InvalidOperationException(string.Format("'{0}' index already exists and cannot be overriden without data loss", indexName));
                }
            }

            string result;

            string command = Commands.CreateIndex(indexName);

            string jsondata = new IndexSettingsBuilder().Analysis(a => a.Analyzer(an => an.Custom("default", custom => custom
                                                                                                  .Tokenizer(DefaultTokenizers.standard)
                                                                                                  .Filter(DefaultTokenFilters.standard)))).Build();
            try {
                result = esConnection.Put(command, jsondata);
            } catch (Exception e) {
                throw e;
            }

            // Init mapp    ings for each types declared

            // If no types is declared
            if (typeNames == null || typeNames.Length == 0) {
                foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IElasticDocument))) {
                    IElasticDocument doc = (IElasticDocument)node.CreateInstance();
                    command = Commands.PutMapping(indexName, doc.TypeName);
                    jsondata = doc.GetMapping();
                    try {
                        result = esConnection.Put(command, jsondata);
                    } catch (Exception e) {
                        throw e;
                    }
                }
            } else {
                foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IElasticDocument))) {
                    IElasticDocument doc = (IElasticDocument)node.CreateInstance();
                    foreach (string type in typeNames) {
                        if (type == doc.TypeName) {
                            command = Commands.PutMapping(indexName, doc.TypeName);
                            jsondata = doc.GetMapping();
                            esConnection.Put(command, jsondata);
                        }
                    }
                }
            }

            return esConnection.Get(Commands.GetMapping(new string[]{ indexName }, typeNames));

        }

        internal static bool IsIndexExists(string indexName, ElasticConnection connection) {
            try {
                connection.Head(new IndexExistsCommand(indexName));
                return true;
            } catch (OperationException ex) {
                if (ex.HttpStatusCode == 404)
                    return false;
                throw;
            }
        }

        public static void LoadPlugins(AppHost application) {

            //foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IElasticDocument))) {
            //    IElasticDocument doc = (IElasticDocument)node.CreateInstance();
            //}

        }

        public static IElasticDocumentCollection GetElasticDocumentCollectionByTypeName(string typeName, Dictionary<string, object> parameters = null) {
            foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IElasticDocumentCollection))) {
                IElasticDocumentCollection docs = (IElasticDocumentCollection)node.CreateInstance();
                docs.Parameters = parameters;
                if (docs.TypeName == typeName) {
                    return docs;
                }
            }
            return null;
        }

        public static IElasticDocument GetElasticDocumentByTypeName(string typeName, Dictionary<string, object> parameters = null) {
            foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IElasticDocument))) {
                IElasticDocument docs = (IElasticDocument)node.CreateInstance();
                if (docs.TypeName == typeName) {
                    return docs;
                }
            }
            return null;
        }

        public OpenSearchDescription GetDefaultOpenSearchDescription (IElasticDocumentCollection collection){

            OpenSearchDescription osd = new OpenSearchDescription();

            osd.ShortName = collection.TypeName + " Elastic Catalogue";
            osd.Attribution = "Terradue";
            osd.Contact = "info@terradue.com";
            osd.Developer = "Terradue GeoSpatial Development Team";
            osd.SyndicationRight = "open";
            osd.AdultContent = "false";
            osd.Language = "en-us";
            osd.OutputEncoding = "UTF-8";
            osd.InputEncoding = "UTF-8";
            osd.Description = string.Format("This Search Service performs queries in the index {0}. There are several URL templates that return the results in different formats." +
                                            "This search service is in accordance with the OGC 10-032r3 specification.", collection.IndexName);

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            var osee = ose.GetFirstExtensionByTypeAbility(collection.GetOpenSearchResultType());
            if (osee == null) {
                throw new InvalidTypeSearchException(collection.TypeName, string.Format("OpenSearch Engine for Type '{0}' is not found in the extensions. Check that plugins are loaded", collection.TypeName));
            }

            var searchExtensions = ose.Extensions;
            List<OpenSearchDescriptionUrl> urls = new List<OpenSearchDescriptionUrl>();

            NameValueCollection parameters = collection.GetOpenSearchParameters(collection.DefaultMimeType);

            UriBuilder searchUrl = new UriBuilder(string.Format("{0}/catalogue/{1}/{2}/search", RootWebConfig.AppSettings.Settings["baseUrl"].Value, collection.IndexName, collection.TypeName));
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
                                                      "search"));

            }
            searchUrl = new UriBuilder(string.Format("{0}/catalogue/{1}/{2}/search", RootWebConfig.AppSettings.Settings["baseUrl"].Value, collection.IndexName, collection.TypeName));
            urls.Add(new OpenSearchDescriptionUrl("application/opensearchdescription+xml", 
                                                  searchUrl.ToString(),
                                                  "self"));
            osd.Url = urls.ToArray();

            return osd;
        }
    }
}