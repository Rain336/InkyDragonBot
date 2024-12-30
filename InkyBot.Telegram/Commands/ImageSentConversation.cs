using InkyBot.Conversation;
using InkyBot.EInk;
using InkyBot.Telegram.Extensions;
using InkyBot.Telegram.Internal;
using InkyBot.Users;
using Telegram.Bot.Types.ReplyMarkups;

namespace InkyBot.Telegram.Commands;

internal sealed class ImageSentConversation(
    IUserRepository userRepository,
    ITagDatabaseService tagDatabase,
    IOpenEPaperService service,
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

        using var ms = new MemoryStream();
        var message = context.InitialMessage;
        while (!await ImageHelper.TryExtractImage(message, ms, context, cancellationToken))
        {
            message = await context.WaitForMessageAsync(cancellationToken);
        }

        if (ms.Length == 0)
        {
            return;
        }

        var reply = await context.ReplyWithOptionsAsync(
            "Do you want to replace the full display or just the Badge section?",
            new InlineKeyboardMarkup(
                InlineKeyboardButton.WithCallbackData("Full Display", "display"),
                InlineKeyboardButton.WithCallbackData("Badge", "badge")
            ),
            cancellationToken
        );

        ms.Position = 0;
        if (reply.Data == "display")
        {
            await service.UploadAndConvertImage(
                user.DisplayAddress,
                ms,
                cancellationToken
            );
        }
        else
        {
            await badgeGenerator.GenerateBadgesAsync(
                tag,
                context.Sender,
                ms,
                cancellationToken
            );
        }

        await context.ReplyAsync(
            "Custom image was set",
            cancellationToken
        );
    }
}