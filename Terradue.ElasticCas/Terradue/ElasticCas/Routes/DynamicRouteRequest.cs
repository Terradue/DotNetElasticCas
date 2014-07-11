using System;
using System.Reflection.Emit;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using ServiceStack.ServiceHost;
using System.IO;

namespace Terradue.ElasticCas.Routes {
    public class DynamicRouteRequest : IRequiresRequestStream
    {
        #region IRequiresRequestStream implementation

        public Stream RequestStream { get; set; }

        #endregion


    }

}

