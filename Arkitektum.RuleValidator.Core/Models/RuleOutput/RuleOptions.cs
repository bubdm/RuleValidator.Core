using System;

namespace Arkitektum.RuleValidator.Core.Models.RuleOutput
{
    public class RuleOptions
    {
        public Type Type { get; set; }
        public string UILocation { get; set; }
    }

    public interface IRuleOptionsBuilder
    {
        IRuleOptionsBuilder SetUILocation(string location);
    }

    public class RuleOptionsBuilder : IRuleOptionsBuilder
    {
        private readonly RuleOptions _rule;

        public RuleOptionsBuilder()
        {
            _rule = new RuleOptions();
        }

        public IRuleOptionsBuilder SetUILocation(string location)
        {
            _rule.UILocation = location;
            return this;
        }

        internal RuleOptions Build() => _rule;
    }
}
