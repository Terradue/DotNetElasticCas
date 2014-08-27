using System;
using ServiceStack.ServiceHost;
using PlainElastic.Net.IndexSettings;
using PlainElastic.Net.Serialization;
using PlainElastic.Net;
using Terradue.ElasticCas.Model;

namespace Terradue.ElasticCas.Request {

	    
	public class GetIndexRequest {

		public string IndexName { get; set; }

    }

	[Route("/catalogue/{IndexName}", "PUT")]
    public class CreateIndexRequest : Index, IReturn<OperationResult>{

		
	}

    [Route("/catalogue/{IndexName}", "DELETE")]
    public class DeleteIndexRequest : IReturn<OperationResult>{

        public string IndexName { get; set; }
    }
}

