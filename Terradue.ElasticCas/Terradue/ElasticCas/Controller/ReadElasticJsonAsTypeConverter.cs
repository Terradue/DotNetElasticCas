using System;
using Nest;
using Newtonsoft.Json;
using Terradue.ElasticCas.Model;

namespace Terradue.ElasticCas.Controller
{
    public class ReadElasticJsonAsTypeConverter<T> : JsonConverter where T : class, IElasticDocument, new()
    {
        #region implemented abstract members of JsonConverter

        public override bool CanRead { get { return true; } }
        public override bool CanWrite { get { return true; } }

        public override bool CanConvert(Type objectType)
        {
            return true; 
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            var t = new T();
            var customReader = t as IElasticDocument;
            if (customReader != null)
                return customReader.FromElasticJson(reader, objectType, existingValue, serializer);

            serializer.Populate(reader, t);
            return t;
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer) {
            if (value is IElasticDocument) {
                ((IElasticDocument)value).ToElasticJson(writer, serializer);
            } else {
                serializer.Serialize(writer, value);
            }
        }

        #endregion
    }
}

