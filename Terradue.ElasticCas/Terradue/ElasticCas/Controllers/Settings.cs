using System;
using System.Configuration;

namespace Terradue.ElasticCas.Controllers {
    public class Settings {
        public static string Alias
        {
            get
            {
                return "elasticcas";
            }
        }

        public static string ElasticSearchServer
        {
            get
            {
                if (Exists())
                    return ConfigurationManager.AppSettings["ElasticsearchServer"];
                else
                    return "http://localhost:9200";
            }
        }

        public static bool Exists(){
            return !(ConfigurationManager.AppSettings["ElasticsearchServer"] == null);
            
        }

        public static Uri BaseUrl{
            get {
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["baseUrl"]))
                    return new Uri(ConfigurationManager.AppSettings["baseUrl"]);
                else
                    return new Uri("http://localhost:8081/");
            }
        }
    }
}

