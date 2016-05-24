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


    [Api("Type Edition Service")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class TypeEditionService : BaseService {
        public TypeEditionService() : base("Type Edition Service") {

        }

        [AddHeader(ContentType = ContentType.Json)]
        public object Delete(TypeDeleteRequest request) {

            IOpenSearchableElasticType type = ecf.GetOpenSearchableElasticTypeByNameOrDefault(request.IndexName, request.TypeName);
            NameValueCollection parameters = new NameValueCollection();
            parameters.Set("uid", request.Id);
            var results = new OpenSearchService().QueryResult(type, parameters);

            var response = Delete(type, results);

            return new HttpResult(response, "application/json");
        }

        public BulkOperationsResponse Delete(IOpenSearchableElasticType type, IOpenSearchResultCollection results) {

            BulkRequest bulkRequest = new BulkRequest() {
                Refresh = true,
                Consistency = Consistency.One,
                Index = type.Index,
                Type = type.Type,
                Operations = new List<IBulkOperation>()
            };

            foreach (var doc in results.Items) {
                var bulkDeleteOperation = new BulkDeleteOperation<IElasticItem>(doc.Identifier);
                bulkDeleteOperation.Type = type.Type.Name;
                var bulkOp = bulkDeleteOperation;
                bulkRequest.Operations.Add(bulkOp);
            }

            var response = client.Bulk(bulkRequest);

            BulkOperationsResponse ingestionResponse = new BulkOperationsResponse();
            foreach (var item in response.Items) {
                if (!item.IsValid) {
                    ingestionResponse.Errors++;
                } else {
                    ingestionResponse.Deleted++;
                }

            }

            return ingestionResponse;
        }
    }
}

