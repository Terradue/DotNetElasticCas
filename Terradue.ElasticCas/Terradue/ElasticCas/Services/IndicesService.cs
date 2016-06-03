using System;
using ServiceStack;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceHost;
using ServiceStack.Text;
using ServiceStack.Common.Web;
using Terradue.ElasticCas.Request;
using Terradue.ElasticCas.Responses;
using System.Linq;

namespace Terradue.ElasticCas.Services {
    [Api("Index Service")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class IndexService : BaseService {

        public IndexService() : base("Index Service") {

        }

        [AddHeader(ContentType = ContentType.Json)]
        public IndexInformation Put(CreateIndexRequest request) {
            var indexInformation = ecf.CreateCatalogueIndex(request);
            client.Alias(a => a.Add(s => s.Index(indexInformation.Name).Alias(request.IndexName)));

            return indexInformation;

        }

        [AddHeader(ContentType = ContentType.Json)]
        public IndexInformation Delete(DeleteIndexRequest request) {

            var aliases = client.CatAliases();

            if (aliases.Records != null && aliases.Records.Any() && aliases.Records.Where(a => a.Alias == request.IndexName).Any()) {

                if (request.IndexName != request.ConfirmIndexName)
                    throw new InvalidOperationException("index name is not confirmed correctly");

                var response = client.DeleteIndex(d => d.Index(aliases.Records.Where(a => a.Alias == request.IndexName).First().Index));
                if (response.IsValid)
                    return new IndexInformation(){ Name = request.IndexName, Message = "deleted" };

            }

            throw new InvalidOperationException("Index Not found");

        }
    }
}

