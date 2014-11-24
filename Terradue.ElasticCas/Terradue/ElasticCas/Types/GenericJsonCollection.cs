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
using System.Xml.Linq;
using System.Diagnostics;
using Nest;

namespace Terradue.ElasticCas.Types {

    public class GenericJsonCollection : IElasticDocumentCollection {

        public GenericJsonCollection() {

            links = new Collection<Terradue.ServiceModel.Syndication.SyndicationLink>();
            elementExtensions = new SyndicationElementExtensionCollection();
            authors = new Collection<SyndicationPerson>();

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

            Collection<GenericJson> docs = new Collection<GenericJson>(FromOpenSearchResultCollection(results).items);
            foreach (var doc in docs) {
                doc.TypeName = this.TypeName;
            }

            return new Collection<IElasticDocument>(docs.Cast<IElasticDocument>().ToList());

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

        public string ContentType {
            get {
                return "application/json";
            }
        }

        public bool ShowNamespaces {
            get {
                return false;
            }
            set {

            }
        }

        public long TotalResults {
            get {
                return Items.Count();
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

            if (response is ElasticOpenSearchResponse<GenericJson>) {
                var results = ((ElasticOpenSearchResponse<GenericJson>)response).GetSearchResponse();

                GenericJsonCollection collection = new GenericJsonCollection();
                collection.items = new List<GenericJson>();

                foreach (var doc in results.Documents) {
                    if ( doc is GenericJson) {
                        collection.items.Add((GenericJson)doc);
                    }
                    else
                        throw new InvalidDataException("Result is not a GenericJson document.");
                }
                collection.ShowNamespaces = true;
                collection.Date = DateTime.UtcNow;
                collection.ElementExtensions.Add(new XElement(XName.Get("totalResults", "http://a9.com/-/spec/opensearch/1.1/"), ((ElasticOpenSearchResponse<GenericJson>)response).TotalResult).CreateReader());

                return collection;


            } else {
                throw new NotImplementedException("GenericCollection only transforms from an ElasticOpenSearchResponse");
            }
        }
    }
}

