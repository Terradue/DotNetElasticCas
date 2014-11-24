using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Terradue.ElasticCas.Model
{
    [DataContract]
	public class Mapping
	{
        [DataMember(Name = "id")]
        public string Identifier { get; set; }

        [DataMember(Name = "qname")]
        public string QualifiedName { get; set; }

        [DataMember(Name = "query")]
        public string QueryDslPattern { get; set; }

	}
}

