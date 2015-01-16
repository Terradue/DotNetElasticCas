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

namespace Terradue.ElasticCas.OpenSearch {

    [JsonObject]
    public class ElasticSearchResponse<T> : BaseResponse where T: class, IElasticItem, new(){

        public ElasticSearchResponse (){
            this.Aggregations = new Dictionary<string, IAggregation>();
            this.Facets = new Dictionary<string, Facet>();
        }

        public ElasticSearchResponse (Nest.ISearchResponse<T> response){

            this.Shards = response.Shards;
            this.HitsMetaData = response.HitsMetaData;
            this.Facets = response.Facets;
            this.Aggregations = response.Aggregations;
            this.Suggest = response.Suggest;
            this.ElapsedMilliseconds = response.ElapsedMilliseconds;
            this.TimedOut = response.TimedOut;
            this.ScrollId = response.ScrollId;

        }

        [JsonProperty(PropertyName = "_shards")]
        public ShardsMetaData Shards { get; internal set; }

        [JsonProperty(PropertyName = "hits")]
        public HitsMetaData<T> HitsMetaData { get; internal set; }

        [JsonProperty(PropertyName = "facets")]
        [JsonConverter(typeof(DictionaryKeysAreNotPropertyNamesJsonConverter))]
        public IDictionary<string, Facet> Facets { get; internal set; }

        [JsonProperty(PropertyName = "aggregations")]
        [JsonConverter(typeof(DictionaryKeysAreNotPropertyNamesJsonConverter))]
        public IDictionary<string, IAggregation> Aggregations { get; internal set; }

        [JsonProperty(PropertyName = "suggest")]
        public IDictionary<string, Suggest[]> Suggest { get; internal set; }

        [JsonProperty(PropertyName = "took")]
        public int ElapsedMilliseconds { get; internal set; }

        [JsonProperty("timed_out")]
        public bool TimedOut { get; internal set; }

        [JsonProperty("terminated_early")]
        public bool TerminatedEarly { get; internal set; }

        [JsonProperty(PropertyName = "_scroll_id")]
        public string ScrollId { get; internal set; }

    }
}
