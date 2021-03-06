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
    [Api("Type Ingestion Service")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class TypesIngestionService : BaseService {
        public TypesIngestionService() : base("Type Ingestion Service") {

        }

        [AddHeader(ContentType = ContentType.Json)]
        public HttpResult Post(IngestionRequest request) {

            IOpenSearchableElasticType type = ecf.GetOpenSearchableElasticTypeByNameOrDefault(request.IndexName, request.TypeName);

            var response = Ingest(type, Request);

            return new HttpResult(response, "application/json");

        }

        public BulkOperationsResponse Ingest(IOpenSearchableElasticType type, IHttpRequest request) {

            OpenSearchEngine ose = type.GetOpenSearchEngine(new NameValueCollection());

            IOpenSearchEngineExtension osee = ose.GetExtensionByContentTypeAbility(request.ContentType);
            if (osee == null)
                throw new NotImplementedException(string.Format("No OpenSearch extension found for reading {0}", Request.ContentType));

            MemoryOpenSearchResponse payload = new MemoryOpenSearchResponse(request.GetRawBody(), request.ContentType);

            IOpenSearchResultCollection results = osee.ReadNative(payload);

            return Ingest(type, results);

        }

        public BulkOperationsResponse Ingest(IOpenSearchableElasticType type, IOpenSearchResultCollection results) {

            OpenSearchFactory.RemoveLinksByRel(ref results, "self");
            OpenSearchFactory.RemoveLinksByRel(ref results, "search");

            IElasticCollection docs = type.FromOpenSearchResultCollection(results);

            BulkRequest bulkRequest = new BulkRequest() {
                Refresh = true,
                Consistency = Consistency.One,
                Index = type.Index,
                Type = type.Type,
                Operations = new List<IBulkOperation>()
            };

            RootObjectMapping currentMapping = null;

            try { 
                var mappingResponse = client.GetMapping<IElasticType>(g => g.Index(type.Index.Name).Type(type.Type.Name));
                currentMapping = mappingResponse.Mapping;
            } catch (Exception) {
            }

            var rootObjectMapping = type.GetRootMapping();

            if (!rootObjectMapping.Equals(currentMapping)) {
                client.Map<IElasticType>(m => m.Index(type.Index.Name).Type(type.Type.Name));
            }

            foreach (var doc in docs.ElasticItems) {
                var bulkIndexOperation = new BulkIndexOperation<IElasticItem>(doc);
                bulkIndexOperation.Id = ((IOpenSearchResultItem)doc).Identifier;
                bulkIndexOperation.Type = type.Type.Name;
                var bulkOp = bulkIndexOperation;
                bulkRequest.Operations.Add(bulkOp);
            }

            var response = client.Bulk(bulkRequest);

            BulkOperationsResponse ingestionResponse = new BulkOperationsResponse();
            foreach (var item in response.Items) {
                if (!item.IsValid) {
                    ingestionResponse.Errors++;
                    continue;
                }
                if (item.Version == "1")
                    ingestionResponse.Added++;
                else
                    ingestionResponse.Updated++;

            }

            return ingestionResponse;
        }

        [AddHeader(ContentType = ContentType.Json)]
        public object Post(TypeImportRequest request) {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();
            ose.DefaultTimeOut = 60000;

            OpenSearchUrl url = new OpenSearchUrl(request.url); 

            IOpenSearchable entity = new GenericOpenSearchable(url, ose);

            IOpenSearchableElasticType type = ecf.GetOpenSearchableElasticTypeByNameOrDefault(request.IndexName, request.TypeName);

            IOpenSearchResultCollection osres = ose.Query(entity, new NameValueCollection());
            OpenSearchFactory.RemoveLinksByRel(ref osres, "alternate");
            OpenSearchFactory.RemoveLinksByRel(ref osres, "via");
            OpenSearchFactory.RemoveLinksByRel(ref osres, "self");
            OpenSearchFactory.RemoveLinksByRel(ref osres, "search");

            IElasticCollection documents = type.FromOpenSearchResultCollection(osres);

            BulkRequest bulkRequest = new BulkRequest() {
                Refresh = true,
                Consistency = Consistency.One,
                Index = request.IndexName
            };

            foreach (var doc in documents.Items) {
                bulkRequest.Operations.Add(new BulkIndexOperation<IElasticItem>((IElasticItem)doc) { Id = doc.Id });
            }

            var response = client.Bulk(bulkRequest);

            return response;
        }
    }

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

            return OpenSearchService.Query(type, parameters);
        }

    }

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
            var results = OpenSearchService.QueryResult(type, parameters);

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

