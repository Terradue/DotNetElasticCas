using System;
using Terradue.ElasticCas.Model;
using ServiceStack.Text;
using Terradue.OpenSearch.Result;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using Terradue.ServiceModel.Syndication;
using Mono.Addins;
using System.Runtime.Serialization;
using System.Collections.Generic;
using PlainElastic.Net.Mappings;
using Terradue.ElasticCas.OpenSearch;
using Terradue.OpenSearch.Schema;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Response;
using System.Linq;
using Terradue.ElasticCas.OpenSearch.Extensions;
using System.Web;
using System.IO;
using Terradue.ElasticCas.Controller;
using PlainElastic.Net.Serialization;
using System.Xml.Linq;
using System.Diagnostics;

namespace Terradue.ElasticCas.Types {
    [Extension(typeof(IElasticDocumentCollection))]
    [ExtensionNode("GenericCollection", "Collection of generic documents")]
    public class GenericJsonCollection : IElasticDocumentCollection {

        OpenSearchDescription proxyOpenSearchDescription;

        public GenericJsonCollection() {

            links = new Collection<Terradue.ServiceModel.Syndication.SyndicationLink>();
            elementExtensions = new SyndicationElementExtensionCollection();

        }

        public static string ToJson(GenericJsonCollection gjc) {

            Dictionary<string, object> json = new Dictionary<string, object>();
            json.Add("items", gjc.items);

            return json.ToJson();

        }


        #region IElasticDocumentCollection implementation

        public string IndexName {
            get ;
            set ;
        }

        string typeName;
        public string TypeName {
            get {
                return typeName;
            }
            set {
                typeName = value;
            }
        }

        public System.Type GetOpenSearchResultType() {
            return typeof(GenericJsonCollection);
        }

        public static GenericJsonCollection FromOpenSearchResultCollection(IOpenSearchResultCollection results) {

            if (results is GenericJsonCollection)
                return (GenericJsonCollection)results;

            GenericJsonCollection collection = new GenericJsonCollection();
            collection.links = results.Links;
           
            foreach (IOpenSearchResultItem result in results.Items) {
                var item = GenericJson.FromOpenSearchResultItem(result);
                collection.items.Add(item);
            }
            return collection;

        }

        public Collection<IElasticDocument> CreateFromOpenSearchResultCollection(IOpenSearchResultCollection results) {

            return new Collection<IElasticDocument>(FromOpenSearchResultCollection(results).items.Cast<IElasticDocument>().ToList());

        }

        public PlainElastic.Net.Queries.QueryBuilder<object> BuildQuery(System.Collections.Specialized.NameValueCollection nvc) {

            PlainElastic.Net.Queries.QueryBuilder<object> query = new PlainElastic.Net.Queries.QueryBuilder<object>();

            query.Query(q => q.MatchAll());

            return query;

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

        public string EntrySelfLinkTemplate(IOpenSearchResultItem item, OpenSearchDescription osd, string mimeType) {
            return OpenSearchService.EntrySelfLinkTemplate(item, osd, mimeType);
        }

        public OpenSearchEngine GetOpenSearchEngine(NameValueCollection nvc) {
            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();
            foreach (var ext in ose.Extensions.ToList()) {
                if (ext.Value.DiscoveryContentType == "application/json")
                    ose.Extensions.Remove(ext.Key);
            }
            ose.RegisterExtension(new GenericJsonOpenSearchEngineExtension());
            return ose;
        }

        public OpenSearchDescription GetProxyOpenSearchDescription() {
            return proxyOpenSearchDescription;
        }

        public QuerySettings GetQuerySettings(OpenSearchEngine ose) {

            return new QuerySettings(this.DefaultMimeType, GenericJsonCollection.TransformElasticJsonResponseToGenericCollection);
        }

        public Terradue.OpenSearch.Request.OpenSearchRequest Create(string mimetype, System.Collections.Specialized.NameValueCollection parameters) {
            return ElasticOpenSearchRequest.Create(parameters, this);
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

            UriBuilder searchUrl = new UriBuilder(string.Format("es://elasticsearch/{0}/{1}/search", IndexName, TypeName));
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
                var eosRequest = ElasticOpenSearchRequest.Create(new NameValueCollection(), this);
                return (long)eosRequest.Count();
            }
        }

