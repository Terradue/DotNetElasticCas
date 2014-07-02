using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Terradue.ElasticCas.Model {

    [DataContract]
    public class Metadata {

        public Metadata() {
        }

        public Dictionary<string,Mapping> Mappings { get; set; }
    }
}

