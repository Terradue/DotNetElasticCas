using System;
using ServiceStack.ServiceHost;
using PlainElastic.Net.IndexSettings;
using PlainElastic.Net.Serialization;
using PlainElastic.Net;

namespace Terradue.ElasticCas.Request {

    [Route("/types", "GET")]
    public class TypeListRequest {

		public string IndexName { get; set; }

    }
}
