using System;
using Terradue.ElasticCas.Model;
using PlainElastic.Net;
using ServiceStack.ServiceHost;
using Terradue.ElasticCas.Request;
using ServiceStack.ServiceInterface;
using ServiceStack.Common.Web;
using PlainElastic.Net.Queries;
using Terradue.OpenSearch.Engine;

namespace Terradue.ElasticCas.Service {

    [Api("OpenSearch Query Service")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class OpenSearchQueryRequestService : BaseService {

        public OpenSearchQueryRequestService() : base("OpenSearch Query Service"){
        }

        public object Get(OpenSearchQueryRequest request) {

            IElasticDocumentCollection collection = ElasticCasFactory.GetDtoByTypeName(request.TypeName);
            collection.IndexName = request.IndexName;
            if (collection == null) {
                throw new InvalidTypeModelException(request.TypeName, string.Format("Type '{0}' is not found in the type extensions. Check that plugins are loaded", request.TypeName));
            }

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            string format = Request.QueryString["format"] == null ? "Atom" : Request.QueryString["format"];

            var result = ose.Query(collection, Request.QueryString, format );

            return new HttpResult(result.Result.SerializeToString(), result.Result.ContentType);
        }
    }
}

