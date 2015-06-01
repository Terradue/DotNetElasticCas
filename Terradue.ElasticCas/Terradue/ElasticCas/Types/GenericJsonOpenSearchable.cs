using System;
using Terradue.ElasticCas.Model;
using Nest;
using Terradue.ElasticCas.Controllers;
using Terradue.OpenSearch.Schema;
using Newtonsoft.Json;
using System.Collections.Generic;
using Terradue.OpenSearch.Engine;
using System.Linq;
using Terradue.ElasticCas.OpenSearch.Extensions;
using Terradue.OpenSearch;
using Terradue.ElasticCas.OpenSearch;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Result;
using Mono.Addins;
using System.Collections.Specialized;
using System.Web;

namespace Terradue.ElasticCas.Types {

    [Extension(typeof(IOpenSearchableElasticType))]
    [ExtensionNode("Generic", "Type representing generic data")]
    public class GenericJsonOpenSearchable : IOpenSearchableElasticType {

        IndexNameMarker index;

        public GenericJsonOpenSearchable() {
        }

        public GenericJsonOpenSearchable(IndexNameMarker index, TypeNameMarker type) {
            this.type = type;
            this.index = index;
        }

        #region IOpenSearchableElasticType implementation

        public Nest.RootObjectMapping GetRootMapping() {
            PutMappingDescriptor<GenericJsonItem> putMappingRequest = new PutMappingDescriptor<GenericJsonItem>(ElasticClientWrapper.ConnectionSettings);
            putMappingRequest.MapFromAttributes();
            putMappingRequest.TimestampField(t => t.Enabled().Path("date"));
            putMappingRequest.Type(Type.Name);
            return ((IPutMappingRequest)putMappingRequest).Mapping;
        }

        public Nest.ISearchRequest DescribeSearch(Nest.ISearchRequest search, System.Collections.Specialized.NameValueCollection nvc) {
            SearchDescriptor<GenericJsonItem> searchD = (SearchDescriptor<GenericJsonItem>)search;
            searchD.MatchAll();

            if (!string.IsNullOrEmpty(nvc["q"])) {
                searchD.QueryString(nvc["q"]);
            }

            return searchD;
        }

        public Terradue.OpenSearch.Engine.OpenSearchEngine GetOpenSearchEngine(System.Collections.Specialized.NameValueCollection nvc) {
            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();
            foreach (var ext in ose.Extensions.ToList()) {
                if (ext.Value.DiscoveryContentType == "application/json")
                    ose.Extensions.Remove(ext.Key);
            }
            ose.RegisterExtension(new GenericJsonOpenSearchEngineExtension());
            return ose;
        }

        public string EntrySelfLinkTemplate(IOpenSearchResultItem item, OpenSearchDescription osd, string mimeType) {
            return OpenSearchService.EntrySelfLinkTemplate(item, osd, mimeType);
        }

        Dictionary<string, object> parameters;
        public Dictionary<string, object> Parameters {
            get {
                return parameters;
            }
            set {
                parameters = value;
            }
        }

        public IndexNameMarker Index {
            get {
                return index;
            }
        }

        public IElasticCollection FromOpenSearchResultCollection(IOpenSearchResultCollection results) {

            return GenericJsonCollection.CreateFromOpenSearchResultCollection(results);

        }


        public ParametersResult DescribeParameters() {

            return OpenSearchFactory.GetDefaultParametersResult();


        }


        #endregion

        #region IProxiedOpenSearchable implementation

        public OpenSearchDescription GetProxyOpenSearchDescription() {
            return ElasticCasFactory.GetDefaultOpenSearchDescription(this);
        }

        #endregion

        #region IOpenSearchable implementation

        public QuerySettings GetQuerySettings(OpenSearchEngine ose) {
            return new QuerySettings(this.DefaultMimeType, new GenericJsonOpenSearchEngineExtension().ReadNative);
        }

        public Terradue.OpenSearch.Request.OpenSearchRequest Create(string mimetype, System.Collections.Specialized.NameValueCollection parameters) {
            return ElasticOpenSearchRequest<GenericJsonItem>.Create(parameters, this);
        }

        public Terradue.OpenSearch.Schema.OpenSearchDescription GetOpenSearchDescription() {
            OpenSearchDescription osd = new OpenSearchDescription();

            osd.ShortName = "Elastic Catalogue";
            osd.Attribution = "Terradue";
            osd.Contact = "info@terradue.com";
            osd.Developer = "Terradue GeoSpatial Development Team";
            osd.SyndicationRight = "open";
            osd.AdultContent = "false";
            osd.Language = "en-us";
            osd.OutputEncoding = "UTF-8";
            osd.InputEncoding = "UTF-8";
            osd.Description = "This Search Service performs queries in the available dataset catalogue. There are several URL templates that return the results in different formats (GeoJson, RDF, ATOM or KML). This search service is in accordance with the OGC 10-032r3 specification.";

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            var searchExtensions = ose.Extensions;
            List<OpenSearchDescriptionUrl> urls = new List<OpenSearchDescriptionUrl>();

            NameValueCollection parameters = GetOpenSearchParameters(this.DefaultMimeType);

            UriBuilder searchUrl = new UriBuilder(string.Format("es://elasticsearch/{0}/{1}/search", this.index.Name, this.Type.Name));
            NameValueCollection queryString = HttpUtility.ParseQueryString("?format=json");
            parameters.AllKeys.FirstOrDefault(k => {
                queryString.Add(parameters[k], "{" + k + "?}");
                return false;
            });


            searchUrl.Query = queryString.ToString();
            urls.Add(new OpenSearchDescriptionUrl("application/json", 
                                                  searchUrl.ToString(),
                                                  "search"));

            osd.Url = urls.ToArray();

            return osd;
        }

        public System.Collections.Specialized.NameValueCollection GetOpenSearchParameters(string mimeType) {
            return OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter());
        }

        public long TotalResults {
            get {
                var eosRequest = ElasticOpenSearchRequest<GenericJsonItem>.Create(new NameValueCollection(), this);
                return (long)eosRequest.Count();
            }
        }

        public void ApplyResultFilters(OpenSearchRequest request,  ref IOpenSearchResultCollection osr) {
            OpenSearchFactory.ReplaceSelfLinks(this, request.Parameters, osr, this.EntrySelfLinkTemplate); 
        }

        public string DefaultMimeType {
            get {
                return "application/json";
            }
        }

        public string Identifier {
            get {
                return null;
            }
        }

        public bool CanCache {
            get {
                return true;
            }
        }

        #endregion

        #region IElasticType implementation

        Nest.PropertyNameMarker name;
        public Nest.PropertyNameMarker Name {
            get {
                return name;
            }
            set {
                name = value;
            }
        }

        readonly Nest.TypeNameMarker type;
        public Nest.TypeNameMarker Type {
            get {
                return type;
            }
        }

        #endregion
    }
}

