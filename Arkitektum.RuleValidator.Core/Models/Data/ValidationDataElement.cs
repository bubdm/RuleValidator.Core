using System;

namespace Arkitektum.RuleValidator.Core.Models
{
    public class ValidationDataElement<T>
    {
        public T Data { get; private set; }
        public string FileName { get; private set; }
        public Enum DataType { get; private set; }

        public ValidationDataElement(T data, string fileName, Enum dataType)
        {
            Data = data;
            FileName = fileName;
            DataType = dataType;
        }
    }
}
