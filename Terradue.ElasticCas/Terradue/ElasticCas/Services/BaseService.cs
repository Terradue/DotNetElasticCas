using System;
using ServiceStack;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceHost;
using log4net;
using Terradue.ElasticCas.Controllers;

namespace Terradue.ElasticCas.Services {
    public class BaseService : ServiceStack.ServiceInterface.Service {
		
        protected ElasticClientWrapper client;

        protected ElasticCasFactory ecf;
		
        public BaseService(string name) {

            ecf = new ElasticCasFactory(name);

            client = ecf.Client;


		}
	}
}

