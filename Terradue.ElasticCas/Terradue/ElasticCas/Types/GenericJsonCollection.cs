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
using Terradue.ElasticCas.Controllers;
using System.Xml.Linq;
using System.Diagnostics;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Terradue.ElasticCas.Types {

    public class GenericJsonCollection : IElasticCollection {

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

        public IEnumerable<IElasticItem> ElasticItems { get { return items; } }

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

        public IElasticCollection FromOpenSearchResultCollection(IOpenSearchResultCollection results) {

            return CreateFromOpenSearchResultCollection(results);

        }

        public static IElasticCollection CreateFromOpenSearchResultCollection(IOpenSearchResultCollection results) {

            if (results is GenericJsonCollection)
                return (GenericJsonCollection)results;

            GenericJsonCollection collection = new GenericJsonCollection();
            collection.links = results.Links;

            foreach (IOpenSearchResultItem result in results.Items) {
                var item = GenericJsonItem.FromOpenSearchResultItem(result);
                collection.items.Add(item);
            }
            return collection;

        }

        public void SerializeToStream(System.IO.Stream stream) {

            Dictionary<string, object> col = new Dictionary<string, object>();
            col.Add("items", items);

            var baseStream = new System.IO.MemoryStream();
            var serializer = new Newtonsoft.Json.JsonSerializer();
            var sw = new StreamWriter(stream);
            var jsonTextWriter = new JsonTextWriter(sw);
            serializer.Serialize(jsonTextWriter, col);
            jsonTextWriter.Flush();

        }

        public string SerializeToString() {
            MemoryStream ms = new MemoryStream();
            SerializeToStream(ms);
            ms.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(ms);
            var json = sr.ReadToEnd();
            return json;
        }

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

        public Collection<SyndicationPerson> Contributors {
            get {
                throw new NotImplementedException();
            }
        }

        public TextSyndicationContent Copyright {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public TextSyndicationContent Description {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public string Generator {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        internal List <GenericJsonItem> items;

        public IEnumerable<IOpenSearchResultItem> Items {
            get {
                return items.Cast<IOpenSearchResultItem>().ToArray();
            }
            set {
                items = value.Cast<GenericJsonItem>().ToList();
            }
        }

        Collection<Terradue.ServiceModel.Syndication.SyndicationLink> links;

        public Collection<Terradue.ServiceModel.Syndication.SyndicationLink> Links {
            get {
                return links;
            }
            set {
                links = value;
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
            set {
                elementExtensions = value;
            }
        }

        TextSyndicationContent title;

        public TextSyndicationContent Title {
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

            var serializer = new Newtonsoft.Json.JsonSerializer();
            serializer.TypeNameHandling = TypeNameHandling.All;
            serializer.TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;

            Dictionary<string, object> obj = null;
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr)) {
                obj = serializer.Deserialize<Dictionary<string, object>>(jsonTextReader);
            }

            JContainer items = (JContainer)obj["items"];

            GenericJsonCollection collection = new GenericJsonCollection();
            collection.items = new List<GenericJsonItem>();

            foreach (var item in items) {
                GenericJsonItem it = new GenericJsonItem(item.ToObject<Dictionary<string, object>>());
                collection.items.Add(it);
            }

            return collection;

        }

        public static GenericJsonCollection TransformElasticSearchResponseToGenericJsonCollection(OpenSearchResponse<ISearchResponse<GenericJsonItem>> response) {

            ISearchResponse<GenericJsonItem> results = null;

            results = (ISearchResponse<GenericJsonItem>)response.GetResponseObject();

            if (results == null) {
                throw new NotImplementedException("GenericCollection only transforms from an ElasticOpenSearchResponse");
            }

            GenericJsonCollection collection = new GenericJsonCollection();
            collection.items = new List<GenericJsonItem>();

            foreach (var doc in results.Documents) {
                if (doc is GenericJsonItem) {
                    collection.items.Add(doc);
                } else
                    throw new InvalidDataException("Result is not a GenericJson document.");
            }
            collection.ShowNamespaces = true;
            collection.Date = DateTime.UtcNow;
            collection.ElementExtensions.Add(new XElement(XName.Get("totalResults", "http://a9.com/-/spec/opensearch/1.1/"), results.Total).CreateReader());

            return collection;


        }
    }
}

