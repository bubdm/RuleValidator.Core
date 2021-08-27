using Arkitektum.RuleValidator.Models;
using System.Collections.Generic;

namespace DiBK.RuleValidator.Extensions.Xml
{
    public class XmlRuleMessage : RuleMessage
    {
        public IEnumerable<string> XPath { get; set; }
    }
}
