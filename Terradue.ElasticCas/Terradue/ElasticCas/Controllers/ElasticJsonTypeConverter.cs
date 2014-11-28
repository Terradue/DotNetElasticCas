
using System;
using Newtonsoft.Json;
using Terradue.ElasticCas.Model;
using System.Linq;

namespace Terradue.ElasticCas.Controllers {

    public class ElasticJsonTypeConverter : JsonConverter {

        public override bool CanRead { get { return true; } }

        public override bool CanWrite { get { return true; } }

        public override bool CanConvert(Type objectType) {
            return true;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            if (value is IElasticItem) {

                ((IElasticItem)value).WriteElasticJson(writer, serializer);

            } else {
                throw new InvalidOperationException(string.Format("ElasticJsonTypeConverter supports only IElasticItem"));
            }

        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {

            if (objectType.GetInterfaces().Contains(typeof(IElasticItem))) {
                var ctor = objectType.GetConstructor(new Type[0]);
                IElasticItem item = (IElasticItem)ctor.Invoke(null);
                return item.ReadElasticJson(reader, objectType, existingValue, serializer);
            } else {
                throw new InvalidOperationException(string.Format("ElasticJsonTypeConverter supports only IElasticItem"));
            }

        }
    }
}

