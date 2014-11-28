using System;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch;
using Terradue.ElasticCas.Types;

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

        public override IOpenSearchResultCollection ReadNative(OpenSearchResponse response) {
            if (response.ContentType == "application/json")
                return TransformJsonResponseToGenericJsonCollection(response);

            throw new NotSupportedException("Generic Json extension does not transform OpenSearch response of contentType " + response.ContentType);
        }

        public override string DiscoveryContentType {
            get {
                return "application/json";
            }
        }

        public override OpenSearchUrl FindOpenSearchDescriptionUrlFromResponse(OpenSearchResponse response) {

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

        public static GenericJsonCollection TransformJsonResponseToGenericJsonCollection(OpenSearchResponse response) {

            return (GenericJsonCollection)GenericJsonCollection.DeserializeFromStream(response.GetResponseStream());

        }

        //---------------------------------------------------------------------------------------------------------------------

    }
}

