using Funq;
using System;
using System.Web;
using System.Collections;
using System.ComponentModel;
using System.Web.SessionState;
using ServiceStack;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.Common.Web;
using PlainElastic.Net;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.ServiceModel;
using System.Net;
using ServiceStack.ServiceModel.Serialization;
using ServiceStack.WebHost.Endpoints.Formats;
using Terradue.OpenSearch;
using System.IO;
using Terradue.OpenSearch.Engine;
using Mono.Addins;
using Terradue.OpenSearch.Result;
using Terradue.ElasticCas.Model;
using Terradue.ElasticCas.Service;
using System.Collections.Generic;
using Terradue.ElasticCas.Routes;

namespace Terradue.ElasticCas {
    public class AppHost : AppHostBase {
        public System.Configuration.Configuration WebConfig;

        public static OpenSearchEngine OpenSearchEngine { get; private set; }
        //Tell Service Stack the name of your application and where to find your web services
        public AppHost() : base("Elastic Catalogue", typeof(OpenSearchDescriptionService).Assembly) {

            // Initialize the add-in engine
            AddinManager.Initialize();
            AddinManager.Registry.Update(null);
        

            LoadStaticObject();


        }

        public static void Clear()
        {
            Instance = null;
        }

        public override void Configure(Funq.Container container) {
            //register any dependencies your services use, e.g:
            //container.Register<ICacheClient>(new MemoryCacheClient());
            WebConfig =
				System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(null);

            JsConfig.ExcludeTypeInfo = true;
            //JsConfig.ConvertObjectTypesIntoStringDictionary = true;
            JsConfig.ThrowOnDeserializationError = true;
            JsConfig.IncludePublicFields = true;
            JsConfig.EmitCamelCaseNames = true;
            //JsConfig.IncludeTypeInfo = true;

            //Permit modern browsers (e.g. Firefox) to allow sending of any REST HTTP Method
            base.SetConfig(new EndpointHostConfig {
                GlobalResponseHeaders = {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS" },
                    { "Cache-Control", "no-cache, no-store, must-revalidate" },
                    { "Pragma", "no-cache" },
                    { "Expires", "0" }
                },
                EnableAccessRestrictions = true,
                DebugMode = true, //Enable StackTraces in development
                WebHostUrl = WebConfig.AppSettings.Settings["baseUrl"].Value,
                WriteErrorsToResponse = false, //custom exception handling
                DefaultContentType = ServiceStack.Common.Web.ContentType.Json,
                ReturnsInnerException = false,
                MapExceptionToStatusCode = {
                    { typeof(NotFoundException), 404 },
                }

            });


            Plugins.Add(new DynamicOpenSearchRouteModule());

            this.ServiceExceptionHandler = (httpReq, request, ex) => {
                if (EndpointHost.Config != null && EndpointHost.Config.ReturnsInnerException && ex.InnerException != null && !(ex is IHttpError)) {
                    ex = ex.InnerException;
                }
                ResponseStatus responseStatus = ex.ToResponseStatus();
                if (EndpointHost.DebugMode) {
                    responseStatus.StackTrace = DtoUtils.GetRequestErrorBody(request) + "\n" + ex;
                }
                if (ex is OperationException) {
                    if (((OperationException)ex).HttpStatusCode == 404) {
                        responseStatus.ErrorCode = "Not Found";
                        ex = new NotFoundException(ex.Message);
                    }
                }
                return DtoUtils.CreateErrorResponse(request, ex, responseStatus);
            };

            this.ContentTypeFilters.Register("application/opensearchdescription+xml", OpenSearchDescriptionService.OpenSearchDescriptionSerializer, OpenSearchDescriptionService.OpenSearchDescriptionDeserializer);
            //this.ContentTypeFilters.Register("application/opensearchdescription+xml", SerializeToStream, ServiceStack.Text.XmlSerializer.DeserializeFromStream);

            log4net.Config.DOMConfigurator.Configure();

        }

        public void SerializeToStream(IRequestContext requestContext, object request, Stream stream) {
            XmlSerializer.SerializeToStream(request, stream);
        }

        private void LoadStaticObject() {

            OpenSearchEngine = new OpenSearchEngine();
            OpenSearchEngine.LoadPlugins();

            ElasticCasFactory.LoadPlugins(this);

        }
    }
}
