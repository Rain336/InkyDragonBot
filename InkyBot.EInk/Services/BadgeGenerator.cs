using System.Numerics;
using System.Text;
using InkyBot.EInk.Internal;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Telegram.Bot;
using Telegram.Bot.Types;
using ZXing;
using ZXing.Common;
using Color = SixLabors.ImageSharp.Color;
using Size = System.Drawing.Size;

namespace InkyBot.EInk.Services;

internal sealed class BadgeGenerator(
    IOpenEPaperService service,
    ITelegramBotClient botClient
) : IBadgeGenerator
{
    private const byte SMALL_HARDWARE_TYPE = 3;
    private const byte LARGE_HARDWARE_TYPE = 4;


    public async Task GenerateBadgesAsync(
        TagRecord tag,
        User user,
        Stream? image = null,
        CancellationToken cancellationToken = default)
    {
        // var key = $"badge:{tag.HardwareType}:{user.Id}";
        // if (await distributedCache.GetAsync(key, cancellationToken) is { Length: > 0 } cached)
        // {
        //     await service.UploadImage(
        //         tag.Address!,
        //         cached,
        //         cancellationToken
        //     );
        //     return;
        // }

        switch (tag.HardwareType)
        {
            case LARGE_HARDWARE_TYPE:
                await GenerateLargeBadgesAsync(tag, user, image, cancellationToken);
                break;
        }
    }

    private async Task GenerateLargeBadgesAsync(
        TagRecord tag,
        User user,
        Stream? image,
        CancellationToken cancellationToken)
    {
        if (Assets.LargeTemplateImage is null)
        {
            return;
        }

        var badge = Assets.LargeTemplateImage.Clone();
        DrawingUtils.GenerateDisplayName(user, badge, new Rectangle(4, 124, 226, 28));

        if (await DrawingUtils.GetTelegramProfilePicture(user, new Rectangle(4, 4, 116, 116), botClient,
                cancellationToken) is { } profilePicture)
        {
            badge.Mutate(x => x.DrawImage(profilePicture, new Point(4, 4), 1.0f));
        }

        if (image is not null)
        {
            await DrawingUtils.GenerateUserImageAsync(image, badge, new Rectangle(124, 0, 172, 88), cancellationToken);
        }

        DrawingUtils.GenerateBarcode(user.Username!, badge, new Rectangle(226, 92, 70, 60));

        DrawingUtils.GenerateId(user.Id, badge, new Rectangle(124, 92, 65, 28));

        using var ms = new MemoryStream();
        await badge.SaveAsync(ms, new JpegEncoder
        {
            SkipMetadata = true,
            Quality = 100,
            Interleaved = true,
            ColorType = JpegEncodingColor.YCbCrRatio444
        }, cancellationToken);

        // var image = ms.ToArray();
        // await distributedCache.SetAsync(
        //     $"badge:{tag.HardwareType}:{user.Id}",
        //     image,
        //     new DistributedCacheEntryOptions
        //     {
        //         AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
        //         SlidingExpiration = TimeSpan.FromSeconds(10)
        //     },
        //     cancellationToken
        // );

        await service.UploadImage(
            tag.Address!,
            ms.GetBuffer().AsMemory(0, (int)ms.Position),
            cancellationToken
        );
    }
}