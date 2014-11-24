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
using Terradue.ElasticCas.OpenSearch;
using System.Diagnostics;
using Nest;
using Newtonsoft.Json;
using Nest.Resolvers.Converters;
using Terradue.ElasticCas.Controller;
using Newtonsoft.Json.Linq;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Schema;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch;
using System.Linq;
using Terradue.ElasticCas.OpenSearch.Extensions;
using System.Web;


namespace Terradue.ElasticCas.Types {

    [Extension(typeof(IElasticDocument))]
    [ExtensionNode("Generic", "Document representing generic data")]
    [DataContract]
    [JsonConverter(typeof(ReadElasticJsonAsTypeConverter<GenericJson>))]
    public class GenericJson : IElasticDocument {

        OpenSearchDescription proxyOpenSearchDescription;

        public GenericJson() {
            this.showNamespaces = true;
            this.elementExtensions = new SyndicationElementExtensionCollection();
            this.links = new Collection<SyndicationLink>();
        }

        public GenericJson(JsonObject item) : this() {
            foreach (string ext in item.Keys) {
                if (ext == "identifier") {
                    this.Identifier = item.GetUnescaped(ext);
                    continue;
                }
                if (ext == "date") {
                    this.Date = DateTime.Parse(item.GetUnescaped(ext));
                    continue;
                }
                var test = item.GetUnescaped(ext);
                this.ElementExtensions.Add(ext, "", test);

            }
        }

        public new static GenericJson FromOpenSearchResultItem(IOpenSearchResultItem result) {
            if (result is GenericJson)
                return (GenericJson)result;

            GenericJson item = new GenericJson();

            item.authors = result.Authors;
            item.elementExtensions = result.ElementExtensions;

            //TODO complete the copy

            return item;
        }

        public static string ToJson(GenericJson gj) {

            Dictionary<string, object> json = new Dictionary<string, object>();

            foreach (SyndicationElementExtension ext in gj.ElementExtensions) {
                string obj = ext.GetObject<string>();
                json.Add(ext.OuterName, obj);
            }

            json.Add("identifier", gj.Identifier);
            json.Add("date", gj.Date.ToString("yyyy-MM-ddTHH:mm:ssZ"));

            return ServiceStack.Text.JsonSerializer.SerializeToString(json);

        }

        #region IElasticDocument implementation

        string id;
        public string Id {
            get {
                if (string.IsNullOrEmpty(id) && Links != null && Links.Count > 0) {
                    var link = Links.FirstOrDefault(s => {
                        return s.RelationshipType == "self";
                    });
                    id = link.Uri.ToString();
                }
                return id;
            }
            set {
                id = value;
            }
        }

        [IgnoreDataMember]
        public string IndexName {
            get ;
            set ;
        }

        string typeName;

        [IgnoreDataMember]
        public string TypeName {
            get {
                return typeName;
            }
            set {
                typeName = value;
            }
        }

        public RootObjectMapping GetMapping() {

            RootObjectMapping mapping = new RootObjectMapping();

            mapping.AllFieldMapping = new AllFieldMapping();
            mapping.AllFieldMapping.Enabled = true;
            mapping.TimestampFieldMapping = new TimestampFieldMapping();
            mapping.TimestampFieldMapping.Enabled = true;
            mapping.TimestampFieldMapping.Path = new PropertyPathMarker();
            mapping.TimestampFieldMapping.Path.Name = "date";

            return mapping;
        }

        public IQueryContainer BuildQuery(System.Collections.Specialized.NameValueCollection nvc) {

            IQueryContainer query = new QueryContainer();

            if (string.IsNullOrEmpty(nvc["q"])) {
                query.MatchAllQuery = new MatchAllQuery();
            } else {
                query.QueryString = new QueryStringQuery();
                query.QueryString.Query = nvc["q"];
            }

            return query;

        }

        public NameValueCollection GetTypeNamespaces() {
            return DefaultNamespaces.TypeNamespaces;
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

        DateTime date = DateTime.UtcNow;
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
                if (string.IsNullOrEmpty(identifier)) {
                    identifier = Guid.NewGuid().ToString();
                }
                return identifier;
            }
            set {
                identifier = value;
            }
        }

        Collection<SyndicationLink> links;
        public Collection<SyndicationLink> Links {
            get {
                return links;
            }
        }

        Collection<SyndicationCategory> categories;
        public Collection<SyndicationCategory> Categories {
            get {
                return categories;
            }
        }

        Collection<SyndicationPerson> authors;
        public Collection<SyndicationPerson> Authors {
            get {
                return authors;
            }
        }

        SyndicationElementExtensionCollection elementExtensions;
        public SyndicationElementExtensionCollection ElementExtensions {
            get {
                return elementExtensions;
            }
        }

        bool showNamespaces;
        public bool ShowNamespaces {
            get {
                return showNamespaces;
            }
            set {
                showNamespaces = value;
            }
        }

        public void ToElasticJson(JsonWriter writer, Newtonsoft.Json.JsonSerializer serializer){
            writer.WriteRaw(ToJson(this));
        }


        public IElasticDocument FromElasticJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer) {

            string json = JObject.Load(reader).ToString();
            JsonObject obj = ServiceStack.Text.JsonSerializer.DeserializeFromString<JsonObject>(json);
            return new GenericJson(obj);
        }

        public IElasticDocumentCollection GetContainer() {
            return new GenericJsonCollection();
        }

        public OpenSearchDescription ProxyOpenSearchDescription {
            get {
                return proxyOpenSearchDescription;
            }
            set {
                proxyOpenSearchDescription = value;
            }
        }
        #endregion

        Dictionary<string, object> parameters;

        public Dictionary<string, object> Parameters {
            get {
                return parameters;
            }
            set {
                parameters = value;
            }
        }

        #region IElasticDocument implementation

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
            return ElasticOpenSearchRequest<GenericJson>.Create(parameters, this);
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
                var eosRequest = ElasticOpenSearchRequest<GenericJson>.Create(new NameValueCollection(), this);
                return (long)eosRequest.Count();
            }
        }

        public void ApplyResultFilters(OpenSearchRequest request,  ref IOpenSearchResultCollection osr) {
            OpenSearchFactory.ReplaceSelfLinks(this, request.Parameters, osr, this.EntrySelfLinkTemplate, osr.ContentType); 
        }

        public string DefaultMimeType {
            get {
                return "application/json";
            }
        }

        #endregion
       
    }
}

