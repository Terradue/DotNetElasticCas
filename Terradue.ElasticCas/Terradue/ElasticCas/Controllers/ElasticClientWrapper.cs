using System;
using Nest;
using Terradue.ElasticCas.Model;
using Newtonsoft.Json.Converters;

namespace Terradue.ElasticCas.Controllers {
    public class ElasticClientWrapper : ElasticClient {

        private static string _connectionString = Settings.ElasticSearchServer;

        private static ConnectionSettings _connectionsettings = 
            new ConnectionSettings(new Uri(_connectionString))
                .SetDefaultIndex(Settings.Alias)
                .ThrowOnElasticsearchServerExceptions()
                .ExposeRawResponse()
                .AddContractJsonConverters(t => typeof(Enum).IsAssignableFrom(t)
                                                               ? new StringEnumConverter()
                                                               : null);

        public ElasticClientWrapper() : base(_connectionsettings) {

        }


        public static ConnectionSettings ConnectionSettings {
            get {
                return _connectionsettings;
            }
        }
    }
}

