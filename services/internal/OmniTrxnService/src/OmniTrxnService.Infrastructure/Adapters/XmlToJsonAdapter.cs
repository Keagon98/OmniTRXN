using Newtonsoft.Json;
using OmniTrxnService.Application.Common.Interfaces;
using System.Xml;
using System.Xml.Linq;

namespace OmniTrxnService.Infrastructure.Adapters
{
    public class XmlToJsonAdapter : IXmlToJsonAdapter
    {
        public string Convert(string xml)
        {
            var doc = XDocument.Parse(xml);

            // Remove all namespace prefixes and namespace declarations
            foreach (var element in doc.Descendants().ToList())
            {
                element.Name = element.Name.LocalName;
                // Remove namespace declaration attributes
                element.Attributes()
                    .Where(a => a.IsNamespaceDeclaration)
                    .Remove();
                // Also remove any attributes that have a namespace prefix (if any)
                foreach (var attr in element.Attributes().ToList())
                {
                    if (attr.Name.Namespace != XNamespace.None)
                    {
                        attr.Remove();
                    }
                }
            }
            doc.Root?.SetAttributeValue(XNamespace.Xmlns + "ns2", null); // not needed after removal

            return JsonConvert.SerializeXNode(doc, Newtonsoft.Json.Formatting.None, omitRootObject: false);
        }
    }
}
