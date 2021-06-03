using System;
using System.Collections.Generic;
using System.Reflection;

namespace Arkitektum.RuleValidator.Core.Models.RuleOutput
{
    public class RuleOutputSettings
    {
        private readonly Dictionary<string, RuleOutputOptions> _settings = new();

        public void AddConfig(object key, Action<RuleOutputOptions> options)
        {
            var ruleOutputOptions = new RuleOutputOptions();
            options.Invoke(ruleOutputOptions);

            if (!_settings.ContainsKey(key.ToString()))
                _settings.Add(key.ToString(), ruleOutputOptions);
        }

        public RuleOutputOptions GetSettings(object key)
        {
            return _settings.TryGetValue(key.ToString(), out var options) ? options : null;
        }
    }

    public class RuleOutputOptions
    {
        public RuleOutputConfig OutputConfig { get; set; }
        public List<Assembly> Assemblies { get; } = new List<Assembly>();

        public void AddRuleAssembly(Assembly assembly)
        {
            if (assembly != null && !Assemblies.Contains(assembly))
                Assemblies.Add(assembly);
        }
    }
}
