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
using Terradue.ElasticCas.Controller;
using Terradue.ElasticCas.Types;

[assembly:AddinRoot("ElasticCas", "1.0")]
[assembly:AddinDescription("Elastic Catalogue")]
namespace Terradue.ElasticCas.Model {

    [TypeExtensionPoint()]
    public interface IElasticDocument : IOpenSearchResultItem, IOpenSearchable, IProxiedOpenSearchable {

        new string Id { get; }

        string IndexName { get; set; }

        string TypeName { get; }

        RootObjectMapping GetMapping();

        NameValueCollection GetTypeNamespaces();

        Nest.IQueryContainer BuildQuery(NameValueCollection nvc);

        Dictionary <string, object> Parameters { get; set; }

        OpenSearchEngine GetOpenSearchEngine(NameValueCollection nvc);

        void ToElasticJson(JsonWriter writer, Newtonsoft.Json.JsonSerializer serializer);

        IElasticDocument FromElasticJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer);

        IElasticDocumentCollection GetContainer();

        OpenSearchDescription ProxyOpenSearchDescription { get; set; }

        string EntrySelfLinkTemplate(IOpenSearchResultItem item, OpenSearchDescription osd, string mimeType);

    }

    public interface IElasticDocumentCollection : IOpenSearchResultCollection {

        string IndexName { get; set; }

        string TypeName { get; }

        Collection<IElasticDocument> CreateFromOpenSearchResultCollection(IOpenSearchResultCollection results);

    }
}

