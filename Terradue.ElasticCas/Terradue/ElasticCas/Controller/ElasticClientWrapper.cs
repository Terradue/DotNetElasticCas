using System;
using Nest;
using Terradue.ElasticCas.Model;

namespace Terradue.ElasticCas.Controller {
    public class ElasticClientWrapper : ElasticClient {

        private static string _connectionString = Settings.ElasticSearchServer;

        private static ConnectionSettings _settings = 
            new ConnectionSettings(new Uri(_connectionString))
                .SetDefaultIndex(Settings.Alias)
                .ThrowOnElasticsearchServerExceptions();

        public ElasticClientWrapper() : base(_settings) {

            /*_settings.AddContractJsonConverters(t => typeof(IElasticDocument).IsAssignableFrom(t)
                                                ? new NestServiceStackJsonSerializer()
                                                : null);*/

        }


        public static ConnectionSettings ConnectionSettings {
            get {
                return _settings;
            }
        }
    }
}

