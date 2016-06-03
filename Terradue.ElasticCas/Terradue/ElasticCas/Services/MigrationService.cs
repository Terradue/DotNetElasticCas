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

            var aliases = client.CatAliases();

            if (aliases.Records != null && aliases.Records.Any() && aliases.Records.Where(a => a.Alias == request.IndexName).Any()) {

                var response = ecf.MigrateIndex(aliases.Records.Where(a => a.Alias == request.IndexName).First());
                return response;

            }

            return new MigrationResponse(){ Error = "Index Not found" };

        }

        [AddHeader(ContentType = ContentType.Json)]
        public MigrationResponse Get(ScanOneIndexStatusRequest request) {

            var aliases = client.CatAliases();

            if (aliases.Records != null && aliases.Records.Any() && aliases.Records.Where(a => a.Alias == request.IndexName).Any()) {

                var response = ecf.ScanIndex(aliases.Records.Where(a => a.Alias == request.IndexName).First());
                return response;

            }

            return new MigrationResponse(){ Error = "Index Not found" };
            

        }

        [AddHeader(ContentType = ContentType.Json)]
        public List<MigrationResponse> Get(ScanAllIndicesStatusRequest request) {

            return ecf.ScanAllIndices();

        }
    }
}

