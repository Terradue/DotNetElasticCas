using System;
using Terradue.OpenSearch.Response;
using System.IO;
using Terradue.ElasticCas.Controllers;
using Terradue.ElasticCas.Model;

namespace Terradue.ElasticCas.OpenSearch {
    public class ElasticOpenSearchResponse<T> : OpenSearchResponse where T: class, new() {

        Nest.ISearchResponse<T> result;

        public ElasticOpenSearchResponse(Nest.ISearchResponse<T> result) {
            this.result = result;
        }

        #region implemented abstract members of OpenSearchResponse

        public override System.IO.Stream GetResponseStream() {

            MemoryStream ms = new MemoryStream();

            StreamWriter sw = new StreamWriter(ms);

            sw.Write(result.RequestInformation.ResponseRaw);

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
                return new TimeSpan(0,0,0,0,result.ElapsedMilliseconds);
            }
        }

        #endregion

        public long TotalResult {
            get {
                return result.Total;
            }
        }

        public Nest.ISearchResponse<T> GetSearchResponse()
        {
            return result;
        }
    }
}

