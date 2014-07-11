using System;
using ServiceStack;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceHost;
using PlainElastic.Net;
using ServiceStack.Text;
using PlainElastic.Net.Serialization;

namespace Terradue.ElasticCas.Controller
{
    public class ServiceStackJsonSerializer : IJsonSerializer
    {

        #region IJsonSerializer implementation

        public string Serialize(object o) {
            return JsonSerializer.SerializeToString<object>(o);
        }

        public object Deserialize(string value, Type type) {
            return JsonSerializer.DeserializeFromString(value,type);
        }

        #endregion
    }
}

