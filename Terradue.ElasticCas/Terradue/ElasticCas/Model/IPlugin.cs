using System;
using Mono.Addins;


namespace Terradue.ElasticCas.Plugins {

    [TypeExtensionPoint()]
    public interface IPlugin : ServiceStack.WebHost.Endpoints.IPlugin {
    }
}

