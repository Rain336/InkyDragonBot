using InkyBot.EInk.Configuration;
using InkyBot.EInk.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace InkyBot.EInk.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInkyBotEInk(this IServiceCollection services)
    {
        services
            .AddHttpClient<IOpenEPaperService, OpenEPaperService>()
            .ConfigureHttpClient((sp, client) =>
            {
                client.BaseAddress = new Uri(
                    sp.GetRequiredService<IOptions<OpenEPaperConfiguration>>().Value.AccessPointAddress
                );
            });

        return services
            .AddSingleton<ITagDatabaseService, TagDatabaseService>()
            .AddHostedService<TagDatabaseUpdaterService>()
            .AddTransient<IBadgeGenerator, BadgeGenerator>();
    }
}