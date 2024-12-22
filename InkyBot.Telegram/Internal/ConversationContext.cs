using System.Threading.Channels;
using InkyBot.Conversation;
using InkyBot.Services;
using InkyBot.Telegram.Extensions;
using InkyBot.Telegram.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace InkyBot.Telegram.Internal;

internal sealed class ConversationContext : IConversationContext, IAsyncDisposable
{
    private readonly Channel<Message> _messageChannel = Channel.CreateUnbounded<Message>();
    private readonly Channel<CallbackQuery> _callbackQueryChannel = Channel.CreateUnbounded<CallbackQuery>();
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger<ConversationContext> _logger;
    private readonly AsyncServiceScope _serviceScope;

    public ITelegramBotClient BotClient { get; }
    public IConversation Conversation { get; }

    public Message InitialMessage { get; }

    public string Args { get; }

    public ConversationContext(IServiceProvider sp, Message message, Type conversationType)
    {
        _logger = sp.GetRequiredService<ILogger<ConversationContext>>();
        _serviceScope = sp.CreateAsyncScope();
        BotClient = sp.GetRequiredService<ITelegramBotClient>();

        ((ConversationContextAccessor)_serviceScope.ServiceProvider.GetRequiredService<IConversationContextAccessor>())
            .Context = this;

        Conversation = (IConversation)ActivatorUtilities.GetServiceOrCreateInstance(
            _serviceScope.ServiceProvider,
            conversationType
        );

        InitialMessage = message;

        var idx = message.Text!.IndexOf(' ') + 1;
        Args = idx == 0 ? "" : message.Text.Substring(idx);

        _ = RunConversationAsync(sp);
    }

    public ValueTask<Message> WaitForMessageAsync(CancellationToken cancellationToken = default)
    {
        return _messageChannel.Reader.ReadAsync(cancellationToken);
    }

    public ValueTask<CallbackQuery> WaitForCallbackQueryAsync(CancellationToken cancellationToken = default)
    {
        return _callbackQueryChannel.Reader.ReadAsync(cancellationToken);
    }

    public ValueTask OnMessage(Message message, CancellationToken cancellationToken = default)
    {
        return _messageChannel.Writer.WriteAsync(message, cancellationToken);
    }

    public ValueTask OnCallbackQuery(CallbackQuery message, CancellationToken cancellationToken = default)
    {
        return _callbackQueryChannel.Writer.WriteAsync(message, cancellationToken);
    }

    public void Cancel()
    {
        _cts.Cancel();
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Dispose();

        if (Conversation is IAsyncDisposable asyncDisposable) await asyncDisposable.DisposeAsync();
        else if (Conversation is IDisposable disposable) disposable.Dispose();

        await _serviceScope.DisposeAsync();
    }

    private async Task RunConversationAsync(IServiceProvider sp)
    {
        try
        {
            await Conversation.ExecuteAsync(this, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogTrace(
                "Conversation {ChatId} Canceled with initial message {Message}",
                InitialMessage.Chat.Id,
                InitialMessage.Text
            );
        }
        catch (Exception e)
        {
            _logger.LogWarning(
                e,
                "A Conversation {ChatId} threw an Exception with initial message {Message}",
                InitialMessage.Chat.Id,
                InitialMessage.Text
            );
        }
        finally
        {
            await sp.GetRequiredService<IUpdateProcessorService>()
                .Finish(InitialMessage.Chat.Id);
        }
    }
}