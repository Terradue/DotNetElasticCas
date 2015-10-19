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
using Terradue.OpenSearch.Result;

namespace Terradue.ElasticCas.OpenSearch {
    public class ElasticOpenSearchRequest<T> : OpenSearchRequest where T: class, IElasticItem, new() {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string indexName;
        string typeName;

        Type resultType;

        ElasticClientWrapper client;

        static ElasticCasFactory factory = new ElasticCasFactory("ElasticOpenSearchRequest");

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
            
            ElasticOpenSearchRequest<T> eosRequest = new ElasticOpenSearchRequest<T>(factory.Client, type.Index.Name, type.Type.Name, parameters, type);

            return eosRequest;

        }

        public SearchDescriptor<T> DescribeSearch(SearchDescriptor<T> search) {

            search.Index(indexName).Type(typeName);

            log.DebugFormat("describe search for type {0}", typeName);
            type.DescribeSearch(search, Parameters);

            int count = OpenSearchEngine.DEFAULT_COUNT;
            if (!string.IsNullOrEmpty(Parameters["count"])) {
                count = int.Parse(Parameters["count"]);
            } 

            search.Size(count);

            int startIndex = 1;
            int startPage = 1;

            if (!string.IsNullOrEmpty(Parameters["startIndex"])) {
                startIndex = int.Parse(Parameters["startIndex"]);

            }

            if (!string.IsNullOrEmpty(Parameters["startPage"])) {
                startPage = int.Parse(Parameters["startPage"]);
            }

            search.From(startIndex - 1 + ((startPage - 1) * count));
            log.DebugFormat("describe completed");

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

            // error is catched in order to not produce an excpetion
            // we assume 0 results
            // the error shall be described in the result feed
            try {
                ISearchResponse<T> response = client.Search<T>(DescribeSearch);
                return response.Total;
            } catch (Exception) {
                return 0;
            }

        }

        #region implemented abstract members of OpenSearchRequest

        public override IOpenSearchResponse GetResponse() {

            log.DebugFormat("Describe query and start search");
            var response = client.Search<T>(DescribeSearch);
            log.DebugFormat("Search completed in {0} ms, returning response", response.ElapsedMilliseconds);
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

