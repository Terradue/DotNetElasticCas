using System;
using ServiceStack.ServiceHost;
using Terradue.ElasticCas.Model;
using Nest;

namespace Terradue.ElasticCas.Request {


	[Route("/catalogue/{IndexName}", "PUT")]
    public class CreateIndexRequest : Index, IReturn<IndexStatus>{

		
	}

    [Route("/catalogue/{IndexName}", "DELETE")]
    public class DeleteIndexRequest : IReturn<string>{

        public string IndexName { get; set; }
    }
}

