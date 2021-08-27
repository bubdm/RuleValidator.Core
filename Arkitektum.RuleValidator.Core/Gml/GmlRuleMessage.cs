using DiBK.RuleValidator.Extensions.Xml;
using System.Collections.Generic;

namespace DiBK.RuleValidator.Extensions.Gml
{
    public class GmlRuleMessage : XmlRuleMessage
    {
        public IEnumerable<string> GmlIds { get; set; }
        public string ZoomTo { get; set; }
    }
}
