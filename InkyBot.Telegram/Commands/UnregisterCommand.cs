using InkyBot.Conversation;
using InkyBot.EInk;
using InkyBot.Telegram.Extensions;
using InkyBot.Users;

namespace InkyBot.Telegram.Commands;

[RegisterCommand("unregister", Description = "Unregisters you as the wearer of the display")]
public class UnregisterCommand(
    IUserRepository userRepository,
    ITagDatabaseService tagDatabase,
    IOpenEPaperService openEPaperService
) : IConversation
{
    public async Task ExecuteAsync(IConversationContext context, CancellationToken cancellationToken = default)
    {
        if (await userRepository.GetCurrentUser() is not { } user || user.DisplayAddress is null)
        {
            await context.ReplyAsync(
                "You are not registered as the wearer of a display",
                cancellationToken
            );
            return;
        }

        await userRepository.DeleteUser(user.Id);
        await openEPaperService.ResetDisplay(user.DisplayAddress, cancellationToken);

        await context.ReplyAsync(
            "Display has been unregistered",
            cancellationToken
        );
    }
}