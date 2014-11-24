using System;
using System.Linq;
using Terradue.OpenSearch.Request;
using ServiceStack.Common.Web;
using Terradue.ElasticCas.Model;
using Terradue.OpenSearch.Response;
using System.IO;
using System.Web;
using Terradue.ElasticCas.Controller;
using Terradue.OpenSearch.Engine;
using System.Diagnostics;
using Nest;
using Terradue.ElasticCas.Types;

namespace Terradue.ElasticCas.OpenSearch {
    public class ElasticOpenSearchRequest<T> : OpenSearchRequest where T: class, IElasticDocument, new() {

        string indexName;
        string typeName;

        Type resultType;

        ElasticClientWrapper client;

        ISearchRequest search;

        internal ElasticOpenSearchRequest(ElasticClientWrapper client, string indexName, string typeName, System.Collections.Specialized.NameValueCollection parameters) : base(BuildOpenSearchUrl(indexName, typeName, parameters)) {
            this.resultType = resultType;
            this.indexName = indexName;
            this.typeName = typeName;
            this.client = client;
        }

        public string IndexName {
            get {
                return indexName;
            }
        }

        public string TypeName {
            get {
                return typeName;
            }
        }

        internal ISearchRequest SearchRequest {
            get {
                return search;
            }
            set {
                search = value;
            }
        }

        public static ElasticOpenSearchRequest<T> Create(System.Collections.Specialized.NameValueCollection parameters, IElasticDocument document) {

            ElasticCasFactory ecf = new ElasticCasFactory("ElasticOpenSearchRequest");

            ElasticOpenSearchRequest<T> eosRequest = new ElasticOpenSearchRequest<T>(ecf.Client, document.IndexName, document.TypeName, parameters);

            var search = new Nest.SearchRequest(document.IndexName, document.TypeName);

            var query = document.BuildQuery(parameters);
            search.Query = query;
            search.QueryString = new Elasticsearch.Net.SearchRequestParameters();

            if (!string.IsNullOrEmpty(parameters["count"])) {
                search.Size = int.Parse(parameters["count"]);
            } else {
                search.Size = OpenSearchEngine.DEFAULT_COUNT;
            }

            if (!string.IsNullOrEmpty(parameters["startIndex"])) {
                search.From = int.Parse(parameters["startIndex"]) - 1;
            }

            if (!string.IsNullOrEmpty(parameters["q"])) {

                eosRequest.Parameters.Set("q", parameters["q"]);
            }

            eosRequest.search = search;

            return eosRequest;

        }

        static Terradue.OpenSearch.OpenSearchUrl BuildOpenSearchUrl(string indexName, string typeName, System.Collections.Specialized.NameValueCollection parameters) {
            UriBuilder url = new UriBuilder(string.Format("{0}/{1}/{2}/search", Settings.ElasticSearchServer, indexName, typeName));
            var array = (from key in parameters.AllKeys
                                  from value in parameters.GetValues(key)
                                  select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                .ToArray();
            url.Query = string.Join("&", array);

            return new Terradue.OpenSearch.OpenSearchUrl(url.Uri);
        }

        public long Count()  {

            ISearchResponse<T> response = client.Search<T>(SearchRequest);

            return response.Total;
        }

        #region implemented abstract members of OpenSearchRequest

        public override OpenSearchResponse GetResponse() {

            var response = client.Search<T>(SearchRequest);
            return new ElasticOpenSearchResponse<T>(response);

        }

        #endregion


    }
}

