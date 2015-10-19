using System;

namespace Terradue.ElasticCas {
    using System.Xml;
    using System.Xml.Linq;

    public static class XElementExtensions
    {
        public static XmlElement ToXmlElement(this XElement el)
        {
            var doc = new XmlDocument();
            doc.Load(el.CreateReader());
            return doc.DocumentElement;
        }
    }
}

