using Arkitektum.RuleValidator.Core.Models.RuleOutput;
using Arkitektum.RuleValidator.Models;
using System.Collections.Generic;
using System.Reflection;

namespace Arkitektum.RuleValidator.Core.Services
{
    public interface IRuleOutputService
    {
        List<RuleSet> GetRuleSets();
        List<Rule> OrderRules(List<Rule> rules);
        IEnumerable<Rule> GetRulesByUILocation(IEnumerable<Rule> rules, string uiLocation);
    }
}
