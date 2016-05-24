using System;
using ServiceStack;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceHost;
using ServiceStack.Text;
using ServiceStack.Common.Web;
using Terradue.ElasticCas.Request;
using Terradue.ElasticCas.Responses;

namespace Terradue.ElasticCas.Services {
	[Api("Index Service")]
	[Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
	          EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
	public class IndexService : BaseService {

        public IndexService() : base("Index Service") {

		}

		[AddHeader(ContentType=ContentType.Json)]
        public IndexInformation Put(CreateIndexRequest request) {
            var indexInformation = ecf.CreateCatalogueIndex(request);
            client.Index(request.ToJson(), i => i.Index("ec_indices").Id(request.IndexName));

            return indexInformation;

		}

        [AddHeader(ContentType=ContentType.Json)]
        public Nest.IIndicesResponse Delete(DeleteIndexRequest request) {
            return client.DeleteIndex(d => d.Index(request.IndexName));
        }
	}
}

