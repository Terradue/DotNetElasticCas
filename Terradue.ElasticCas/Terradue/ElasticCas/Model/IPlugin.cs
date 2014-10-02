using System;
using Mono.Addins;


namespace Terradue.ElasticCas.Model {

    [TypeExtensionPoint()]
    public interface IPlugin : ServiceStack.WebHost.Endpoints.IPlugin {
    }
}

