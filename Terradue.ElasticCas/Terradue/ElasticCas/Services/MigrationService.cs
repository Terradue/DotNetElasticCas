using System;
using ServiceStack;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceHost;
using ServiceStack.Text;
using ServiceStack.Common.Web;
using Terradue.ElasticCas.Request;
using Terradue.ElasticCas.Responses;
using System.Linq;
using System.Collections.Generic;

namespace Terradue.ElasticCas.Services {
    [Api("Index Service")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
           EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class MigrationService : BaseService {

        public MigrationService() : base("Index Service") {

        }

        [AddHeader(ContentType = ContentType.Json)]
        public MigrationResponse Put(MigrateOneIndexRequest request) {

            var aliases = client.CatAliases(c => c.FilterPath(request.IndexName));

            if (aliases.Records.Count() == 0) {
                return new MigrationResponse(){ Error = "This is not an alias" };
            }

            var response = ecf.MigrateIndex(aliases.Records.First());

            return response;

        }

        [AddHeader(ContentType = ContentType.Json)]
        public MigrationResponse Get(ScanOneIndexStatusRequest request) {

            var aliases = client.CatAliases(c => c.FilterPath(request.IndexName));

            if (aliases.Records.Count() == 0) {
                return new MigrationResponse(){ Error = "This is not an alias" };
            }

            var response = ecf.MigrateIndex(aliases.Records.First());

            return response;

        }

        [AddHeader(ContentType = ContentType.Json)]
        public List<MigrationResponse> Get(ScanAllIndicesStatusRequest request) {

            return ecf.ScanAllIndices();

        }
    }
}

