using System.Collections.Concurrent;
using System.Collections.Immutable;
using InkyBot.Conversation;
using InkyBot.Services;
using InkyBot.Telegram.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace InkyBot.Telegram.Services;

internal sealed class UpdateProcessorService(
    ITelegramBotClient botClient,
    IServiceProvider serviceProvider,
    ILogger<UpdateProcessorService> logger,
    IEnumerable<CommandRegistration> commandRegistrations
) : IUpdateProcessorService, IAsyncDisposable
{
    private readonly ConcurrentDictionary<long, ConversationContext> _conversations = new();

    private readonly ImmutableDictionary<string, Type> _conversationFactories = commandRegistrations
        .ToImmutableDictionary(x => x.Command, x => x.ConversationType);

    public ValueTask ProcessUpdate(Update update, CancellationToken cancellationToken)
    {
        switch (update)
        {
            case { Message: { } message }: return ProcessMessage(message, cancellationToken);
            case { CallbackQuery: { } callbackQuery }: return ProcessCallbackQuery(callbackQuery, cancellationToken);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask Finish(long chatId)
    {
        if (_conversations.TryRemove(chatId, out var context))
        {
            logger.LogTrace("Finishing Conversation {ChatId}", chatId);
            return context.DisposeAsync();
        }

        return ValueTask.CompletedTask;
    }

    private async ValueTask ProcessMessage(Message message, CancellationToken cancellationToken)
    {
        if (message.Chat.Type != ChatType.Private) return;

        if (_conversations.TryGetValue(message.Chat.Id, out var context))
        {
            if (message.Text == "/cancel")
            {
                context.Cancel();
            }
            else
            {
                await context.OnMessage(message, cancellationToken);
            }

            return;
        }

        if (string.IsNullOrEmpty(message.Text)) return;
        if (!message.Text.StartsWith('/')) return;

        var idx = message.Text.IndexOf(' ');
        if (idx == -1) idx = message.Text.Length;

        var command = message.Text.Substring(1, idx - 1).Trim();

        if (!_conversationFactories.TryGetValue(command, out var conversationType))
        {
            await botClient.SendMessage(
                message.Chat.Id,
                @"Unknown command.
Send /help for a list of commands",
                cancellationToken: cancellationToken
            );
            return;
        }

        logger.LogTrace("Starting Conversation {ChatId} with initial message {Message}", message.Chat.Id, message.Text);
        _conversations[message.Chat.Id] = new ConversationContext(serviceProvider, message, conversationType);
    }

    private async ValueTask ProcessCallbackQuery(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (_conversations.TryGetValue(callbackQuery.From.Id, out var context))
        {
            await context.OnCallbackQuery(callbackQuery, cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var context in _conversations.Values)
        {
            await context.DisposeAsync();
        }

        _conversations.Clear();
    }
}