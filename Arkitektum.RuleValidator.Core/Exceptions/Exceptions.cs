using System;

namespace Arkitektum.RuleValidator.Core.Exceptions
{
    public class GeometryFromGMLException : Exception
    {
        public GeometryFromGMLException()
        {
        }

        public GeometryFromGMLException(string message) : base(message)
        {
        }

        public GeometryFromGMLException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
