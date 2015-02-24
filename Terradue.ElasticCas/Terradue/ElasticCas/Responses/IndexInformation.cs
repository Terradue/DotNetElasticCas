using System;
using Nest;
using System.Collections.Generic;

namespace Terradue.ElasticCas.Responses {

    public class IndexInformation {

        public string Name { get; set; }

        public ShardsMetaData Shards { get; set; }

        public Dictionary<string,ICollection<PropertyNameMarker>> Mappings { get; set; }

    }
}

