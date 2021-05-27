using Arkitektum.RuleValidator.Core.Models.RuleOutput;
using Arkitektum.RuleValidator.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arkitektum.RuleValidator.Core.Services
{
    public class RuleOutputService : IRuleOutputService
    {
        private readonly static string _undefinedGroupName = "Ikke definert";
        private readonly static string _validationRulesName = "Valideringsregler";
        private readonly RuleOutputOptions _ruleOutputOptions;

        public RuleOutputService(
            IOptions<RuleOutputOptions> ruleOutputOptions)
        {
            _ruleOutputOptions = ruleOutputOptions.Value;
        }

        public List<RuleSet> GetRuleSets()
        {
            var allRules = LoadRules();

            if (!allRules.Any())
                return new List<RuleSet>();

            var outputConfig = _ruleOutputOptions.OutputConfig;

            if (outputConfig == null)
            {
                return new List<RuleSet>
                {
                    new RuleSet(_validationRulesName, allRules.Select(rule => new RuleInfo(rule.Id, rule.Name, rule.Description, rule.MessageType.ToString(), rule.Documentation)))
                };
            }

            allRules.RemoveAll(rule => !outputConfig.Ignores.Contains(rule.GetType()));

            var ruleGroupings = GetRuleGroupings(allRules);
            var ruleSetsArray = new RuleSet[ruleGroupings.Count];
            var unmappedRules = new List<RuleInfo>();

            foreach (var (groupName, rules) in ruleGroupings)
            {
                var groupIndex = outputConfig.Groups.FindIndex(groupOptions => groupOptions.Name == groupName);

                if (groupIndex != -1)
                {
                    var ruleInfos = new RuleInfo[rules.Count];

                    foreach (var rule in rules)
                    {
                        var ruleIndex = outputConfig.Groups[groupIndex].Rules
                            .FindIndex(ruleOptions => ruleOptions.Type == rule.GetType());

                        if (ruleIndex != -1)
                        {
                            ruleInfos[ruleIndex] = new RuleInfo(rule.Id, rule.Name, rule.Description, rule.MessageType.ToString(), rule.Documentation);
                        }
                    }

                    ruleSetsArray[groupIndex] = new RuleSet(groupName, ruleInfos.Where(ruleInfo => ruleInfo != null));
                }
            }

            var ruleSets = new List<RuleSet>(ruleSetsArray.Where(ruleSet => ruleSet != null));

            if (ruleGroupings.TryGetValue(_undefinedGroupName, out var undefinedRules))
            {
                var ruleInfos = undefinedRules.Select(rule => new RuleInfo(rule.Id, rule.Name, rule.Description, rule.MessageType.ToString(), rule.Documentation));
                ruleSets.Add(new RuleSet(_undefinedGroupName, ruleInfos));
            }

            return ruleSets;
        }

        public List<Rule> OrderRules(List<Rule> rules)
        {
            var outputConfig = _ruleOutputOptions.OutputConfig;

            if (outputConfig == null)
                return rules;

            var groupedRules = outputConfig.Groups.SelectMany(group => group.Rules).ToList();
            var maxLength = Math.Max(groupedRules.Count, rules.Count);
            var ordered = new Rule[maxLength];
            var notMapped = new List<Rule>();

            foreach (var rule in rules)
            {
                var index = groupedRules.FindIndex(ruleConfig => ruleConfig.Type == rule.GetType());

                if (index != -1)
                    ordered[index] = rule;
                else
                    notMapped.Add(rule);
            }

            var orderedRules = ordered.Where(rule => rule != null).ToList();
            orderedRules.AddRange(notMapped);

            return orderedRules;
        }

        public IEnumerable<Rule> GetRulesByUILocation(IEnumerable<Rule> rules, string uiLocation)
        {
            var outputConfig = _ruleOutputOptions.OutputConfig;

            if (outputConfig == null)
                return new List<Rule>();

            return rules.Where(rule => HasUILocation(rule.GetType(), uiLocation));
        }

        private Dictionary<string, List<Rule>> GetRuleGroupings(List<Rule> rules)
        {
            var ruleGroupings = new Dictionary<string, List<Rule>>();

            foreach (var rule in rules)
            {
                var groupOptions = FindGroupOptions(rule.GetType());
                var groupName = groupOptions?.Name ?? _undefinedGroupName;

                if (!ruleGroupings.ContainsKey(groupName))
                    ruleGroupings.Add(groupName, new List<Rule> { rule });
                else
                    ruleGroupings[groupName].Add(rule);
            }

            return ruleGroupings;
        }

        private GroupOptions FindGroupOptions(Type type)
        {
            foreach (var groupOptions in _ruleOutputOptions.OutputConfig.Groups)
            {
                foreach (var ruleOptions in groupOptions.Rules)
                {
                    if (ruleOptions.Type == type)
                        return groupOptions;
                }
            }

            return null;
        }

        private bool HasUILocation(Type type, string uiLocation)
        {
            foreach (var groupOptions in _ruleOutputOptions.OutputConfig.Groups)
            {
                foreach (var ruleOptions in groupOptions.Rules)
                {
                    if (ruleOptions.Type == type)
                        return ruleOptions.UILocation == uiLocation || groupOptions.UILocation == uiLocation;
                }
            }

            return false;
        }

        private List<Rule> LoadRules()
        {
            return _ruleOutputOptions.Assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(Rule)) && type.GetConstructor(Type.EmptyTypes) != null)
                .Select(type =>
                {
                    var rule = Activator.CreateInstance(type) as Rule;
                    rule.Create();

                    return rule;
                })
                .Where(rule => !rule.Disabled)
                .ToList();
        }
    }
}
