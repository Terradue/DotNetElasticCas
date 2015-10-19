using System;
using Terradue.ElasticCas.Model;
using ServiceStack.ServiceHost;
using Terradue.ElasticCas.Request;
using ServiceStack.ServiceInterface;
using ServiceStack.Common.Web;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch;
using System.Web;
using System.Collections.Specialized;
using System.IO;
using Terradue.ServiceModel.Syndication;
using System.Xml;
using Terradue.ElasticCas.Types;
using Terradue.ElasticCas.Controllers;
using Terradue.ElasticCas.OpenSearch;
using Nest;

namespace Terradue.ElasticCas.Services {

    [Api("OpenSearch Query Service")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class OpenSearchQueryRequestService : BaseService {

        public OpenSearchQueryRequestService() : base("OpenSearch Query Service") {
        }

        public object Get(OpenSearchQueryRequest request) {

            IOpenSearchableElasticType type = ecf.GetOpenSearchableElasticTypeByNameOrDefault(request.IndexName, request.TypeName);

            NameValueCollection parameters = new NameValueCollection(Request.QueryString);
            if (request.AdditionalParameters != null) {
                foreach (var key in request.AdditionalParameters.AllKeys) {
                    parameters.Set(key, request.AdditionalParameters[key]);
                }
            }

            return new OpenSearchService().Query(type, parameters);
        }

        public static void SerializeToStream(IRequestContext requestContext, 
                                             object response, Stream stream) {
            if (!(response is SyndicationFeed))
                ServiceStack.Text.XmlSerializer.SerializeToStream(response, stream);

            var sw = XmlWriter.Create(stream);
            Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter((SyndicationFeed)response);
            atomFormatter.WriteTo(sw);
            sw.Flush();
            sw.Close();
        }

        public static object DeserializeFromStream(Type type, Stream stream) {
            if (type == typeof(SyndicationFeed)) {
                var sw = XmlReader.Create(stream);
                Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter();
                atomFormatter.ReadFrom(sw);
                sw.Close();
                return atomFormatter.Feed;
            } else {
                try {
                    return ServiceStack.Text.XmlSerializer.DeserializeFromStream(type, stream);
                }
                catch (Exception e){
                    stream.Seek(0, SeekOrigin.Begin);
                    return Activator.CreateInstance(type);
                }
            }
        }
    }
}

