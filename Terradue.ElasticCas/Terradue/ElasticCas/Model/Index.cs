using System;
using ServiceStack.ServiceHost;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Terradue.ServiceModel.Syndication;

namespace Terradue.ElasticCas.Model
{
    [DataContract]
    public class Index
    {

        public Index(){
            Version = 1;
        }

        public Index(Index index){
            this.Authors = index.Authors;
            this.Description = index.Description;
            this.IndexName = index.IndexName;
            this.Version = index.Version;
        }

        [DataMember(Name = "indexName")]
        public string IndexName { get; set; }

        [DataMember(Name = "description")]
        public string Description {get; set;}

        [DataMember(Name = "authors")]
        public List<Person> Authors { get; set; }

        [DataMember(Name = "version")]
        public int Version { get; set; }

        [DataMember(Name = "created")]
        public DateTime Created { get; set; }

        [DataMember(Name = "lastMigrated")]
        public DateTime LastMigrated { get; set; }

    }
}

