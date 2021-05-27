using Arkitektum.RuleValidator.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arkitektum.RuleValidator.Core.Models.RuleOutput
{
    public class GroupOptions
    {
        public string Name { get; set; }
        public string UILocation { get; set; }
        public List<RuleOptions> Rules { get; set; } = new();
    }

    public interface IGroupOptionsBuilder
    {
        IGroupOptionsBuilder AddRule<T>() where T : Rule;
        IGroupOptionsBuilder AddRule<T>(Action<IRuleOptionsBuilder> config) where T : Rule;
        IGroupOptionsBuilder SetUILocation(string location);
    }

    public class GroupOptionsBuilder : IGroupOptionsBuilder
    {
        private readonly GroupOptions _group;

        public GroupOptionsBuilder()
        {
            _group = new GroupOptions();
        }

        public IGroupOptionsBuilder AddRule<T>() where T : Rule
        {
            var ruleType = typeof(T);

            if (_group.Rules.Any(rule => rule.Type == ruleType))
                throw new ArgumentException($"Regelen '{nameof(T)}' er allerede lagt til");

            var ruleConfig = new RuleOptions { Type = ruleType };
            _group.Rules.Add(ruleConfig);

            return this;
        }

        public IGroupOptionsBuilder AddRule<T>(Action<IRuleOptionsBuilder> config) where T : Rule
        {
            var ruleType = typeof(T);

            if (_group.Rules.Any(rule => rule.Type == ruleType))
                throw new ArgumentException($"Regelen '{nameof(T)}' er allerede lagt til");

            var builder = new RuleOptionsBuilder();
            config(builder);

            var ruleConfig = builder.Build();
            ruleConfig.Type = ruleType;

            _group.Rules.Add(ruleConfig);
            return this;
        }

        public IGroupOptionsBuilder SetUILocation(string location)
        {
            _group.UILocation = location;
            return this;
        }

        internal GroupOptions Build() => _group;
    }
}
