using System;
using System.Linq;
using ServiceStack;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceHost;
using ServiceStack.Common.Web;
using Terradue.OpenSearch;
using System.Text;
using System.Xml;
using ServiceStack.Text;
using System.IO;
using System.Web;
using System.Collections.Generic;
using PlainElastic.Net;
using PlainElastic.Net.Serialization;
using Terradue.ElasticCas.Request;
using Terradue.OpenSearch.Schema;
using Terradue.ElasticCas.Model;
using Terradue.OpenSearch.Engine;
using System.Collections.Specialized;

namespace Terradue.ElasticCas.Service {

	[Api("OpenSearch Description Service")]
	[Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Xml,
	          EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Xml)]
    public class OpenSearchDescriptionService : BaseService {

        public OpenSearchDescriptionService () : base ("OpenSearch Description Service"){}

		[AddHeader(ContentType = ContentType.Xml)]
		public object Get(OpenSearchDescriptionGetRequest request)
		{
            IElasticDocumentCollection collection = ElasticCasFactory.GetElasticDocumentCollectionByTypeName(request.TypeName);
            if (collection == null) {
                throw new InvalidTypeModelException(request.TypeName, string.Format("Type '{0}' is not found in the type extensions. Check that plugins are loaded", request.TypeName));
            }

            collection.IndexName = request.IndexName;

            OpenSearchDescription osd = ecf.GetOpenSearchDescription(collection);

            return new HttpResult(osd, "application/opensearchdescription+xml");

		}

       

		public static void OpenSearchDescriptionSerializer (IRequestContext reqCtx, object res, IHttpResponse stream)
		{
			stream.AddHeader("Content-Encoding",Encoding.Default.EncodingName);
			using(XmlWriter writer = XmlWriter.Create(stream.OutputStream, new XmlWriterSettings() { OmitXmlDeclaration = false, Encoding = Encoding.Default }))
			{
				new System.Xml.Serialization.XmlSerializer(res.GetType()).Serialize(writer, res);
			}

		}

		public static Object OpenSearchDescriptionDeserializer (Type type, Stream stream) {
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(type);
            return ser.Deserialize( stream );
		}

    }

}

