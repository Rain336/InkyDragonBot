using InkyBot.Services;
using InkyBot.Telegram.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace InkyBot.Telegram.Services;

internal sealed class TelegramPollingService(
    ITelegramBotClient botClient,
    IOptionsMonitor<TelegramConfiguration> configuration,
    ILogger<TelegramPollingService> logger,
    IUpdateProcessorService updateProcessorService
) : BackgroundService, IUpdateHandler
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (configuration.CurrentValue.UpdateMethod != TelegramUpdateMethod.Polling) return Task.CompletedTask;

        return botClient.ReceiveAsync(this, null, stoppingToken);
    }

    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        await updateProcessorService.ProcessUpdate(update, cancellationToken);
    }

    public Task HandleErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        logger.LogWarning(exception, "Error processing telegram updates: {source}", source.ToString());
        return Task.CompletedTask;
    }
}