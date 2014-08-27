using System;
using Terradue.ElasticCas.Service;
using Terradue.ElasticCas.Request;
using Terradue.ElasticCas.Model;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch;
using ServiceStack.Common.Web;
using System.Web;
using System.Collections.Specialized;
using Terradue.ServiceModel.Syndication;
using System.Collections.ObjectModel;

namespace Terradue.ElasticCas.Service {
    public class OpenSearchService {

        public static object Query(IElasticDocumentCollection collection, NameValueCollection parameters) {

            OpenSearchEngine ose = collection.GetOpenSearchEngine(parameters);

            // special case for description
            if (HttpContext.Current.Request.AcceptTypes != null && HttpContext.Current.Request.AcceptTypes[0] == "application/opensearchdescription+xml" || parameters["format"] == "description") {
                return new HttpResult(collection.GetProxyOpenSearchDescription(), "application/opensearchdescription+xml");
            }

            Type type = OpenSearchFactory.ResolveTypeFromRequest(HttpContext.Current.Request, ose);

            var result = ose.Query(collection, parameters, type );

            OpenSearchFactory.ReplaceSelfLinks(result, collection.EntrySelfLinkTemplate, result.Result.ContentType);   
            OpenSearchFactory.ReplaceOpenSearchDescriptionLinks(result);
            OpenSearchFactory.ReplaceId(result);

            result.Result.Title = string.Format("Result for OpenSearch query over type {0} in index {1}", collection.TypeName, collection.IndexName);


            return new HttpResult(result.Result.SerializeToString(), result.Result.ContentType);
        }

    }
}

