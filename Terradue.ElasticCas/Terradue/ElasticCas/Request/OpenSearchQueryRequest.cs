using System;
using ServiceStack;
using ServiceStack.ServiceHost;

namespace Terradue.ElasticCas.Request {

    [Route("/catalogue/{IndexName}/{TypeName}/search")]
    public class OpenSearchQueryRequest {
        
        public string IndexName {get; set;}

        public string TypeName {get; set;}
    }
}

