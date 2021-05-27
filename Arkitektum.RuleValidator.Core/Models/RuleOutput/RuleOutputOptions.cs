using System.Collections.Generic;
using System.Reflection;

namespace Arkitektum.RuleValidator.Core.Models.RuleOutput
{
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
