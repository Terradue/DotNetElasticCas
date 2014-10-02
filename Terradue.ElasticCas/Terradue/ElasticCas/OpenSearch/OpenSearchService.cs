using System;
using Terradue.ElasticCas.Request;
using Terradue.ElasticCas.Model;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch;
using ServiceStack.Common.Web;
using System.Web;
using System.Collections.Specialized;
using Terradue.ServiceModel.Syndication;
using System.Collections.ObjectModel;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Schema;

namespace Terradue.ElasticCas.OpenSearch {
    public class OpenSearchService {

        public static IOpenSearchResult QueryResult(IElasticDocumentCollection collection, NameValueCollection parameters) {

            OpenSearchEngine ose = collection.GetOpenSearchEngine(parameters);

            Type type = OpenSearchFactory.ResolveTypeFromRequest(HttpContext.Current.Request, ose);

            var result = ose.Query(collection, parameters, type );

            OpenSearchFactory.ReplaceSelfLinks(collection, parameters, result.Result, collection.EntrySelfLinkTemplate, result.Result.ContentType);   
            OpenSearchFactory.ReplaceOpenSearchDescriptionLinks(collection, result.Result);

            result.Result.Title = string.Format("Result for OpenSearch query over type {0} in index {1}", collection.TypeName, collection.IndexName);

            return result;
        }

        public static HttpResult Query(IElasticDocumentCollection collection, NameValueCollection parameters) {

            // special case for description
            if (HttpContext.Current.Request.AcceptTypes != null && HttpContext.Current.Request.AcceptTypes[0] == "application/opensearchdescription+xml" || parameters["format"] == "description") {
                return new HttpResult(collection.GetProxyOpenSearchDescription(), "application/opensearchdescription+xml");
            }

            var result = QueryResult(collection, parameters);

            return new HttpResult(result.Result.SerializeToString(), result.Result.ContentType);
        }

        public static string EntrySelfLinkTemplate(IOpenSearchResultItem item, OpenSearchDescription osd, string mimeType) {
            if (item == null)
                return null;

            string identifier = item.Identifier;

            NameValueCollection nvc = new NameValueCollection();

            nvc.Set("q", string.Format("_id:{0}", item.Identifier));

            UriBuilder template = new UriBuilder(OpenSearchFactory.GetOpenSearchUrlByType(osd, mimeType).Template);
            string[] queryString = Array.ConvertAll(nvc.AllKeys, key => string.Format("{0}={1}", key, nvc[key]));
            template.Query = string.Join("&", queryString);
            return template.ToString();
        }
    }
}

