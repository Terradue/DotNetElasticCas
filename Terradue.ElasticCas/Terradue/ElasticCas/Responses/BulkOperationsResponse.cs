using System;

namespace Terradue.ElasticCas {

    public class BulkOperationsResponse {

        public BulkOperationsResponse() {
        }

        public long Added {get; set;}
        public long Updated {get; set;}
        public long Deleted {get; set;}
        public long Errors {get; set;}

    }
}

