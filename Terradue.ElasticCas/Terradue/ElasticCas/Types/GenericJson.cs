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
using System.Diagnostics;


namespace Terradue.ElasticCas.Types {

    [Extension(typeof(IElasticDocument))]
    [ExtensionNode("Generic", "Document representing generic data")]
    [DataContract]
    public class GenericJson : IElasticDocument {

        public GenericJson() {
            this.showNamespaces = true;
            this.elementExtensions = new SyndicationElementExtensionCollection();
            this.links = new Collection<SyndicationLink>();
        }

        public GenericJson(JsonObject item) : this() {
            foreach (string ext in item.Keys) {
                var test = item.GetUnescaped(ext);
                this.ElementExtensions.Add(ext, "", test);

            }
        }

        string id;
        public string Id {
            get {
                if (id == null)
                    return "";
                else
                    return Identifier;
            }
            set {
                id = value;
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

            return JsonSerializer.SerializeToString(json);

        }

        #region IElasticDocument implementation

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

        public string GetMapping() {
            return new MapBuilder<GenericJson>()
                        .RootObject
                (typeName: typeName,
                 map: r => r
                 .All(a => a.Enabled(true))
                 .Dynamic(true))
                .Build();
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

        #endregion
       
    }
}

