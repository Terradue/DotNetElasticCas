using System;
using PlainElastic.Net.Serialization;
using ServiceStack.Text;

namespace Terradue.ElasticCas {
	public class JsonNetSerializer : IJsonSerializer {
		
		//
		// Methods
		//
		public object Deserialize(string value, Type type) {
            //return JsonConvert.DeserializeObject(value, type, this.Settings);
            return JsonSerializer.DeserializeFromString(value,type);

		}

		public string Serialize(object o) {
            //return JsonConvert.SerializeObject(o, 0, this.Settings);
            return JsonSerializer.SerializeToString<object>(o);

		}
	}
}