        public void ApplyResultFilters(OpenSearchRequest request,  ref IOpenSearchResultCollection osr) {

        }

        public string DefaultMimeType {
            get {
                return "application/json";
            }
        }

        public void SerializeToStream(System.IO.Stream stream) {
            JsConfig.ExcludeTypeInfo = true;
            JsConfig.IncludeTypeInfo = false;
            JsonSerializer.SerializeToStream(this, stream);
        }

        public string SerializeToString() {
            MemoryStream ms = new MemoryStream();
            SerializeToStream(ms);
            ms.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(ms);
            return sr.ReadToEnd();
        }

        public string Id {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        internal List <GenericJson> items;
        public IEnumerable<IOpenSearchResultItem> Items {
            get {
                return items.Cast<IOpenSearchResultItem>().ToArray();
            }
            set {
                items = value.Cast<GenericJson>().ToList();
            }
        }

        Collection<Terradue.ServiceModel.Syndication.SyndicationLink> links;
        public Collection<Terradue.ServiceModel.Syndication.SyndicationLink> Links {
            get {
                return links;
            }
        }

        Collection<Terradue.ServiceModel.Syndication.SyndicationCategory> categories;
        public Collection<Terradue.ServiceModel.Syndication.SyndicationCategory> Categories {
            get {
                return categories;
            }
        }

        Collection<Terradue.ServiceModel.Syndication.SyndicationPerson> authors;
        public Collection<Terradue.ServiceModel.Syndication.SyndicationPerson> Authors {
            get {
                return authors;
            }
        }

        Terradue.ServiceModel.Syndication.SyndicationElementExtensionCollection elementExtensions;
        public Terradue.ServiceModel.Syndication.SyndicationElementExtensionCollection ElementExtensions {
            get {
                return elementExtensions;
            }
        }

        string title;
        public string Title {
            get {
                return title;
            }
            set {
                title = value;
            }
        }

        DateTime date;
        public DateTime Date {
            get {
                return date;
            }
            set {
                date = value;
            }
        }

        string identifier;
        public string Identifier {
            get {
                return identifier;
            }
            set {
                identifier = value;
            }
        }

        public long Count {
            get {
                return Items.Count();
            }
        }

        readonly string contentType;
        public string ContentType {
            get {
                return contentType;
            }
        }

        public bool ShowNamespaces {
            get {
                return false;
            }
            set {

            }
        }

        #endregion

        public static GenericJsonCollection DeserializeFromStream(System.IO.Stream stream) {

            JsonObject json = JsonSerializer.DeserializeFromStream<JsonObject>(stream);

            JsonArrayObjects items = json.Get<JsonArrayObjects>("items");

            GenericJsonCollection collection = new GenericJsonCollection();
            collection.items = new List<GenericJson>();

            foreach (var item in items) {
                GenericJson it = new GenericJson(item);
                collection.items.Add(it);
            }

            return collection;

        }

        public static GenericJsonCollection TransformElasticJsonResponseToGenericCollection(OpenSearchResponse response) {

            if (response is ElasticOpenSearchResponse) {
                var results = ((ElasticOpenSearchResponse)response).GetOperationResult();
                ServiceStackJsonSerializer ser = new ServiceStackJsonSerializer();

                SearchResult<JsonObject> items = ser.ToSearchResult<JsonObject>(results);

                GenericJsonCollection collection = new GenericJsonCollection();
                collection.items = new List<GenericJson>();

                foreach (var hit in items.hits.hits) {
                    GenericJson item = new GenericJson(hit._source);
                    item.Identifier = hit._id;
                    collection.items.Add(item);

                }
                collection.ShowNamespaces = true;
                collection.Date = DateTime.UtcNow;
                collection.ElementExtensions.Add(new XElement(XName.Get("totalResults", "http://a9.com/-/spec/opensearch/1.1/"), ((ElasticOpenSearchResponse)response).TotalResult).CreateReader());

                return collection;


            } else {
                throw new NotImplementedException("GenericCollection only transforms from an ElasticOpenSearchResponse");
            }
        }

        public OpenSearchDescription ProxyOpenSearchDescription {
            get {
                return proxyOpenSearchDescription;
            }
            set {
                proxyOpenSearchDescription = value;
            }
        }
    }
}

