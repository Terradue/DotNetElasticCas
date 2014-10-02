using System;
using ServiceStack;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceHost;
using PlainElastic.Net;
using ServiceStack.Text;
using PlainElastic.Net.Serialization;
using ServiceStack.Common.Web;
using Terradue.ElasticCas.Request;

namespace Terradue.ElasticCas.Services {
	[Api("Index Service")]
	[Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
	          EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
	public class IndexService : BaseService {

        public IndexService() : base("Index Service") {

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
            var result = ecf.CreateCatalogueIndex(request);
            var command = Commands.Index(request.IndexName, "ec_indices", request.IndexName);
            result = esConnection.Post(command, request.ToJson());

            return request;

		}

        [AddHeader(ContentType=ContentType.Json)]
        public object Delete(DeleteIndexRequest request) {
            return esConnection.Delete(Commands.Delete(request.IndexName));
        }
	}
}

