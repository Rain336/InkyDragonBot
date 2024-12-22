using InkyBot.Conversation;

namespace InkyBot.Services;

public interface IConversationContextAccessor
{
    public IConversationContext? Context { get; }
}