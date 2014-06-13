using System;
using Newtonsoft.Json;
using PlainElastic.Net.Serialization;
using Newtonsoft.Json.Converters;

namespace Terradue.ElasticCas {
	public class JsonNetSerializer : IJsonSerializer {
		//
		// Properties
		//
		public JsonSerializerSettings Settings {
			get;
			set;
		}
		//
		// Constructors
		//
		public JsonNetSerializer() {
			this.Settings = new JsonSerializerSettings();
			this.Settings.Converters.Add(new IsoDateTimeConverter());
			//this.Settings.Converters.Add(new Newtonsoft.Json.Converters.FacetCreationConverter());
			this.Settings.NullValueHandling = NullValueHandling.Ignore;
		}
		//
		// Methods
		//
		public object Deserialize(string value, Type type) {
			return JsonConvert.DeserializeObject(value, type, this.Settings);
		}

		public string Serialize(object o) {
			return JsonConvert.SerializeObject(o, 0, this.Settings);
		}
	}
}

