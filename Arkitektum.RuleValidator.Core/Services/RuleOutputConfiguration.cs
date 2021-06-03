using Arkitektum.RuleValidator.Core.Models.RuleOutput;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Arkitektum.RuleValidator.Core.Services
{
    public static class RuleOutputConfiguration
    {
        public static void ConfigureRuleOutput(this IServiceCollection services, Action<RuleOutputSettings> settings)
        {
            services.Configure(settings);
            services.AddTransient<IRuleOutputService, RuleOutputService>();
        }
    }
}
