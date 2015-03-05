using System;
using Terradue.OpenSearch.Response;
using System.IO;
using Terradue.ElasticCas.Controllers;
using Terradue.ElasticCas.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;
using Nest;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace Terradue.ElasticCas.OpenSearch {
    public class ElasticOpenSearchResponse<TResult> : OpenSearchResponse<ISearchResponse<TResult>> where TResult: class, IElasticItem, new() {

        ISearchResponse<TResult> response;

        public ElasticOpenSearchResponse(ISearchResponse<TResult> response) {
            this.response = response;
        }

        #region implemented abstract members of OpenSearchResponse

        public override object GetResponseObject() {

            return response;

        }

        public override string ContentType {
            get {
                return "application/json";
            }
        }

        public override TimeSpan RequestTime {
            get {
                return new TimeSpan(0,0,0,0,GetResults().ElapsedMilliseconds);
            }
        }

        public override object Clone() {
            return new ElasticOpenSearchResponse<TResult>(response);
        }

        #endregion

        public ISearchResponse<TResult> GetResults()
        {
            //StreamReader sr = new StreamReader(response);
            //string str = sr.ReadToEnd();
            //SearchResponse<T> res = JsonConvert.DeserializeObject<SearchResponse<T>>(str);

            return response;
        }


    }

}

