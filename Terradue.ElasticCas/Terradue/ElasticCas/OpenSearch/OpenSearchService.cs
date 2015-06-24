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
using Nest;
using Terradue.OpenSearch.Filters;

namespace Terradue.ElasticCas.OpenSearch {
    public static class OpenSearchService {

        public static IOpenSearchResultCollection QueryResult(IOpenSearchableElasticType type, NameValueCollection parameters, Type resultType = null) {

            OpenSearchEngine ose = type.GetOpenSearchEngine(parameters);

            if ( resultType == null )
                resultType = OpenSearchFactory.ResolveTypeFromRequest(HttpContext.Current.Request, ose);

            if ( resultType == typeof(ParametersResult) ){
                return type.DescribeParameters();
            }

            var result = ose.Query(type, parameters, resultType );

            OpenSearchFactory.ReplaceSelfLinks(type, parameters, result, type.EntrySelfLinkTemplate, result.ContentType);   
            OpenSearchFactory.ReplaceOpenSearchDescriptionLinks(type, result);

            result.Title = new TextSyndicationContent(string.Format("Result for OpenSearch query over type {0} in index {1}", type.Type.Name, type.Index.Name));

            return result;
        }

        public static HttpResult Query(IOpenSearchableElasticType document, NameValueCollection parameters, Type resultType = null) {

            // special case for description
            if (HttpContext.Current.Request.AcceptTypes != null && HttpContext.Current.Request.AcceptTypes[0] == "application/opensearchdescription+xml" || parameters["format"] == "description") {
                return new HttpResult(document.GetProxyOpenSearchDescription(), "application/opensearchdescription+xml");
            }

            var result = QueryResult(document, parameters, resultType);

            return new HttpResult(result.SerializeToString(), result.ContentType);
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

