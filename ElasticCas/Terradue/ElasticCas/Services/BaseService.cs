using System;
using ServiceStack;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceHost;
using PlainElastic.Net;
using log4net;
using PlainElastic.Net.Serialization;

namespace Terradue.ElasticCas {
	public class BaseService : Service {
		protected System.Configuration.Configuration rootWebConfig;
		protected System.Configuration.KeyValueConfigurationElement esHost;
		protected System.Configuration.KeyValueConfigurationElement esPort;
		protected ElasticConnection esConnection;
		protected readonly ILog logger;

		protected IJsonSerializer serializer;

		public BaseService() {

			// Init Log
			logger = LogManager.GetLogger(this.GetType().Name);

			// Get web config
			rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(null);
			if (rootWebConfig.AppSettings.Settings.Count > 0) {
				esHost = rootWebConfig.AppSettings.Settings["esHost"];
				esPort = rootWebConfig.AppSettings.Settings["esPort"];
				if (esHost != null)
					logger.InfoFormat("Using ElasticSearch Host : {0}", esHost);
				else {
					esHost.Value = "localhost";
					logger.InfoFormat("No ElasticSearch Host specified, using default : {0}", esHost);
				}
			}

			esConnection = new ElasticConnection(esHost.Value, int.Parse(esPort.Value));

            serializer = new ServiceStackJsonSerializer();


		}
	}
}

