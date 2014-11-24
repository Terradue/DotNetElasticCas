using System;
using ServiceStack;
using Terradue.ElasticCas.Model;
using Terradue.ElasticCas.Request;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.Common.Web;
using Terradue.ElasticCas.Controller;
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

namespace Terradue.ElasticCas.Services {
    [Api("Type Ingestion Service")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class TypesIngestionService : BaseService {
        public TypesIngestionService() : base("Type Ingestion Service") {

        }

        [AddHeader(ContentType = ContentType.Json)]
        public IEnumerable<BulkOperationResponseItem> Post(IngestionRequest request) {


            IElasticDocument document = ElasticCasFactory.GetElasticDocumentByTypeName(request.TypeName);

            if (document == null) {
                var gdocument = new GenericJson();
                gdocument.TypeName = request.TypeName;
                document = gdocument;
            }

            OpenSearchEngine ose = document.GetOpenSearchEngine(new NameValueCollection());

            IOpenSearchEngineExtension osee = ose.GetExtensionByContentTypeAbility(Request.ContentType);
            if (osee == null)
                throw new NotImplementedException(string.Format("No OpenSearch extension found for reading {0}", Request.ContentType));

            MemoryOpenSearchResponse payload = new MemoryOpenSearchResponse(Request.GetRawBody(), Request.ContentType);

            IOpenSearchResultCollection results = osee.ReadNative(payload);

            OpenSearchFactory.RemoveLinksByRel(ref results, "self");
            OpenSearchFactory.RemoveLinksByRel(ref results, "search");

            Collection<IElasticDocument> docs = document.GetContainer().CreateFromOpenSearchResultCollection(results);

            BulkRequest bulkRequest = new BulkRequest() {
                Refresh = true,
                Consistency = Consistency.One,
                Index = request.IndexName,
                Type = request.TypeName,
                Operations = new List<IBulkOperation>()
            };

            RootObjectMapping currentMapping = null;

            try { 
                var mappingResponse = client.GetMapping<IElasticDocument>(g => g.Index(request.IndexName).Type(request.TypeName));
                currentMapping = mappingResponse.Mapping;
            } catch ( Exception e ) {}

            RootObjectMapping typeMapping = document.GetMapping();

            if (!typeMapping.Equals(currentMapping)) {
                IPutMappingRequest putMapping = new PutMappingRequest(request.IndexName, request.TypeName);
                putMapping.Mapping = typeMapping;
                client.Map(putMapping);
            }

            foreach (var doc in docs) {
                var bulkIndexOperation = new BulkIndexOperation<IElasticDocument>(doc);
                bulkIndexOperation.Id = ((IOpenSearchResultItem)doc).Identifier;
                bulkIndexOperation.Type = request.TypeName;
                var bulkOp = bulkIndexOperation;
                bulkRequest.Operations.Add(bulkOp);
            }

            var response = client.Bulk(bulkRequest);

            return response.Items;
        }

        [AddHeader(ContentType = ContentType.Json)]
        public object Post(TypeImportRequest request) {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();
            ose.DefaultTimeOut = 60000;

            OpenSearchUrl url = new OpenSearchUrl(request.url); 

            IOpenSearchable entity = new GenericOpenSearchable(url, ose);

            IElasticDocument document = ElasticCasFactory.GetElasticDocumentByTypeName(request.TypeName);

            if (document == null) {
                var gdocument = new GenericJson();
                gdocument.TypeName = request.TypeName;
                document = gdocument;
            }

            IOpenSearchResult osres = ose.Query(entity, new NameValueCollection());
            OpenSearchFactory.RemoveLinksByRel(ref osres, "alternate");
            OpenSearchFactory.RemoveLinksByRel(ref osres, "via");
            OpenSearchFactory.RemoveLinksByRel(ref osres, "self");
            OpenSearchFactory.RemoveLinksByRel(ref osres, "search");

            Collection<IElasticDocument> documents = document.GetContainer().CreateFromOpenSearchResultCollection(osres.Result);

            BulkRequest bulkRequest = new BulkRequest() {
                Refresh = true,
                Consistency = Consistency.One,
                Index = request.IndexName
            };

            foreach (var doc in documents) {
                bulkRequest.Operations.Add(new BulkIndexOperation<IElasticDocument>(doc) { Id = doc.Id });
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


            var response = client.Get<string>(request.Id, request.IndexName, request.TypeName);

            var result = JsonObject.Parse(response.Source);

            return result;
        }

        public object Get(TypeNamespacesRequest request) {

            IElasticDocument document = ElasticCasFactory.GetElasticDocumentByTypeName(request.TypeName);

            if (document == null) {
                var gdocument = new GenericJson();
                gdocument.TypeName = request.TypeName;
                document = gdocument;
            }
                
            return document.GetTypeNamespaces();
        }
    }
}

