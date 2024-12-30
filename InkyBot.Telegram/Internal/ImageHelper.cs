using System.Net.Mime;
using InkyBot.Conversation;
using InkyBot.Telegram.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace InkyBot.Telegram.Internal;

internal static class ImageHelper
{
    public static async Task<bool> TryExtractImage(
        Message message,
        MemoryStream ms,
        IConversationContext context,
        CancellationToken cancellationToken)
    {
        if (message.Photo is { Length: > 0 })
        {
            var photo = message.Photo
                .MinBy(x => Math.Abs((x.Width - 172) + (x.Height - 88)));

            await context.BotClient.GetInfoAndDownloadFile(photo!.FileId, ms, cancellationToken);
            return true;
        }

        if (message.Document is not null)
        {
            if (message.Document.MimeType is null || !SupportedContentTypes.Contains(message.Document.MimeType))
            {
                await context.ReplyAsync(
                    "The image you sent is not supported",
                    cancellationToken
                );
                return true;
            }

            await context.BotClient.GetInfoAndDownloadFile(message.Document.FileId, ms, cancellationToken);
            return true;
        }

        if (message.Sticker is not null)
        {
            if (message.Sticker.IsAnimated || message.Sticker.IsVideo)
            {
                await context.ReplyAsync(
                    "The sticker you sent is not supported",
                    cancellationToken
                );
                return true;
            }

            await context.BotClient.GetInfoAndDownloadFile(message.Sticker.FileId, ms, cancellationToken);
            return true;
        }

        await context.ReplyAsync(
            "Please send an image",
            cancellationToken
        );
        return false;
    }

    private static readonly HashSet<string> SupportedContentTypes =
    [
        MediaTypeNames.Image.Bmp,
        MediaTypeNames.Image.Jpeg,
        MediaTypeNames.Image.Png,
        MediaTypeNames.Image.Tiff,
        MediaTypeNames.Image.Webp
    ];
}