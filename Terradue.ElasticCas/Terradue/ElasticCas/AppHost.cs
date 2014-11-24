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
using Terradue.ElasticCas.Services;
using Terradue.ElasticCas.Routes;
using log4net;
using Terradue.ServiceModel.Syndication;
using Terradue.ElasticCas.Controller;
using Terradue.ElasticCas.Types;

namespace Terradue.ElasticCas {
    public class AppHost : AppHostBase {

        public System.Configuration.Configuration WebConfig;
        public readonly ILog Logger; 

        public static OpenSearchEngine OpenSearchEngine { get; private set; }
        //Tell Service Stack the name of your application and where to find your web services
        public AppHost() : base("Elastic Catalogue", typeof(OpenSearchDescriptionService).Assembly) {

            log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

            // Initialize the add-in engine
            AddinManager.Initialize();
            AddinManager.Registry.Update(null);

            LoadStaticObject();

            Logger = LogManager.GetLogger("AppHost");

        }

        public override void Configure(Funq.Container container) {

            Logger.Info("Reading global configuration");
            //register any dependencies your services use, e.g:
            //container.Register<ICacheClient>(new MemoryCacheClient());
            WebConfig =
				System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(null);

            JsConfig.ExcludeTypeInfo = true;
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;
            JsConfig.ThrowOnDeserializationError = true;
            JsConfig.IncludePublicFields = true;
            JsConfig.EmitCamelCaseNames = true;
            JsConfig<DateTime>.SerializeFn = time => time.ToString("yyyy-MM-ddTHH:mm:ssZ");
            JsConfig.AssumeUtc = true;
            JsConfig<GenericJson>.RawSerializeFn = GenericJson.ToJson;
            JsConfig<GenericJsonCollection>.RawSerializeFn = GenericJsonCollection.ToJson;

            //JsConfig.IncludeTypeInfo = true;

            Logger.Info("Configure ServiceStack EndpointHostConfig");
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
                WriteErrorsToResponse = true,
                DefaultContentType = ServiceStack.Common.Web.ContentType.Json,
                MapExceptionToStatusCode = {
                    { typeof(NotFoundException), 404 },
                }
            });

            Logger.Info("Load Plugins");
            LoadPlugins();

            Logger.Info("Configure Service Exception Handler");
            this.ServiceExceptionHandler = (httpReq, request, ex) => {
                if (EndpointHost.Config != null && EndpointHost.Config.ReturnsInnerException && ex.InnerException != null && !(ex is IHttpError)) {
                    ex = ex.InnerException;
                }
                ResponseStatus responseStatus = ex.ToResponseStatus();
                if (EndpointHost.DebugMode) {
                    responseStatus.StackTrace = DtoUtils.GetRequestErrorBody(request) + "\n" + ex;
                }
                return DtoUtils.CreateErrorResponse(request, ex, responseStatus);
            };

            Logger.Info("Register ContentType Filters");
            this.ContentTypeFilters.Register("application/opensearchdescription+xml", OpenSearchDescriptionService.OpenSearchDescriptionSerializer, OpenSearchDescriptionService.OpenSearchDescriptionDeserializer);
            this.ContentTypeFilters.Register("application/atom+xml", OpenSearchQueryRequestService.SerializeToStream, OpenSearchQueryRequestService.DeserializeFromStream);


            this.PreRequestFilters.Insert(0, (httpReq, httpRes) => {
                httpReq.UseBufferedStream = true;
            });


        }

        private void LoadStaticObject() {

            OpenSearchEngine = new OpenSearchEngine();
            OpenSearchEngine.LoadPlugins();

            ElasticCasFactory.LoadPlugins(this);

        }

        void LoadPlugins() {

            Logger.Info("Load DynamicOpenSearchRouteModule");
            Plugins.Add(new DynamicOpenSearchRouteModule());

            foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(Terradue.ElasticCas.Model.IPlugin))) {
                Terradue.ElasticCas.Model.IPlugin plugin = (Terradue.ElasticCas.Model.IPlugin)node.CreateInstance();
                Logger.Info("Load " + node.Id);
                Plugins.Add(plugin);
            }

        }
    }
}
