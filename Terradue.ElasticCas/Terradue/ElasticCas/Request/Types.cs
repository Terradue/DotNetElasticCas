using System;
using ServiceStack.ServiceHost;
using System.Runtime.Serialization;
using System.Collections.Generic;
using PlainElastic.Net.Serialization;
using ServiceStack.Text;
using ServiceStack.Text.Json;
using Terradue.OpenSearch.Result;
using Terradue.ServiceModel.Syndication;
using Terradue.ElasticCas.Model;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.WebHost.Endpoints.Support;
using Terradue.ElasticCas.Service;
using System.Collections.Specialized;
using System.IO;

namespace Terradue.ElasticCas.Request {


    [Route("/catalogue/{IndexName}/{TypeName}", "POST")]
    public class IngestionRequest : IReturn<IElasticDocument> {
        public string IndexName { get; set; }

        public string TypeName { get; set; }
    }


    [Route("/catalogue/{IndexName}/{TypeName}/{Id}", "GET")]
    public class TypeGetRequest : IReturn<IElasticDocument> {
        public string IndexName { get; set; }

        public string TypeName { get; set; }

        public string Id { get; set; }
    }

    [Route("/catalogue/{IndexName}/{TypeName}/_import", "POST")]
    public class TypeImportRequest : IReturn<IElasticDocumentCollection> {
        public string IndexName { get; set; }

        public string TypeName { get; set; }

        public string url { get; set; }

        public Dictionary<string, object> parameters { get; set; } 
    }

    [Route("/catalogue/{IndexName}/{TypeName}/_namespaces", "GET")]
    public class TypeNamespacesRequest : IReturn<NameValueCollection> {
        public string IndexName { get; set; }

        public string TypeName { get; set; }
    }
}

