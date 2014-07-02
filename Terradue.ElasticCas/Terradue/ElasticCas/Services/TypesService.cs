using System;
using ServiceStack;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceHost;
using PlainElastic.Net;
using ServiceStack.Text;
using PlainElastic.Net.Serialization;
using ServiceStack.Common.Web;
using PlainElastic.Net.Utils;
using Terradue.OpenSearch;
using Terradue.OpenSearch.Engine.Extensions;
using System.Security.Policy;
using System.Web;
using System.Collections.Specialized;
using System.Net;
using Terradue.OpenSearch.Result;
using Terradue.ElasticCas.Request;
using Terradue.OpenSearch.Engine;
using Terradue.ElasticCas.Model;
using Terradue.OpenSearch.GeoJson.Extensions;
using System.Collections.ObjectModel;
using Terradue.OpenSearch.Response;

namespace Terradue.ElasticCas.Service {
    [Api("Type Ingestion Service")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class TypesIngestionService : BaseService {
        public TypesIngestionService() : base("Type Ingestion Service") {

        }

        [AddHeader(ContentType = ContentType.Json)]
        public object Post(IElasticDocumentCollection collection) {

            string command = new BulkCommand(index: collection.IndexName, type: collection.TypeName);

            string bulkJson = 
                new BulkBuilder(serializer)
                    .BuildCollection(collection.Items,
                                     (builder, item) => builder.Index(data: item, id: item.Id.Replace('.', '_'))
                );

            string response;

            try {
                response = esConnection.Post(command, bulkJson);
            } catch (Exception e) {
                throw e;
            }
            return response;
        }

        [AddHeader(ContentType = ContentType.Json)]
        public object Post(SingleIngestionRequest request) {

            string response = "";

            try {
                OpenSearchEngine ose = new OpenSearchEngine();
                ose.LoadPlugins();

                IOpenSearchEngineExtension osee = ose.GetExtensionByDiscoveryContentType(Request.ContentType);
                if (osee == null)
                    throw new NotImplementedException(string.Format("No OpenSearch extension found for reading {0}", Request.ContentType));

                IElasticDocumentCollection documents = ElasticCasFactory.GetElasticDocumentCollectionByTypeName(request.TypeName);

                if (documents == null)
                    throw new InvalidTypeModelException(request.TypeName, string.Format("Type '{0}' is not found in the type extensions. Check that plugins are loaded", request.TypeName));

                MemoryOpenSearchResponse payload = new MemoryOpenSearchResponse(Request.InputStream, Request.ContentType);

                IOpenSearchResultCollection results = osee.ReadNative(payload);

                Collection<IElasticDocument> docs = documents.CreateFromOpenSearchResultCollection(results);

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

            if (collection == null)
                throw new InvalidTypeModelException(request.TypeName, string.Format("Type '{0}' is not found in the type extensions. Check that plugins are loaded", request.TypeName));

            IOpenSearchResult osres = ose.Query(entity, new NameValueCollection());
            OpenSearchFactory.RemoveLinksByRel(osres, "alternate");
            OpenSearchFactory.RemoveLinksByRel(osres, "via");
            OpenSearchFactory.RemoveLinksByRel(osres, "self");
            OpenSearchFactory.RemoveLinksByRel(osres, "search");

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
    }
}

