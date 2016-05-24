using System;
using ServiceStack.ServiceHost;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Terradue.ServiceModel.Syndication;

namespace Terradue.ElasticCas.Model
{
    [DataContract]
	public class Person
	{
        [DataMember(Name = "name")]
        public string Name {get; set;}

        [DataMember(Name = "email")]
        public string Email {get; set;}

        [DataMember(Name = "uri")]
        public string Uri {get; set;}

	}

}

