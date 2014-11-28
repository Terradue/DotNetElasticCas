using System;
using Terradue.OpenSearch.Result;
using Mono.Addins;
using ServiceStack.ServiceHost;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Terradue.OpenSearch;
using Terradue.OpenSearch.Schema;
using Terradue.OpenSearch.Engine;
using Nest;
using Newtonsoft.Json;
using Terradue.ElasticCas.Controllers;
using Terradue.ElasticCas.Types;


namespace Terradue.ElasticCas.Model {

    [JsonConverter(typeof(ElasticJsonTypeConverter))]
    public interface IElasticItem : IOpenSearchResultItem {

        TypeNameMarker TypeName { get; }

        NameValueCollection GetTypeNamespaces();

        IElasticItem ReadElasticJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer);

        void WriteElasticJson(JsonWriter writer, Newtonsoft.Json.JsonSerializer serializer);


    }

    public interface IElasticCollection : IOpenSearchResultCollection {

        IElasticCollection FromOpenSearchResultCollection(IOpenSearchResultCollection results);

        IEnumerable<IElasticItem> ElasticItems { get; }

    }
}

