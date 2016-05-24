using System;
using Newtonsoft.Json;
using System.Xml;
using Terradue.ServiceModel.Syndication;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace Terradue.ElasticCas.Converters {
    public class SyndicationLinkCollectionConverter : JsonConverter {

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(SyndicationLink);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer) {

            List<SyndicationLink> links = new List<SyndicationLink>();

            JArray jarray = serializer.Deserialize<JArray>(reader);

            foreach (JToken element in jarray.Children()) {

                var json = element.ToObject<JObject>();

                var link = new SyndicationLink();
                if (json.Property("uri") != null)
                    link.Uri = new Uri(json.Property("uri").Value.ToString());
                else
                    link.Uri = new Uri("http://localhost");
                if (json.Property("title") != null)
                    link.Title = json.Property("title").Value.ToString();
                if (json.Property("relationshipType") != null)
                    link.RelationshipType = json.Property("relationshipType").Value.ToString();
                if (json.Property("mediaType") != null)
                    link.MediaType = json.Property("mediaType").Value.ToString();
                if (json.Property("length") != null)
                    link.Length = json.Property("length").Value.Value<long>();
                //link.ElementExtensions.Add = serializer.Deserialize(json.Property("elementExtensions").CreateReader(), typeof(SyndicationElementExtensionCollection));

                if (json.Property("attributeExtensions") != null && json.Property("attributeExtensions").Value.HasValues) {
                    foreach (var item in json.Property("attributeExtensions").Value.Value<JObject>().Properties()) {

                        string ns = null;
                        string name = "";
                        if (item.Name.Contains(":")) {
                            var qn = item.Name.Split(':').ToList();
                            name = qn.Last();
                            ns = item.Name.Replace(":" + name, "");
                            link.AttributeExtensions.Add(new XmlQualifiedName(name, ns), item.Value.Value<string>());
                        } else {
                            name = item.Name;
                            link.AttributeExtensions.Add(new XmlQualifiedName(name), item.Value.Value<string>());
                        }
                    }
                }
                links.Add(link);
            }



            return new Collection<SyndicationLink>(links);

        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer) {

            serializer.Serialize(writer, value);

        }
    }
}

