using System;
using Terradue.OpenSearch.Result;
using Mono.Addins;
using ServiceStack.ServiceHost;
using PlainElastic.Net.Queries;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Terradue.OpenSearch;
using Terradue.OpenSearch.Schema;

[assembly:AddinRoot("ElasticCas", "1.0")]
[assembly:AddinDescription("Elastic Catalogue")]
namespace Terradue.ElasticCas.Model {

    [TypeExtensionPoint()]
    public interface IElasticDocument : IOpenSearchResultItem {

        string IndexName { get; set; }

        string TypeName { get; }

        string GetMapping();

        NameValueCollection GetTypeNamespaces();
    }

    [TypeExtensionPoint()]
    public interface IElasticDocumentCollection : IOpenSearchResultCollection, IOpenSearchable, IProxiedOpenSearchable {

        string IndexName { get; set; }

        string TypeName { get; }

        Dictionary <string, object> Parameters { get; set; }

        Collection<IElasticDocument> CreateFromOpenSearchResultCollection(IOpenSearchResultCollection results);

        Type GetOpenSearchResultType();

        QueryBuilder<object> BuildQuery(NameValueCollection nvc);

        OpenSearchDescription ProxyOpenSearchDescription { get; set; }

        string EntrySelfLinkTemplate(IOpenSearchResultItem item, OpenSearchDescription osd, string mimeType);
    }
}

