using DiBK.RuleValidator.Models.Config;

namespace DiBK.RuleValidator.Extensions
{
    public static class ConfigBuilderExtensions
    {
        public static GroupConfigBuilder WithUILocation(this GroupConfigBuilder builder, string location)
        {
            builder.Group.Options["UILocation"] = location;

            return builder;
        }

        public static RuleConfigBuilder WithUILocation(this RuleConfigBuilder builder, string location)
        {
            builder.Rule.Options["UILocation"] = location;

            return builder;
        }
    }
}