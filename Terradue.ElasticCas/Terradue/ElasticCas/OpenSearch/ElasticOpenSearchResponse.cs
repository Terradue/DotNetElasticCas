using System;
using Terradue.OpenSearch.Response;
using PlainElastic.Net;
using PlainElastic.Net.Serialization;
using System.IO;
using Terradue.ElasticCas.Controller;

namespace Terradue.ElasticCas {
    public class ElasticOpenSearchResponse : OpenSearchResponse {

        OperationResult result;

        public ElasticOpenSearchResponse(OperationResult result) {
            this.result = result;
        }

        #region implemented abstract members of OpenSearchResponse

        public override System.IO.Stream GetResponseStream() {
            ServiceStackJsonSerializer ser= new ServiceStackJsonSerializer();

            var results = ser.ToSearchResult<string>(result);

            MemoryStream ms = new MemoryStream();

            StreamWriter sw = new StreamWriter(ms);

            sw.Write("{\"collection\":{\"features\":[");
            string sep = "";

            foreach ( string document in results.Documents ){
                sw.Write(sep+document);
                sep = ",";
            }

            sw.Write("],\"type\":\"FeatureCollection\"}");

            sw.Flush();
            sw.Close();

            ms.Seek(0, SeekOrigin.Begin);

            return ms;
        }

        public override string ContentType {
            get {
                return "application/json";
            }
        }

        public override TimeSpan RequestTime {
            get {
                ServiceStackJsonSerializer ser = new ServiceStackJsonSerializer();

                var results = ser.ToSearchResult<object>(result);

                return new TimeSpan(results.took);
            }
        }

        #endregion

        public OperationResult GetOperationResult()
        {
            return result;
        }
    }
}

