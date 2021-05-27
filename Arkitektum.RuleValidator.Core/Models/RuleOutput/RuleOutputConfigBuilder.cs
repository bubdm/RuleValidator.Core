using System;
using System.Collections.Generic;

namespace Arkitektum.RuleValidator.Core.Models.RuleOutput
{
    public class RuleOutputConfig
    {
        public List<GroupOptions> Groups { get; set; } = new();
    }

    public class RuleOutputConfigBuilder
    {
        protected RuleOutputConfig Config;

        private RuleOutputConfigBuilder()
        {
            Config = new();
        }

        public static RuleOutputConfigBuilder Create() => new();

        public RuleOutputConfigBuilder AddGroup(string name, Action<IGroupOptionsBuilder> config)
        {
            var builder = new GroupOptionsBuilder();
            config(builder);

            var group = builder.Build();
            group.Name = name;

            Config.Groups.Add(group);
            return this;
        }

        public RuleOutputConfig Build() => Config;
    }
}
