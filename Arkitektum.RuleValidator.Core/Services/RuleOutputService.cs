using Arkitektum.RuleValidator.Core.Models.RuleOutput;
using Arkitektum.RuleValidator.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arkitektum.RuleValidator.Core.Services
{
    public class RuleOutputService : IRuleOutputService
    {
        private readonly static string _validationRulesName = "Valideringsregler";
        private readonly static Dictionary<string, List<RuleSet>> _allRuleSets = new();
        private readonly RuleOutputSettings _settings;
        private readonly ILogger<RuleOutputService> _logger;

        public RuleOutputService(
            IOptions<RuleOutputSettings> options,
            ILogger<RuleOutputService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public List<RuleSet> GetRuleSets(object key)
        {
            if (_allRuleSets.ContainsKey(key.ToString()))
                return _allRuleSets[key.ToString()];

            var settings = _settings.GetSettings(key);

            if (settings == null)
                return new List<RuleSet>();

            var allRules = LoadRules(settings);

            if (!allRules.Any())
                return new List<RuleSet>();

            var outputConfig = settings.OutputConfig;

            if (outputConfig == null)
            {
                var ruleInfos = allRules.Select(rule => new RuleInfo(rule.Id, rule.Name, rule.Description, rule.MessageType.ToString(), rule.Documentation));
                var noConfigRuleSets = new List<RuleSet> { new RuleSet(_validationRulesName, ruleInfos) };

                _allRuleSets.Add(key.ToString(), noConfigRuleSets);
                return noConfigRuleSets;
            }

            var ruleSets = GetRuleSets(allRules, settings);
            _allRuleSets.Add(key.ToString(), ruleSets);

            return ruleSets;
        }

        public List<Rule> OrderRules(object key, List<Rule> rules)
        {
            var outputConfig = _settings.GetSettings(key)?.OutputConfig;

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

        public IEnumerable<Rule> GetRulesByUILocation(object key, IEnumerable<Rule> rules, string uiLocation)
        {
            var settings = _settings.GetSettings(key);

            if (settings.OutputConfig == null)
                return new List<Rule>();

            return rules.Where(rule => HasUILocation(rule.GetType(), uiLocation, settings));
        }

        private List<RuleSet> GetRuleSets(List<Rule> rules, RuleOutputOptions settings)
        {
            var ruleGroupings = GetRuleGroupings(rules, settings);
            var ruleSetsArray = new RuleSet[ruleGroupings.Count];
            var unmappedRules = new List<RuleInfo>();

            foreach (var (groupName, groupRules) in ruleGroupings)
            {
                var groupIndex = settings.OutputConfig.Groups.FindIndex(groupOptions => groupOptions.Name == groupName);

                if (groupIndex != -1)
                {
                    var rulesInGroup = settings.OutputConfig.Groups[groupIndex].Rules;
                    var ruleInfos = new RuleInfo[rulesInGroup.Count];

                    foreach (var rule in groupRules)
                    {
                        var ruleIndex = rulesInGroup
                            .FindIndex(ruleOptions => ruleOptions.Type == rule.GetType());

                        if (ruleIndex != -1)
                            ruleInfos[ruleIndex] = new RuleInfo(rule.Id, rule.Name, rule.Description, rule.MessageType.ToString(), rule.Documentation);
                    }

                    ruleSetsArray[groupIndex] = new RuleSet(groupName, ruleInfos.Where(ruleInfo => ruleInfo != null));
                }
            }

            return new List<RuleSet>(ruleSetsArray.Where(ruleSet => ruleSet != null));
        }

        private Dictionary<string, List<Rule>> GetRuleGroupings(List<Rule> rules, RuleOutputOptions options)
        {
            var ruleGroupings = new Dictionary<string, List<Rule>>();
            var notGrouped = new List<string>();

            foreach (var rule in rules)
            {
                var groupOptions = FindGroupOptions(rule.GetType(), options);

                if (groupOptions == null)
                {
                    notGrouped.Add($"{rule.Id}: {rule.GetType().Name}");
                    continue;
                }

                if (!ruleGroupings.ContainsKey(groupOptions.Name))
                    ruleGroupings.Add(groupOptions.Name, new List<Rule> { rule });
                else
                    ruleGroupings[groupOptions.Name].Add(rule);
            }

            if (notGrouped.Any())
                _logger.LogWarning($"RuleOutputConfiguration for '{options.Key}': Følgende regler er ikke tilknyttet en gruppe:{Environment.NewLine}{string.Join(Environment.NewLine, notGrouped)}");

            return ruleGroupings;
        }

        private static GroupOptions FindGroupOptions(Type type, RuleOutputOptions options)
        {
            foreach (var groupOptions in options.OutputConfig.Groups)
            {
                foreach (var ruleOptions in groupOptions.Rules)
                {
                    if (ruleOptions.Type == type)
                        return groupOptions;
                }
            }

            return null;
        }

        private static bool HasUILocation(Type type, string uiLocation, RuleOutputOptions options)
        {
            foreach (var groupOptions in options.OutputConfig.Groups)
            {
                foreach (var ruleOptions in groupOptions.Rules)
                {
                    if (ruleOptions.Type == type)
                        return ruleOptions.UILocation == uiLocation || groupOptions.UILocation == uiLocation;
                }
            }

            return false;
        }

        private static List<Rule> LoadRules(RuleOutputOptions options)
        {
            var ignoredRules = options.OutputConfig?.Ignores ?? new List<Type>();

            return options.Assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(Rule)) && type.GetConstructor(Type.EmptyTypes) != null && !ignoredRules.Contains(type))
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
