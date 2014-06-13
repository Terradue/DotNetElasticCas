using System;
using ServiceStack.ServiceHost;
using PlainElastic.Net.IndexSettings;
using PlainElastic.Net.Serialization;
using PlainElastic.Net;

namespace Terradue.ElasticCas.Request {

	[Route("/catalogue/{IndexName}", "GET")]
	public class GetIndexRequest {

		public string IndexName { get; set; }

    }

	[Route("/catalogue/{IndexName}", "PUT")]
    public class CreateIndexRequest : IReturn<OperationResult>{

		public string IndexName { get; set; }

        public string[] TypeNames {
            get;
            set;
        }
	}
}

