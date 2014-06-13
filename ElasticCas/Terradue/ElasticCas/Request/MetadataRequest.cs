using System;
using ServiceStack.ServiceHost;
using Terradue.ElasticCas.Model;

namespace Terradue.ElasticCas.Request {

    [Route("/catalogue/{IndexName}/{TypeName}/metadata", "POST")]
    public class MetadataPostRequest : IReturn<Metadata>  {

        public string IndexName {get; set;}

        public string TypeName {get; set;}

    }

    [Route("/catalogue/{IndexName}/{TypeName}/metadata", "GET")]
    public class MetadataGetRequest : IReturn<Metadata> {

        public string IndexName {get; set;}

    }

}

