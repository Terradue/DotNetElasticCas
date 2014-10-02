using System;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Linq;

namespace Terradue.ElasticCas.OpenSearch {
    public class DefaultNamespaces {

        public static NameValueCollection TypeNamespaces {
            get {
                NameValueCollection nvc = new NameValueCollection();
                nvc.Set("http://www.w3.org/2005/Atom", "atom");
                nvc.Set("http://a9.com/-/spec/opensearch/1.1/", "os");

                return nvc;
            }
        }

        public static XmlNamespaceManager GetXmlNamespaceManager(XElement elem) {
            XmlNamespaceManager xnsm = new XmlNamespaceManager(elem.CreateReader().NameTable);
            foreach (var key in TypeNamespaces.AllKeys) {
                xnsm.AddNamespace(TypeNamespaces[key], key);
            }

            return xnsm;
        }
    }
}

