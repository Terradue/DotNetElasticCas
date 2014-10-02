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
using PlainElastic.Net;
using PlainElastic.Net.Utils;
using ServiceStack.Text;

namespace Terradue.ElasticCas.Services {
    [Api("Type Ingestion Service")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class TypesIngestionService : BaseService {
        public TypesIngestionService() : base("Type Ingestion Service") {

        }

        [AddHeader(ContentType = ContentType.Json)]
        public object Post(IngestionRequest request) {

            string response = "";

            try {

                IElasticDocumentCollection collection = ElasticCasFactory.GetElasticDocumentCollectionByTypeName(request.TypeName);

                if (collection == null) {
                    var gcollection = new GenericJsonCollection();
                    gcollection.TypeName = request.TypeName;
                    collection = gcollection;
                }

                OpenSearchEngine ose = collection.GetOpenSearchEngine(new NameValueCollection());

                IOpenSearchEngineExtension osee = ose.GetExtensionByContentTypeAbility(Request.ContentType);
                if (osee == null)
                    throw new NotImplementedException(string.Format("No OpenSearch extension found for reading {0}", Request.ContentType));

                MemoryOpenSearchResponse payload = new MemoryOpenSearchResponse(Request.GetRawBody(), Request.ContentType);

                IOpenSearchResultCollection results = osee.ReadNative(payload);

                OpenSearchFactory.RemoveLinksByRel(ref results,"alternate");
                OpenSearchFactory.RemoveLinksByRel(ref results,"self");
                OpenSearchFactory.RemoveLinksByRel(ref results,"search");

                Collection<IElasticDocument> docs = collection.CreateFromOpenSearchResultCollection(results);

                string command = new BulkCommand(index: request.IndexName, type: request.TypeName);

                string bulkJson = 
                    new BulkBuilder(serializer)
                    .BuildCollection(docs,
                                     (builder, item) => builder.Index(data: item, id: item.Id.Replace('.', '_')));

                response = esConnection.Post(command, bulkJson);
            } catch (Exception e) {
                throw e;
            }
            return response;
        }

        [AddHeader(ContentType = ContentType.Json)]
        public object Post(TypeImportRequest request) {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();
            ose.DefaultTimeOut = 60000;

            OpenSearchUrl url = new OpenSearchUrl(request.url); 

            IOpenSearchable entity = new GenericOpenSearchable(url, ose);

            IElasticDocumentCollection collection = ElasticCasFactory.GetElasticDocumentCollectionByTypeName(request.TypeName);

            if (collection == null) {
                var gcollection = new GenericJsonCollection();
                gcollection.TypeName = request.TypeName;
                collection = gcollection;
            }

            IOpenSearchResult osres = ose.Query(entity, new NameValueCollection());
            OpenSearchFactory.RemoveLinksByRel(ref osres, "alternate");
            OpenSearchFactory.RemoveLinksByRel(ref osres, "via");
            OpenSearchFactory.RemoveLinksByRel(ref osres, "self");
            OpenSearchFactory.RemoveLinksByRel(ref osres, "search");

            Collection<IElasticDocument> documents = collection.CreateFromOpenSearchResultCollection(osres.Result);

            string command = new BulkCommand(index: request.IndexName, type: request.TypeName);

            string bulkJson = 
                new BulkBuilder(serializer)
                    .BuildCollection(documents,
                                     (builder, item) => builder.Index(data: item, id: item.Id.Replace('.', '_'))
					                 // You can apply any custom logic here
					                 // to generate Indexes, Creates or Deletes.
                );

            string response;

            try {
                response = esConnection.Post(command, bulkJson);
            } catch (Exception e) {
                throw e;
            }
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

            string response;
            string result;

            response = esConnection.Get(UrlBuilder.BuildUrlPath(new string[] {
                request.IndexName,
                request.TypeName,
                request.Id
            }));
            var resultObject = JsonObject.Parse(response);
            result = resultObject.GetUnescaped("_source");


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

