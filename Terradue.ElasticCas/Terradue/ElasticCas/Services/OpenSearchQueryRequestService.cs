using System;
using Terradue.ElasticCas.Model;
using PlainElastic.Net;
using ServiceStack.ServiceHost;
using Terradue.ElasticCas.Request;
using ServiceStack.ServiceInterface;
using ServiceStack.Common.Web;
using PlainElastic.Net.Queries;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch;
using System.Web;

namespace Terradue.ElasticCas.Service {

    [Api("OpenSearch Query Service")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class OpenSearchQueryRequestService : BaseService {

        public OpenSearchQueryRequestService() : base("OpenSearch Query Service"){
        }

        public object Get(OpenSearchQueryRequest request) {

            IElasticDocumentCollection collection = ElasticCasFactory.GetElasticDocumentCollectionByTypeName(request.TypeName);

            if (collection == null) {
                throw new InvalidTypeModelException(request.TypeName, string.Format("Type '{0}' is not found in the type extensions. Check that plugins are loaded", request.TypeName));
            }

            collection.IndexName = request.IndexName;

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            Type type = OpenSearchFactory.ResolveTypeFromRequest(HttpContext.Current.Request, ose);

            var result = ose.Query(collection, Request.QueryString, type );

            return new HttpResult(result.Result.SerializeToString(), result.Result.ContentType);
        }
    }
}

