using System;
using Terradue.OpenSearch.Result;
using Mono.Addins;
using ServiceStack.ServiceHost;
using PlainElastic.Net.Queries;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

[assembly:AddinRoot("ElasticCas", "1.0")]
[assembly:AddinDescription("Elastic Catalogue")]
namespace Terradue.ElasticCas.Model {

    [TypeExtensionPoint()]
    public interface IElasticDocument : IOpenSearchResultItem {

        string IndexName { get; set; }

        string TypeName { get; }

        string GetMapping();

        QueryBuilder<T> BuildQuery<T>(NameValueCollection nvc); 


    }

    [TypeExtensionPoint()]
    public interface IElasticDocumentCollection : IOpenSearchResultCollection {

        string IndexName { get; set; }

        string TypeName { get; }

        Collection<IElasticDocument> CreateFromOpenSearchResult(IOpenSearchResultCollection results);

        Type GetOpenSearchResultType();

    }
}

