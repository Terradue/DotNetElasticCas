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

            OpenSearchDescription osd = new OpenSearchDescription();

            osd.ShortName = collection.TypeName + " Elastic Catalogue";
            osd.Attribution = "Terradue";
            osd.Contact = "info@terradue.com";
            osd.Developer = "Terradue GeoSpatial Development Team";
            osd.SyndicationRight = "open";
            osd.AdultContent = "false";
            osd.Language = "en-us";
            osd.OutputEncoding = "UTF-8";
            osd.InputEncoding = "UTF-8";
            osd.Description = string.Format("This Search Service performs queries in the index {0}. There are several URL templates that return the results in different formats." +
                                            "This search service is in accordance with the OGC 10-032r3 specification.", request.IndexName);

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            var osee = ose.GetFirstExtensionByTypeAbility(collection.GetOpenSearchResultType());
            if (osee == null) {
                throw new InvalidTypeSearchException(request.TypeName, string.Format("OpenSearch Engine for Type '{0}' is not found in the extensions. Check that plugins are loaded", request.TypeName));
            }

            var searchExtensions = ose.Extensions;
            List<OpenSearchDescriptionUrl> urls = new List<OpenSearchDescriptionUrl>();

            NameValueCollection parameters = collection.GetOpenSearchParameters(collection.DefaultMimeType);

            UriBuilder searchUrl = new UriBuilder(string.Format("{0}/catalogue/{1}/{2}/search", ecf.RootWebConfig.AppSettings.Settings["baseUrl"].Value, request.IndexName, request.TypeName));
            NameValueCollection queryString = HttpUtility.ParseQueryString("?format=format");
            parameters.AllKeys.FirstOrDefault(k => {
                queryString.Add(parameters[k], "{"+k+"?}");
                return false;
            });

            foreach (Type type in searchExtensions.Keys) {

                queryString.Set("format", searchExtensions[type].Identifier);
                searchUrl.Query = queryString.ToString();
                urls.Add(new OpenSearchDescriptionUrl(searchExtensions[type].DiscoveryContentType, 
                                                      searchUrl.ToString(),
                                                      "search"));

            }
            osd.Url = urls.ToArray();

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

