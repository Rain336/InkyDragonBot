using System.Reflection;
using InkyBot.Conversation;
using InkyBot.Services;
using InkyBot.Telegram.Configuration;
using InkyBot.Telegram.Internal;
using InkyBot.Telegram.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace InkyBot.Telegram.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInkyBotTelegram(this IServiceCollection services)
    {
        foreach (var (attribute, type) in typeof(ServiceCollectionExtensions).Assembly
                     .GetTypes()
                     .Where(x => x is { IsClass: true, IsAbstract: false } &&
                                 Attribute.IsDefined(x, typeof(RegisterCommandAttribute), false) &&
                                 x.IsAssignableTo(typeof(IConversation)))
                     .Select(x => (x.GetCustomAttribute<RegisterCommandAttribute>()!, x)))
        {
            services.AddSingleton(new CommandRegistration(attribute.Command, type, attribute.Description));
        }

        return services
            .AddSingleton<ITelegramBotClient>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<TelegramConfiguration>>();
                var lifetime = sp.GetRequiredService<IHostApplicationLifetime>();

                return new TelegramBotClient(config.Value.BotToken, null, lifetime.ApplicationStopping);
            })
            .AddSingleton<IUpdateProcessorService, UpdateProcessorService>()
            .AddScoped<IConversationContextAccessor, ConversationContextAccessor>()
            .AddHostedService<TelegramPollingService>();
    }
}