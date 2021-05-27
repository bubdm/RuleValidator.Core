using System.Collections.Generic;

namespace Arkitektum.RuleValidator.Core.Models.RuleOutput
{
    public class RuleSet
    {
        public string Group { get; set; }
        public IEnumerable<RuleInfo> Rules { get; set; }

        public RuleSet()
        {
        }

        public RuleSet(string group, IEnumerable<RuleInfo> rules)
        {
            Group = group;
            Rules = rules;
        }
    }
}
