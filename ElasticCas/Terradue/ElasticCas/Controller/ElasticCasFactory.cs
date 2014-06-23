using System;
using PlainElastic.Net;
using System.Xml;
using ServiceStack.Text;
using PlainElastic.Net.Serialization;
using PlainElastic.Net.Mappings;
using System.Collections.Generic;
using Terradue.GeoJson.Feature;
using Terradue.GeoJson.CoordinateReferenceSystem;
using Mono.Addins;
using Terradue.ElasticCas.Model;
using ServiceStack.ServiceHost;
using ServiceStack.WebHost.Endpoints.Utils;
using ServiceStack.WebHost.Endpoints;
using Terradue.ElasticCas.Request;
using PlainElastic.Net.IndexSettings;
using log4net;

namespace Terradue.ElasticCas {

    public class ElasticCasFactory {
        ElasticConnection esConnection;
        internal System.Configuration.Configuration RootWebConfig { get; set; }
        internal System.Configuration.KeyValueConfigurationElement EsHost { get; set; }
        internal System.Configuration.KeyValueConfigurationElement EsPort { get; set; }
        protected readonly ILog logger;

        internal ElasticCasFactory(string name) {

            // Init Log
            logger = LogManager.GetLogger(name);

            // Get web config
            RootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(null);
            if (RootWebConfig.AppSettings.Settings.Count > 0) {
                EsHost = RootWebConfig.AppSettings.Settings["esHost"];
                EsPort = RootWebConfig.AppSettings.Settings["esPort"];
                if (EsHost != null)
                    logger.InfoFormat("Using ElasticSearch Host : {0}", EsHost);
                else {
                    EsPort.Value = "localhost";
                    logger.InfoFormat("No ElasticSearch Host specified, using default : {0}", EsHost);
                }
            }

            esConnection = new ElasticConnection(EsHost.Value, int.Parse(EsPort.Value));
            logger.InfoFormat("New ElasticSearch Connection from {0}", name);
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
				}
				else {
                    throw new InvalidOperationException(string.Format("'{0}' index already exists and cannot be overriden without data loss", indexName));
				}
			}

			string command = Commands.CreateIndex(indexName);

            string jsondata = new IndexSettingsBuilder().Analysis(a => a.Analyzer(an => an.Custom("default", custom => custom
                                                                                                  .Tokenizer(DefaultTokenizers.standard)
                                                                                                  .Filter(DefaultTokenFilters.standard)))).Build();

            esConnection.Put(command, jsondata);

            // Init mapp    ings for each types declared

            // If no types is declared
            if (typeNames == null || typeNames.Length == 0) {
                foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IElasticDocument))) {
                    IElasticDocument doc = (IElasticDocument)node.CreateInstance();
                    command = Commands.PutMapping(indexName, doc.TypeName);
                    jsondata = doc.GetMapping();

                    esConnection.Put(command, jsondata);
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

            return esConnection.Get(Commands.GetMapping(new string[]{indexName}, typeNames));

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

            foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IElasticDocument))) {
                IElasticDocument doc = (IElasticDocument)node.CreateInstance();
                Types.SetType(application, doc);
            }

        }

        public static IElasticDocumentCollection GetDtoByTypeName(string typeName, Dictionary<string, object> parameters = null) {
            foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IElasticDocumentCollection))) {
                IElasticDocumentCollection docs = (IElasticDocumentCollection)node.CreateInstance();
                docs.Parameters = parameters;
                if (docs.TypeName == typeName) {
                    return docs;
                }
            }
            return null;
        }
	}
}