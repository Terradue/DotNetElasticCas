using System;
using Nest;
using Terradue.ElasticCas.Model;

namespace Terradue.ElasticCas.Controllers {
    public class ElasticClientWrapper : ElasticClient {

        private static string _connectionString = Settings.ElasticSearchServer;

        private static ConnectionSettings _connectionsettings = 
            new ConnectionSettings(new Uri(_connectionString))
                .SetDefaultIndex(Settings.Alias)
                .ThrowOnElasticsearchServerExceptions()
                .ExposeRawResponse();

        public ElasticClientWrapper() : base(_connectionsettings) {

            /*_settings.AddContractJsonConverters(t => typeof(IElasticDocument).IsAssignableFrom(t)
                                                ? new NestServiceStackJsonSerializer()
                                                : null);*/

        }


        public static ConnectionSettings ConnectionSettings {
            get {
                return _connectionsettings;
            }
        }
    }
}

