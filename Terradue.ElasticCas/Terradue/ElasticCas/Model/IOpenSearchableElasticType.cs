using System;
using Nest;
using Newtonsoft.Json;
using Terradue.OpenSearch;
using System.Collections.Generic;
using Terradue.OpenSearch.Engine;
using System.Collections.Specialized;
using Terradue.OpenSearch.Schema;
using Terradue.OpenSearch.Result;
using Mono.Addins;

[assembly:AddinRoot("ElasticCas", "1.1")]
[assembly:AddinDescription("Elastic Catalogue")]
namespace Terradue.ElasticCas.Model {

    [TypeExtensionPoint()]
    public interface IOpenSearchableElasticType : IElasticType, IOpenSearchable, IProxiedOpenSearchable {

        IndexNameMarker Index { get; }

        RootObjectMapping GetRootMapping();

        ISearchRequest DescribeSearch(ISearchRequest search, System.Collections.Specialized.NameValueCollection nvc);

        Dictionary <string, object> Parameters { get; set; }

        OpenSearchEngine GetOpenSearchEngine(NameValueCollection nvc);

        string EntrySelfLinkTemplate(IOpenSearchResultItem item, OpenSearchDescription osd, string mimeType);

        IElasticCollection FromOpenSearchResultCollection(IOpenSearchResultCollection results);

        List<OpenSearchDescriptionUrlParameter> DescribeParameters();
    }
}

