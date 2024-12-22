namespace InkyBot.Conversation;

public interface IConversation
{
    Task ExecuteAsync(IConversationContext context, CancellationToken cancellationToken = default);
}