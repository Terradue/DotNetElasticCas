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

namespace Terradue.ElasticCas {

	[Api("OpenSearch Description Service")]
	[Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Xml,
	          EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Xml)]
    public class OpenSearchDescriptionService : BaseService {

		[AddHeader(ContentType = ContentType.Xml)]
		public object Get(OpenSearchDescriptionGetRequest request)
		{
			OpenSearchDescription osd = new OpenSearchDescription();

			osd.ShortName = "Datasets Elastic Catalogue";
			osd.Attribution = "Terradue";
			osd.Contact = "info@terradue.com";
			osd.Developer = "Terradue GeoSpatial Development Team";
			osd.SyndicationRight = "open";
			osd.AdultContent = "false";
			osd.Language = "en-us";
			osd.OutputEncoding = "UTF-8";
			osd.InputEncoding = "UTF-8";
			osd.Description = "This Search Service performs queries in the available dataset catalogue. There are several URL templates that return the results in different formats (GeoJson, RDF, ATOM or KML). This search service is in accordance with the OGC 10-032r3 specification.";

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

