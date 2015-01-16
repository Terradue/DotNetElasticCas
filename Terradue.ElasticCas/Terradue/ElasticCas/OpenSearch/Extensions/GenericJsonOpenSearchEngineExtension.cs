using System;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch;
using Terradue.ElasticCas.Types;
using System.IO;
using Nest;

namespace Terradue.ElasticCas.OpenSearch.Extensions {
    public class GenericJsonOpenSearchEngineExtension : OpenSearchEngineExtension<GenericJsonCollection> {

        public GenericJsonOpenSearchEngineExtension() {
        }

        #region OpenSearchEngineExtension implementation

        public override string Identifier {
            get {
                return "json";
            }
        }

        public override string Name {
            get {
                return "Generic json";
            }
        }

        public override Type GetTransformType() {
            return typeof(GenericJsonOpenSearchable);
        }

        public override IOpenSearchResultCollection ReadNative(IOpenSearchResponse response) {
            if (response.ObjectType == typeof(byte[])) {
                if (response.ContentType == "application/json")
                    return TransformJsonResponseToGenericJsonCollection((OpenSearchResponse<byte[]>)response);
                throw new NotSupportedException("Generic Json extension does not transform OpenSearch response of contentType " + response.ContentType);
            }
            if (response.ObjectType == typeof(ISearchResponse<GenericJsonItem>)) {
                return GenericJsonCollection.TransformElasticSearchResponseToGenericJsonCollection((OpenSearchResponse<ISearchResponse<GenericJsonItem>>)response);
            }
            throw new InvalidOperationException("Generic Json extension does not transform OpenSearch response of type " + response.ObjectType);
        }

        public override string DiscoveryContentType {
            get {
                return "application/json";
            }
        }

        public override OpenSearchUrl FindOpenSearchDescriptionUrlFromResponse(IOpenSearchResponse response) {

            if (response.ContentType == "application/json") {
                // TODO
                throw new NotImplementedException();
            }

            return null;
        }

        public override IOpenSearchResultCollection CreateOpenSearchResultFromOpenSearchResult(IOpenSearchResultCollection results) {
            if (results is GenericJsonCollection)
                return results;

            return GenericJsonCollection.CreateFromOpenSearchResultCollection(results);
        }

        #endregion

        public static GenericJsonCollection TransformJsonResponseToGenericJsonCollection(OpenSearchResponse<byte[]> response) {

            return (GenericJsonCollection)GenericJsonCollection.DeserializeFromStream(new MemoryStream((byte[])response.GetResponseObject()));

        }

        //---------------------------------------------------------------------------------------------------------------------

    }
}

