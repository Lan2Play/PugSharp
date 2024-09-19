using Microsoft.Extensions.DependencyInjection;

using Polly;
namespace PugSharp.ApiStats;

public static class ApiStatsServiceRegistration
{
    public static IServiceCollection AddApiStats(this IServiceCollection services, IAsyncPolicy<HttpResponseMessage> policy)
    {
        services.AddHttpClient<ApiStats>()
             .AddPolicyHandler(policy);
        services.AddSingleton<ApiStats>();


        services.AddHttpClient<DemoUploader>()
               .AddPolicyHandler(policy);
        services.AddSingleton<DemoUploader>();

        return services;
    }
}