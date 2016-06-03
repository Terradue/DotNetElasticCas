using System;
using Nest;
using Terradue.ElasticCas.Controllers;

namespace Terradue.ElasticCas {
    public static class ElasticExtensions {
        public static T ThrowOnError<T>(this T response, TypeInformation typeStatus, string indexName, string actionDescription, ElasticCasFactory ecf) where T : IResponse
        {
            if (!response.IsValid)
            {
                typeStatus.Message = "Failed to " + actionDescription + ": " + response.ServerError != null ? response.ServerError.Error : "";
                // save the Type information as a document in the index
                ecf.Client.Index(typeStatus.Name, i => i.Index(indexName).Type("migration").Id(typeStatus.Name));
                throw new InvalidOperationException(response.ServerError.Error);
            }

            return response;
        }
    }
}

