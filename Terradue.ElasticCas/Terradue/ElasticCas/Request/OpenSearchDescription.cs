using System;
using ServiceStack.ServiceHost;
using Terradue.OpenSearch;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using Terradue.OpenSearch.Schema;
using Terradue.ElasticCas.Model;

namespace Terradue.ElasticCas.Request {


    [Route("/catalogue/{IndexName}/{TypeName}/description", "GET")]
	public class OpenSearchDescriptionGetRequest {

		public string IndexName {get; set;}

        public string TypeName {get; set;}

    }
}

