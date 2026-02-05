using EventHubHelperWebsite.Models;
using Microsoft.Extensions.Options;

namespace EventHubHelperWebsite.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddAppSettingsDependencyInjection(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.GetSection("Values").Exists() 
                ? configuration.GetSection("Values")
                : configuration;
            services.Configure<AppSettings>(config);

            services.AddSingleton(sp => 
            {
                var appSettings = sp.GetRequiredService<IOptions<AppSettings>>().Value;

                if (appSettings is null)
                    throw new ArgumentNullException(nameof(appSettings), "AppSettings configuration is missing or invalid.");
                return appSettings;
            });
            return services;
        }
    }
}
