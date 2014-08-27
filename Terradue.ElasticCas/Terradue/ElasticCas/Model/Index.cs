using System;
using ServiceStack.ServiceHost;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Terradue.ElasticCas.Model
{
    [DataContract]
    public class Index
    {

        [DataMember(Name = "indexName")]
        public string IndexName { get; set; }

        [DataMember(Name = "typeNames")]
        public string[] TypeNames { get; set; }

        [DataMember(Name = "description")]
        public string Description {get; set;}

        [DataMember(Name = "creator")]
        public Creator Creator {get; set;}


    }

    [DataContract]
    public class Creator
    {

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "email")]
        public string[] TypeNames { get; set; }

        [DataMember(Name = "uri")]
        public string Description { get; set; }


    }
}

