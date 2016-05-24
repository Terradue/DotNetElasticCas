using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Terradue.ElasticCas {

    public class MigrationResponse {

        public string AliasName { get; set; }

        public string IndexName { get; set; }

        public string Mode { get; set; }

        public List<TypeInformation> TypeStatus { get; set; }

        public string Error { get; set; }

    }

}

