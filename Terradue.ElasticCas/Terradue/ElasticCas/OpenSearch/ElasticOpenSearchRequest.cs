using System;
using System.Linq;
using Terradue.OpenSearch.Request;
using ServiceStack.Common.Web;
using Terradue.ElasticCas.Model;
using PlainElastic.Net;
using Terradue.OpenSearch.Response;
using System.IO;
using System.Web;
using PlainElastic.Net.Serialization;
using Terradue.ElasticCas.Controller;

namespace Terradue.ElasticCas {
    public class ElasticOpenSearchRequest : OpenSearchRequest {

        string indexName;
        string typeName;

        ElasticConnection esConnection;

        string queryJson;

        internal ElasticOpenSearchRequest(ElasticConnection esConnection, string indexName, string typeName, System.Collections.Specialized.NameValueCollection parameters) : base(BuildOpenSearchUrl(esConnection, indexName, typeName, parameters)) {
            this.indexName = indexName;
            this.typeName = typeName;
            this.esConnection = esConnection;
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

        internal string QueryJson {
            get {
                return queryJson;
            }
            set {
                queryJson = value;
            }
        }

        public static ElasticOpenSearchRequest Create(System.Collections.Specialized.NameValueCollection parameters, IElasticDocumentCollection collection) {

            ElasticCasFactory ecf = new ElasticCasFactory("ElasticOpenSearchRequest");

            ElasticOpenSearchRequest eosRequest = new ElasticOpenSearchRequest(ecf.EsConnection, collection.IndexName, collection.TypeName, parameters);

            var query = collection.BuildQuery(parameters);

            eosRequest.QueryJson = query.Build();

            return eosRequest;

        }

        static Terradue.OpenSearch.OpenSearchUrl BuildOpenSearchUrl(ElasticConnection esConnection, string indexName, string typeName, System.Collections.Specialized.NameValueCollection parameters) {
            UriBuilder url = new UriBuilder(string.Format("es://{0}:{1}/{2}/{3}/search", esConnection.DefaultHost, esConnection.DefaultPort, indexName, typeName));
            var array = (from key in parameters.AllKeys
                                  from value in parameters.GetValues(key)
                                  select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                .ToArray();
            url.Query = string.Join("&", array);

            return new Terradue.OpenSearch.OpenSearchUrl(url.Uri);
        }

        public long Count() {
            var command = new CountCommand(IndexName, TypeName);
            var result = esConnection.Get(command, queryJson);

            ServiceStackJsonSerializer ser = new ServiceStackJsonSerializer();
            return ser.ToCountResult(result).count;
        }

        #region implemented abstract members of OpenSearchRequest

        public override Terradue.OpenSearch.Response.OpenSearchResponse GetResponse() {

            var command = new SearchCommand(IndexName, TypeName);

            OperationResult result = esConnection.Post(command, queryJson);
            return new ElasticOpenSearchResponse(result);



        }

        #endregion


    }
}

