using System;
using ServiceStack.ServiceHost;
using Terradue.ElasticCas.Routes;
using ServiceStack.Text;

namespace Terradue.ElasticCas.Request {

    [Route("/api/{IndexName}/routes", "POST")]
    public class CreateNewRoute : DynamicOpenSearchRoute, IReturn<CreateNewRoute> {

        public string IndexName { get; set; }
    }

    [Route("/api/{IndexName}/{RouteId}", "GET")]
    public class DynamicRoute : JsonObject {

        public string IndexName { get; set; }

        public string RouteId { get; set; }
    }
}

