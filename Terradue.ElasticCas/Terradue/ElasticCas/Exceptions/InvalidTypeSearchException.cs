using System;

namespace Terradue.ElasticCas.Exceptions {
    public class InvalidTypeSearchException : Exception {
        string typeName;

        public InvalidTypeSearchException(string typeName, string message): base(message) {
            this.typeName = typeName;

        }

        public string TypeName {
            get {
                return typeName;
            }
        }
    }
}

