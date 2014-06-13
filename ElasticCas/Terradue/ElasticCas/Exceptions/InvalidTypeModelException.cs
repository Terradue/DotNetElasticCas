using System;

namespace Terradue.ElasticCas {
    public class InvalidTypeModelException : Exception {
        string typeName;

        public InvalidTypeModelException(string typeName, string message): base(message) {
            this.typeName = typeName;
           
        }

        public string TypeName {
            get {
                return typeName;
            }
        }
    }
}

