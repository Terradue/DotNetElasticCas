using System;
using ServiceStack.ServiceHost;
using Terradue.ElasticCas.Model;
using Nest;
using System.Collections.Generic;

namespace Terradue.ElasticCas.Request {


    [Route("/catalogue/{IndexName}/migration", "PUT")]
    public class MigrateOneIndexRequest : IReturn<IndexStatus>{

        public string IndexName { get; set; }
    }

    [Route("/catalogue/migration", "PUT")]
    public class MigrateAllIndicesRequest : IReturn<IndexStatus>{

        public string IndexName { get; set; }
    }

    [Route("/catalogue/{IndexName}/migration", "GET")]
    public class ScanOneIndexStatusRequest : IReturn<MigrationResponse>{

        public string IndexName { get; set; }
    }

    [Route("/catalogue/migration", "GET")]
    public class ScanAllIndicesStatusRequest : IReturn<List<MigrationResponse>>{

    }
}