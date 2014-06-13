using System;
using ServiceStack;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceHost;
using PlainElastic.Net;
using ServiceStack.Text;
using PlainElastic.Net.Serialization;
using ServiceStack.Common.Web;
using Terradue.ElasticCas.Request;

namespace Terradue.ElasticCas {
	[Api("Index Service")]
	[Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
	          EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
	public class IndexService : BaseService {
		public IndexService() : base() {

		}

		[AddHeader(ContentType=ContentType.Json)]
        public object Get(GetIndexRequest request) {
            string command = Commands.GetMapping(request.IndexName);
			string response;

			try {
				response = esConnection.Get(command);
			} catch (Exception e) {
				throw e;
			}

			return response;
		}

		[AddHeader(ContentType=ContentType.Json)]
		public object Put(CreateIndexRequest request) {
			ElasticCasFactory casFactory = new ElasticCasFactory(esConnection);
            return casFactory.CreateCatalogueIndex(request.IndexName, request.TypeNames);
		}
	}
}

