using System;
using ServiceStack;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceHost;
using PlainElastic.Net;
using ServiceStack.Text;
using PlainElastic.Net.Serialization;
using ServiceStack.Common.Web;
using PlainElastic.Net.Utils;
using Newtonsoft.Json;
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

namespace Terradue.ElasticCas.Services {
    [Api("DataSet Ingestion Service")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
           EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class TypesIngestionService : BaseService {
        public TypesIngestionService() : base() {

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

            string command = Commands.Index(request.doc.IndexName, request.doc.TypeName, request.doc.Id);

            string jsondata = request.doc.ToJson();
			string response;

			try {
				response = esConnection.Put(command, jsondata);
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

            IElasticDocumentCollection collection = ElasticCasFactory.GetDtoByTypeName(request.TypeName);

            if ( collection == null )
                throw new NotFoundException(string.Format("Type '{0}' is not found in the type extensions. Check that plugins are loaded", request.TypeName));

            IOpenSearchResult osres = ose.Query(entity,new NameValueCollection(),collection.GetOpenSearchResultType());

            Collection<IElasticDocument> documents = collection.CreateFromOpenSearchResult(osres.Result);

            OpenSearchFactory.RemoveLinksByRel(osres, "self");
            OpenSearchFactory.RemoveLinksByRel(osres, "alternate");
            OpenSearchFactory.RemoveLinksByRel(osres, "via");

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

	[Api("DataSet Retrieval Service")]
	[Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
	          EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
	public class DataSetRetrievalService : BaseService {
		public DataSetRetrievalService() : base() {

		}

        /*[AddHeader(ContentType = ContentType.Json)]
		public object Get(DatasetGetRequest request) {

			string response;
			string result;

			response = esConnection.Get(UrlBuilder.BuildUrlPath(new string[] {
				request.IndexName,
				"dataset",
				request.Id
			}));
			var resultObject = JsonObject.Parse(response);
			result = resultObject.GetUnescaped("_source");


			return result;
		}*/
    }
}

