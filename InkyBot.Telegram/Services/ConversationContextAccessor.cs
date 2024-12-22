using InkyBot.Conversation;
using InkyBot.Services;

namespace InkyBot.Telegram.Services;

internal sealed class ConversationContextAccessor : IConversationContextAccessor
{
    public IConversationContext? Context { get; set; }
}