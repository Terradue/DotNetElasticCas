using System;
using ServiceStack.ServiceHost;
using System.Runtime.Serialization;
using System.Collections.Generic;
using PlainElastic.Net.Serialization;
using ServiceStack.Text;
using ServiceStack.Text.Json;
using Terradue.GeoJson.Feature;
using Terradue.OpenSearch.Result;
using Terradue.ServiceModel.Syndication;
using Terradue.ElasticCas.Model;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.WebHost.Endpoints.Support;
using Terradue.ElasticCas.Services;

namespace Terradue.ElasticCas.Request {

    public static class Types {

        /// <summary>
        /// Sets the type.
        /// </summary>
        /// <param name="application">Application.</param>
        /// <param name="doc">Document.</param>
        public static void SetType(AppHost application, IElasticDocument doc){

            string baseRoute = "/catalogue/{IndexName}/" + doc.TypeName;
            application.Routes.Add(doc.GetType(), baseRoute + "/{Id}", "PUT");
            application.Routes.Add(doc.GetType(), baseRoute, "POST");

            application.RequestBinders.Add(doc.GetType(), httpReq => {
                IElasticDocument requestDto = (IElasticDocument)EndpointHandlerBase.DeserializeHttpRequest(doc.GetType(), httpReq, httpReq.ContentType);
                requestDto.IndexName = httpReq.PathInfo.Split('/')[2];
                return new SingleIngestionRequest{doc=requestDto};
            });
            EndpointHost.Metadata.Add(typeof(TypesIngestionService), doc.GetType(), typeof(IElasticDocument));

        }

        /// <summary>
        /// Sets the type.
        /// </summary>
        /// <param name="application">Application.</param>
        /// <param name="docs">Documents.</param>
        public static void SetType(AppHost application, IElasticDocumentCollection docs){

            string baseRoute = "/catalogue/{IndexName}/" + docs.TypeName;
            application.Routes.Add(typeof(IElasticDocumentCollection), baseRoute + "/_bulk", "POST");

        }
       
    }

    public class SingleIngestionRequest : IReturn<IElasticDocument> {
        public IElasticDocument doc;
    }


    [Route("/catalogue/{IndexName}/{TypeName}/{Id}", "GET")]
    public class TypeGetRequest : IReturn<IElasticDocument> {
        public string IndexName { get; set; }

        public string TypeName { get; set; }

        public string Id { get; set; }
    }

    [Route("/catalogue/{IndexName}/{TypeName}/_import", "POST")]
    public class TypeImportRequest : IReturn<IElasticDocumentCollection> {
        public string IndexName { get; set; }

        public string TypeName { get; set; }

        public string url { get; set; }
    }
}

