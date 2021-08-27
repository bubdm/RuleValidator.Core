using Arkitektum.RuleValidator.Core.Models.RuleOutput;
using Arkitektum.RuleValidator.Models;
using System.Collections.Generic;

namespace Arkitektum.RuleValidator.Core.Services
{
    public interface IRuleOutputService
    {
        List<RuleSet> GetRuleSets(object key);
        List<Rule> OrderRules(object key, List<Rule> rules);
        IEnumerable<Rule> GetRulesByUILocation(object key, IEnumerable<Rule> rules, string uiLocation);
    }
}
