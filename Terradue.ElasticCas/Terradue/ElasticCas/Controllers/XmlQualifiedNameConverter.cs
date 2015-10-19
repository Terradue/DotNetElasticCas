using System;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Terradue.ElasticCas {
    public class XmlQualifiedNameConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(XmlQualifiedName);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer) {

            var json = serializer.Deserialize<string>(reader);
            string name;
            string ns;

            if (json.Contains(":")) {
                var qn = json.Split(':').ToList();
                name = qn.Last();
                ns = json.Replace(":" + name, "");
                return new XmlQualifiedName(name, ns);
            } else {
                name = json;
                return new XmlQualifiedName(name);
            }

        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer) {

            serializer.Serialize(writer, value);

        }
    }
}

