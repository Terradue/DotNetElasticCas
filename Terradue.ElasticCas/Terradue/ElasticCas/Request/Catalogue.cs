using System;
using ServiceStack.ServiceHost;

namespace Terradue.ElasticCas.Request {

    [Route("/types", "GET")]
    public class TypeListRequest {

		public string IndexName { get; set; }

    }
}
