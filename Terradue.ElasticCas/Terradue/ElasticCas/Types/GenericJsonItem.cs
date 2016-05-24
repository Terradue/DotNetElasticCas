using System;
using Terradue.ElasticCas.Model;
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
using Terradue.ElasticCas.Controllers;
using Newtonsoft.Json.Linq;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Schema;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch;
using System.Linq;
using Terradue.ElasticCas.OpenSearch.Extensions;
using System.Web;
using System.Xml;
using Terradue.ElasticCas.Converters;


namespace Terradue.ElasticCas.Types {

    [Serializable]
    [DataContract]
    [JsonConverter(typeof(ElasticJsonTypeConverter))]
    public class GenericJsonItem : IElasticItem, IElasticObject {
    
        Dictionary<string, object> payload;

        public GenericJsonItem() {
            this.payload = new Dictionary<string, object>();
            LastUpdatedTime = DateTime.UtcNow;
            PublishDate = DateTime.UtcNow;
            links = new Collection<SyndicationLink>();
            authors = new Collection<SyndicationPerson>();
            categories = new Collection<SyndicationCategory>();
            contributors = new Collection<SyndicationPerson>();
        }

        public GenericJsonItem(Dictionary<string, object> item) : this() {
        
            foreach (string ext in item.Keys) {
                if (payload.ContainsKey(ext))
                    payload.Remove(ext);

                payload.Add(ext, item[ext]);

            }
        }

        public GenericJsonItem(GenericJsonItem item) : this() {

            Identifier = item.Identifier;
            LastUpdatedTime = item.LastUpdatedTime;

            ElementExtensions = new SyndicationElementExtensionCollection(item.ElementExtensions);
        }

        public new static GenericJsonItem FromOpenSearchResultItem(IOpenSearchResultItem result) {
            if (result is GenericJsonItem)
                return (GenericJsonItem)result;

            GenericJsonItem item = new GenericJsonItem();

            item.Identifier = result.Identifier;
            item.LastUpdatedTime = result.LastUpdatedTime;

            foreach (SyndicationElementExtension ext in result.ElementExtensions) {
                XmlDocument doc = new XmlDocument();
                doc.Load(ext.GetReader());
                var obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.None, false));

                item.payload.Add(ext.OuterName, obj[ext.OuterName]);
            }

            return item;
        }

        public string ToJson(GenericJsonItem gj) {

            return JsonConvert.SerializeObject(gj.payload);

        }

        #region IElasticItem implementation

        string id;

        public string Id {
            get {
                if (string.IsNullOrEmpty(id) && Links != null && Links.Count > 0) {
                    var link = Links.FirstOrDefault(s => {
                        return s.RelationshipType == "self";
                    });
                    if (link != null)
                        id = link.Uri.ToString();
                }
                return id;
            }
            set {
                id = value;
            }
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

        public TextSyndicationContent Title {
            get {
                if (payload.ContainsKey("title"))
                    return new TextSyndicationContent((string)payload["title"]);
                return null;
            }
            set {
                payload["title"] = value.Text;
            }
        }

        public DateTime Modified {
            get {
                return this.LastUpdatedTime;
            }
            set {
                this.LastUpdatedTime = value;
            }
        }


        public DateTime LastUpdatedTime {
            get {
                if (payload.ContainsKey("updated"))
                    return (DateTime)payload["updated"];
                return DateTime.UtcNow;
            }
            set {
                payload["updated"] = value;
            }
        }

        public DateTime PublishDate {
            get {
                if (payload.ContainsKey("published"))
                    return (DateTime)payload["published"];
                return DateTime.UtcNow;
            }
            set {
                payload["published"] = value;
            }
        }

        public string Identifier {
            get {
                if (!payload.ContainsKey("identifier") || string.IsNullOrEmpty((string)payload["identifier"])) {
                    payload["identifier"] = Guid.NewGuid().ToString();
                }
                return (string)payload["identifier"];
            }
            set {
                payload["identifier"] = value;
            }
        }

        TextSyndicationContent summary;

        public TextSyndicationContent Summary {
            get {
                return summary;
            }
            set {
                summary = value;
            }
        }

        readonly Collection<SyndicationPerson> contributors;

        public Collection<SyndicationPerson> Contributors {
            get {
                return contributors;
            }
        }

        TextSyndicationContent copyright;

        public TextSyndicationContent Copyright {
            get {
                return copyright;
            }
            set {
                copyright = value;
            }
        }


        Collection<SyndicationLink> links;

        [ElasticProperty(Type = FieldType.Nested)]
        [JsonConverter(typeof(SyndicationLinkCollectionConverter))]
        public Collection<SyndicationLink> Links {
            get {
                return links;

            }
            set {
                links = value;
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

        SyndicationContent content;

        public SyndicationContent Content {
            get {
                return content;
            }
            set {
                content = value;
            }
        }

        public SyndicationElementExtensionCollection ElementExtensions {
            get {
                var elements = payload.Select<KeyValuePair<string, object>, SyndicationElementExtension>(p => {
                    if (p.Key == "identifier" || p.Key == "date")
                        return null;
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    dic.Add(p.Key, p.Value);
                    string json = JsonConvert.SerializeObject(dic);
                    return new SyndicationElementExtension(JsonConvert.DeserializeXNode(json).CreateReader());
                }).Where(y => y != null);
                return new SyndicationElementExtensionCollection(elements);
            }
            set {
                throw new NotImplementedException();
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

        string sortKey;

        public string SortKey {
            get {
                if (sortKey == null)
                    return LastUpdatedTime.ToUniversalTime().ToString("O");
                return sortKey.ToString();
            }
            set {
                sortKey = value;
            }
        }

        public TypeNameMarker TypeName { get; set; }

        public IElasticItem ReadElasticJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer) {

            return new GenericJsonItem(serializer.Deserialize<Dictionary<string, object>>(reader));
        }

        public void WriteElasticJson(JsonWriter writer, Newtonsoft.Json.JsonSerializer serializer) {
            serializer.Serialize(writer, payload);
        }

        #endregion
       
        public int CurrentVersion {
            get {
                return 1;
            }
        }

        public IElasticObject UpgradeObject(IElasticObject obj) {
            return obj;
        }
    }
}

