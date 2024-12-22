using InkyBot.Conversation;
using InkyBot.EInk;
using InkyBot.Telegram.Extensions;
using InkyBot.Users;

namespace InkyBot.Telegram.Commands;

[RegisterCommand("reset", Description = "Resets the display to the default Badge display")]
public class ResetCommand(
    IUserRepository userRepository,
    ITagDatabaseService tagDatabase,
    IBadgeGenerator badgeGenerator
) : IConversation
{
    public async Task ExecuteAsync(IConversationContext context, CancellationToken cancellationToken = default)
    {
        if (await userRepository.GetCurrentUser() is not { } user || user.DisplayAddress is null)
        {
            await context.ReplyAsync(
                "You are not registered as the wearer of a display, please call /start with a display's mac address",
                cancellationToken
            );
            return;
        }

        if (!tagDatabase.TryGetTag(user.DisplayAddress, out var tag) || tag.Address is null)
        {
            await context.ReplyAsync(
                "Your display is no longer active or out of range. Please contact @vrilly for support",
                cancellationToken
            );
            return;
        }

        await badgeGenerator.GenerateBadgesAsync(
            tag,
            context.Sender!,
            null,
            cancellationToken
        );
        await context.ReplyAsync(
            "Default badge display restored",
            cancellationToken
        );
    }
}