using System;
using ServiceStack.ServiceHost;
using System.Runtime.Serialization;
using System.Collections.Generic;
using ServiceStack.Text;
using ServiceStack.Text.Json;
using Terradue.OpenSearch.Result;
using Terradue.ServiceModel.Syndication;
using Terradue.ElasticCas.Model;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.WebHost.Endpoints.Support;
using Terradue.ElasticCas.Services;
using System.Collections.Specialized;
using System.IO;

namespace Terradue.ElasticCas.Request {


    [Route("/catalogue/{IndexName}/{TypeName}", "POST")]
    public class IngestionRequest {
        public string IndexName { get; set; }

        public string TypeName { get; set; }
    }


    [Route("/catalogue/{IndexName}/{TypeName}/{Id}", "GET")]
    public class TypeGetRequest : IReturn<IElasticItem> {
        public string IndexName { get; set; }

        public string TypeName { get; set; }

        public string Id { get; set; }
    }

    [Route("/catalogue/{IndexName}/{TypeName}/_import", "POST")]
    public class TypeImportRequest : IReturn<IElasticCollection> {
        public string IndexName { get; set; }

        public string TypeName { get; set; }

        public string url { get; set; }

        public Dictionary<string, object> parameters { get; set; } 
    }

    [Route("/catalogue/{IndexName}/{TypeName}/{Id}", "DELETE")]
    public class TypeDeleteRequest : IReturn<IElasticItem> {
        public string IndexName { get; set; }

        public string TypeName { get; set; }

        public string Id { get; set; }
    }
}

