using System;
using ServiceStack.ServiceHost;
using ServiceStack.Text;
using System.Runtime.Serialization;
using System.Collections.Generic;
using ServiceStack.WebHost.Endpoints;
using Terradue.ElasticCas.Service;

namespace Terradue.ElasticCas.Routes {

    [DataContract]
    public class DynamicOpenSearchRoute {

        string id = Guid.NewGuid().ToString();

        [DataMember(Name="id")]
        [ApiMember(Name="id")]
        public string Id {
            get {
                return id;
            }
            set {
                id = value;
            }
        }

        [DataMember(Name="typeName")]
        [ApiMember(Name="typeName")]
        public string TypeName { get; set; }

        [DataMember(Name="routeFromIndex")]
        [ApiMember(Name="routeFromIndex")]
        public virtual string RouteFromIndex { get; set; }

        [DataMember(Name="routeParameters")]
        [ApiMember(Name="routeParameters")]
        public virtual Dictionary<string, RouteParameter> RouteParameters { get; set; }

        [DataMember(Name="queryStringParameters")]
        [ApiMember(Name="queryStringParameters")]
        public virtual Dictionary<string, string> QueryStringParameters { get; set; }


    }

    [DataContract]
    public class RouteParameter {

        [DataMember(Name="osParamId")]
        [ApiMember(Name="osParamId")]
        public virtual string OpenSearchParameterId { get; set; }

    }

    public class DynamicOpenSearchRouteModule : IPlugin
    {
        public void Register(IAppHost appHost)
        {
            new RoutingService().LoadAllRoutes(appHost);
        }
    }
}

