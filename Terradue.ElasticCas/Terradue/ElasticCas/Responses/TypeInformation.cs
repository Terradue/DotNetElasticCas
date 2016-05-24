using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Terradue.ElasticCas {

    public class TypeInformation {

        public string Name  { get; set; }

        public int Version { get; set; }

        public string Description { get; set; }

        public string Message { get; set; }

    }
}
