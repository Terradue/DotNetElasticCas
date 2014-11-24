using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
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
using Terradue.ElasticCas.Exceptions;
using Terradue.ElasticCas.OpenSearch;
using Nest;

namespace Terradue.ElasticCas.Services {
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
            IElasticDocument document = ElasticCasFactory.GetElasticDocumentByTypeName(routeDefinition.TypeName);
            if (document == null) {
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
            for (int i = 0; i < defintionRouteSplitted.Count(); i++) {
                string routeElement = defintionRouteSplitted[i];
                if (!routeElement.StartsWith("{")) {
                    parameters[i] = null;
                    continue;
                }
                string parameter = routeElement.Trim(new char[]{ '{', '}' });
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

            document.ProxyOpenSearchDescription = ecf.GetDefaultOpenSearchDescription(document);
            OpenSearchDescription osd = document.GetProxyOpenSearchDescription();

            OpenSearchDescriptionUrl osdUrl = OpenSearchFactory.GetOpenSearchUrlByTypeAndMaxParam(osd, mimeTypes, osParameters);
            osParameters = OpenSearchFactory.ReplaceTemplateByIdentifier(osParameters, osdUrl);
            osParameters.Add(Request.QueryString);

            document.IndexName = match.Groups["indexName"].Value;

            return OpenSearchService.Query(document, osParameters);

        }

        void RegisterDynamicRoute(string indexName, DynamicOpenSearchRoute route) {
            string newRoute = string.Format("/api/{0}/{1}/{2}", indexName, route.Id, route.RouteFromIndex);
            //AppHost.Instance.Routes.Add<DynamicRouteRequest>(newRoute);
            AppHost.Instance.Dispose();
            //AppHost.Clear();
            //new AppHost().Init();
        }

        IIndexResponse SaveNewRoute(string indexName, DynamicOpenSearchRoute route) {

            return client.Index<DynamicOpenSearchRoute>(route, i => i.Id(route.Id).Index("ec_routes"));
        }

        DynamicOpenSearchRoute LoadRoute(string indexName, string routeId) {

            var response = client.Get<DynamicOpenSearchRoute>(routeId, indexName, "ec_routes");

            if (response.Found) {
                return response.Source;
            } else {
                throw new NotFoundException(string.Format("API route id {0} not found.", routeId));
            }
        }

        public void LoadAllRoutes(IAppHost host) {

            var indices = client.CatIndices(c => c.V(true));
          
            foreach (var index in indices.Records) {

                var routes = client.Search<DynamicOpenSearchRoute>(s => s.Index(index.Index).Type("ec_routes"));

                if (routes.Total > 0) {
                    routes.Documents.FirstOrDefault(r => {
                        string newRoute = string.Format("/api/{0}/{1}/{2}", index, r.Id, r.RouteFromIndex);
                        host.Routes.Add<DynamicRouteRequest>(newRoute, "GET");
                        return false;
                    });
                }
            }


        }
    }
}

