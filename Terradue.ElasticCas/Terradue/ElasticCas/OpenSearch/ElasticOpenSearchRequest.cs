using System;
using System.Linq;
using Terradue.OpenSearch.Request;
using ServiceStack.Common.Web;
using Terradue.ElasticCas.Model;
using Terradue.OpenSearch.Response;
using System.IO;
using System.Web;
using Terradue.ElasticCas.Controllers;
using Terradue.OpenSearch.Engine;
using System.Diagnostics;
using Nest;
using Terradue.ElasticCas.Types;
using Newtonsoft.Json;

namespace Terradue.ElasticCas.OpenSearch {
    public class ElasticOpenSearchRequest<T> : OpenSearchRequest where T: class, IElasticItem, new() {

        string indexName;
        string typeName;

        Type resultType;

        ElasticClientWrapper client;

        IOpenSearchableElasticType type;

        System.Collections.Specialized.NameValueCollection parameters;

        internal ElasticOpenSearchRequest(ElasticClientWrapper client, string indexName, string typeName, System.Collections.Specialized.NameValueCollection parameters, IOpenSearchableElasticType type) :
            base(BuildOpenSearchUrl(indexName, typeName, parameters), "application/json") {
            this.parameters = parameters;
            this.type = type;
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

        public static ElasticOpenSearchRequest<T> Create(System.Collections.Specialized.NameValueCollection parameters, IOpenSearchableElasticType type) {

            ElasticCasFactory ecf = new ElasticCasFactory("ElasticOpenSearchRequest");

            ElasticOpenSearchRequest<T> eosRequest = new ElasticOpenSearchRequest<T>(ecf.Client, type.Index.Name, type.Type.Name, parameters, type);

            return eosRequest;

        }

        public SearchDescriptor<T> DescribeSearch(SearchDescriptor<T> search) {

            search.Index(indexName).Type(typeName);

            type.DescribeSearch(search, Parameters);

            if (!string.IsNullOrEmpty(Parameters["count"])) {
                search.Size(int.Parse(Parameters["count"]));
            } else {
                search.Size(OpenSearchEngine.DEFAULT_COUNT);
            }

            if (!string.IsNullOrEmpty(Parameters["startIndex"])) {
                search.From(int.Parse(Parameters["startIndex"]) - 1);
            }

            return search;
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

        public long Count() {

            ISearchResponse<T> response = client.Search<T>(DescribeSearch);
            return response.Total;

        }

        #region implemented abstract members of OpenSearchRequest

        public override IOpenSearchResponse GetResponse() {

            var response = client.Search<T>(DescribeSearch);
            return new ElasticOpenSearchResponse<T>(response);

        }

        public override System.Collections.Specialized.NameValueCollection OriginalParameters {
            get {
                return parameters;
            }
            set {
                parameters = value;
            }
        }

        #endregion


    }
}

