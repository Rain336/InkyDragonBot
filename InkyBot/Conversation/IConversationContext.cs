using Telegram.Bot;
using Telegram.Bot.Types;

namespace InkyBot.Conversation;

public interface IConversationContext
{
    ITelegramBotClient BotClient { get; }

    IConversation Conversation { get; }

    Message InitialMessage { get; }

    string Args { get; }

    Chat Chat => InitialMessage.Chat;

    User? Sender => InitialMessage.From;

    ValueTask<Message> WaitForMessageAsync(CancellationToken cancellationToken = default);
    
    ValueTask<CallbackQuery> WaitForCallbackQueryAsync(CancellationToken cancellationToken = default);
}