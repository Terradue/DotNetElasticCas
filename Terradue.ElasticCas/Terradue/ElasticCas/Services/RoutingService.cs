using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using PlainElastic.Net;
using PlainElastic.Net.Serialization;
using ServiceStack.Common;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints;
using Terradue.OpenSearch;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Schema;
using Terradue.ElasticCas.Controller;
using Terradue.ElasticCas.Model;
using Terradue.ElasticCas.Request;
using Terradue.ElasticCas.Routes;

namespace Terradue.ElasticCas.Service {
    [Api("Routing Service")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class RoutingService : BaseService {

        public RoutingService() : base("Routing Service") {

        }

        [AddHeader(ContentType = ContentType.Json)]
        public CreateNewRoute Post(CreateNewRoute request) {

            try {
                RegisterDynamicRoute(request.IndexName, request);
            } catch (Exception e) {
                throw e;
            }

            SaveNewRoute(request.IndexName, request);

            return request;

        }

        [AddHeader(ContentType = ContentType.Json)]
        public object Get(DynamicRouteRequest request) {

            // Match the route request to validate the route requested
            Match match = Regex.Match(Request.GetPhysicalPath(), @"\/api\/(?<indexName>\w*)\/(?<routeId>\w*)\/(?<route>.*)");
            DynamicOpenSearchRoute routeDefinition = null;

            // Load the dynamic route according to the index and the route id
            if (match.Success) {
                routeDefinition = LoadRoute(match.Groups["indexName"].Value, match.Groups["routeId"].Value);
            } else {
                throw new InvalidOperationException(string.Format("incorrect API route: {0}", Request.GetPhysicalPath()));
            }

            // Loads the type of the dynamic route
            IElasticDocumentCollection collection = ElasticCasFactory.GetElasticDocumentCollectionByTypeName(routeDefinition.TypeName);
            if (collection == null) {
                throw new InvalidTypeModelException(routeDefinition.TypeName, string.Format("Type '{0}' is not found in the type extensions. Check that plugins are loaded", routeDefinition.TypeName));
            }

            // split the route defintion
            string[] defintionRouteSplitted = routeDefinition.RouteFromIndex.Split('/');
            // check that requested route elements are respected
            string[] requestRouteSplitted = match.Groups["route"].Value.Split('/');
            if (requestRouteSplitted.Count() != defintionRouteSplitted.Count()) 
                throw new InvalidOperationException(string.Format("Incorrect API request: {0}. Must match defintion: {1}", match.Groups["route"].Value, routeDefinition.RouteFromIndex));

            // build a parameters table with params defintion
            RouteParameter[] parameters = new RouteParameter[defintionRouteSplitted.Count()];
            for (int i=0; i<defintionRouteSplitted.Count(); i++ ) {
                string routeElement = defintionRouteSplitted[i];
                if (!routeElement.StartsWith("{")) {
                    parameters[i] = null;
                    continue;
                }
                string parameter = routeElement.Trim(new char[]{'{', '}'});
                if (string.IsNullOrEmpty(parameter))
                    throw new InvalidOperationException(string.Format("API route defintion error: {0}. Empty parameter.", routeDefinition.RouteFromIndex));
                if (!routeDefinition.RouteParameters.ContainsKey(parameter)) 
                    throw new InvalidOperationException(string.Format("API route defintion error: {0}. Undefined parameter: {1}", routeDefinition.RouteFromIndex, routeElement));

                parameters[i] = routeDefinition.RouteParameters[parameter];
            }



            // Now from requested route rebuild the opensearch request
            UriBuilder url = new UriBuilder(ecf.RootWebConfig.AppSettings.Settings["baseUrl"].Value);
            url.Path += string.Format("/catalogue/{0}/{1}/search", match.Groups["indexName"].Value, routeDefinition.TypeName);
            NameValueCollection osParameters = HttpUtility.ParseQueryString("");
            for (int i = 0; i < requestRouteSplitted.Count(); i++) {
                if (parameters[i] == null)
                    continue;
                osParameters.Add(parameters[i].OpenSearchParameterId, requestRouteSplitted[i]);
            }

            List<string> mimeTypes = Request.AcceptTypes.ToList();

            if (osParameters.AllKeys.Contains("enctype"))
                mimeTypes.Add(osParameters["enctype"]);

            collection.ProxyOpenSearchDescription = ecf.GetOpenSearchDescription(collection);
            OpenSearchDescription osd = collection.GetProxyOpenSearchDescription();

            OpenSearchDescriptionUrl osdUrl = OpenSearchFactory.GetOpenSearchUrlByTypeAndMaxParam(osd, mimeTypes, osParameters);
            osParameters = OpenSearchFactory.ReplaceTemplateByIdentifier(osParameters, osdUrl);
            osParameters.Add(Request.QueryString);

            collection.IndexName = match.Groups["indexName"].Value;

            return OpenSearchService.Query(collection, osParameters);

        }

        void RegisterDynamicRoute(string indexName, DynamicOpenSearchRoute route) {
            string newRoute = string.Format("/api/{0}/{1}/{2}", indexName, route.Id, route.RouteFromIndex);
            //AppHost.Instance.Routes.Add<DynamicRouteRequest>(newRoute);
            AppHost.Instance.Dispose();
            //AppHost.Clear();
            //new AppHost().Init();
        }

        string SaveNewRoute(string indexName, DynamicOpenSearchRoute route) {

            string command = new IndexCommand(index: indexName, type: "ec_routes", id: route.Id);

            try {
                return esConnection.Post(command, route.ToJson());
            } catch (Exception e) {
                throw e;
            }
        }

        DynamicOpenSearchRoute LoadRoute(string indexName, string routeId) {

            OperationResult response = null;
            string command = new IndexCommand(indexName, "ec_routes", routeId);

            try {
                response = esConnection.Get(command);
            } catch (Exception e) {
                throw e;
            }

            ServiceStackJsonSerializer ser = new ServiceStackJsonSerializer();
            var results = ser.ToGetResult<DynamicOpenSearchRoute>(response);

            if (results._id == routeId) {
                return results.Document;
            } else {
                throw new NotFoundException(string.Format("API route id {0} not found.", routeId));
            }
        }

        public void LoadAllRoutes(IAppHost host) {


            OperationResult response = null;

            try {
                response = esConnection.Get("_aliases");
            } catch (Exception e) {
                throw e;
            }

            Dictionary<string, object> aliases = response.Result.FromJson<Dictionary<string, object>>();

            foreach (var indexName in aliases.Keys) {

                var command = new SearchCommand(index: indexName, type: "ec_routes");

                try {
                    OperationResult result = esConnection.Get(command);
                    ServiceStackJsonSerializer ser = new ServiceStackJsonSerializer();
                    var results = ser.ToSearchResult<DynamicOpenSearchRoute>(result);
                    if (results.hits.total > 0) {
                        results.Documents.FirstOrDefault(r => {
                            string newRoute = string.Format("/api/{0}/{1}/{2}", indexName, r.Id, r.RouteFromIndex);
                            host.Routes.Add<DynamicRouteRequest>(newRoute, "GET");
                            return false;
                        });
                    }
                } catch (Exception e) {
                    throw e;
                }
            }


        }
    }
}

