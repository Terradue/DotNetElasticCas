using System;
using ServiceStack;
using ServiceStack.ServiceHost;
using System.Collections.Specialized;

namespace Terradue.ElasticCas.Request {

    [Route("/catalogue/{IndexName}/{TypeName}/search")]
    public class OpenSearchQueryRequest {
        
        public string IndexName {get; set;}

        public string TypeName {get; set;}

        public NameValueCollection AdditionalParameters{get; set;}
    }
}

