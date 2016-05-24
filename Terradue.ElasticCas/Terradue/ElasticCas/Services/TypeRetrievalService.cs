using System;
using ServiceStack;
using Terradue.ElasticCas.Model;
using Terradue.ElasticCas.Request;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.Common.Web;
using Terradue.ElasticCas.Controllers;
using Terradue.ElasticCas.Types;
using Terradue.OpenSearch.Engine;
using System.Collections.Specialized;
using Terradue.OpenSearch;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Result;
using System.Collections.ObjectModel;
using ServiceStack.Text;
using Nest;
using Elasticsearch.Net;
using System.Collections.Generic;
using Terradue.ElasticCas.OpenSearch;

namespace Terradue.ElasticCas.Services {

    [Api("Type Retrieval Service")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class TypeRetrievalService : BaseService {
        public TypeRetrievalService() : base("Type Retrieval Service") {

        }

        [AddHeader(ContentType = ContentType.Json)]
        public object Get(TypeGetRequest request) {

            IOpenSearchableElasticType type = ecf.GetOpenSearchableElasticTypeByNameOrDefault(request.IndexName, request.TypeName);

            NameValueCollection parameters = new NameValueCollection();
            parameters.Set("uid", request.Id);

            return new OpenSearchService().Query(type, parameters);
        }

    }
    
}
