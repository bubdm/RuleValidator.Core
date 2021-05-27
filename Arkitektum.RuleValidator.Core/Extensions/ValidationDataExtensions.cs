using Arkitektum.RuleValidator.Core.Models;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Arkitektum.RuleValidator.Core.Extensions
{
    public static class ValidationDataExtensions
    {
        public static XElement GetElement(this ValidationDataElement<XDocument> dataElement, string xPath)
        {
            return XPath2Extensions.GetElement(dataElement.Data, xPath);
        }

        public static IEnumerable<XElement> GetElements(this ValidationDataElement<XDocument> dataElement, string xPath)
        {
            return XPath2Extensions.GetElements(dataElement?.Data, xPath);
        }

        public static T GetValue<T>(this ValidationDataElement<XDocument> dataElement, string xPath) where T : IConvertible
        {
            return XPath2Extensions.GetValue<T>(dataElement.Data, xPath);
        }

        public static string GetValue(this ValidationDataElement<XDocument> dataElement, string xPath)
        {
            return XPath2Extensions.GetValue(dataElement.Data, xPath);
        }

        public static IEnumerable<T> GetValues<T>(this ValidationDataElement<XDocument> dataElement, string xPath) where T : IConvertible
        {
            return XPath2Extensions.GetValues<T>(dataElement.Data, xPath);
        }

        public static IEnumerable<string> GetValues(this ValidationDataElement<XDocument> dataElement, string xPath)
        {
            return XPath2Extensions.GetValues(GetElements(dataElement, xPath), xPath);
        }
    }
}
