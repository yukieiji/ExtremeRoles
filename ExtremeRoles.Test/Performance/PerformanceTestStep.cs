using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ExtremeRoles.Test.Performance;

public static class PerformanceTestStep
{
    public static void Register(IServiceCollection services, Assembly dll)
    {
        var performanceTestType = typeof(IPerformanceTest);

        foreach (var type in dll.GetTypes())
        {
            if (!performanceTestType.IsAssignableFrom(type) ||
                type.IsInterface || type.IsAbstract)
            {
                continue;
            }

            services.AddTransient(performanceTestType, type);
        }
    }
}
