using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using Nest;
using Newtonsoft.Json;
using Terradue.OpenSearch;
using Terradue.OpenSearch.Schema;
using Terradue.ServiceModel.Syndication;

namespace Terradue.ElasticCas.Types {

    public interface IElasticObject {

        DateTime Modified { get; set; }

        Collection<SyndicationLink> Links {
            get ;
            set ;
        }

        int CurrentVersion {
            get ;
        }

    }

}

