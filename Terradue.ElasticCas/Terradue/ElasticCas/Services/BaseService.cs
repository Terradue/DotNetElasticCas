using System;
using ServiceStack;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceHost;
using PlainElastic.Net;
using log4net;
using PlainElastic.Net.Serialization;
using Terradue.ElasticCas.Controller;

namespace Terradue.ElasticCas.Services {
    public class BaseService : ServiceStack.ServiceInterface.Service {
		
		protected ElasticConnection esConnection;

        protected ElasticCasFactory ecf;
		

		protected IJsonSerializer serializer;

        public BaseService(string name) {

            ecf = new ElasticCasFactory(name);

            esConnection = ecf.EsConnection;

            serializer = new ServiceStackJsonSerializer();


		}
	}
}

