using System.Numerics;
using System.Text;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Telegram.Bot;
using Telegram.Bot.Types;
using ZXing;
using ZXing.Common;
using Color = SixLabors.ImageSharp.Color;

namespace InkyBot.EInk.Internal;

internal static class DrawingUtils
{
    private const string LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static void GenerateDisplayName(User user, Image<Rgba32> badge)
    {
        // 226px x 28 px @ 4px 124px
        var family = Assets.FontCollection.Get("Envy Code R");
        var font = family.CreateFont(24);

        var text = $"{user.FirstName} {user.LastName}";
        var offset = Measure(text, font, 226, 28);
        badge.Mutate(x => x.DrawText(
            text,
            font,
            Color.Black,
            new PointF(4, 124 + offset.Y)
        ));
    }

    public static async Task<Image?> GetTelegramProfilePicture(User user, ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        // 116px x 116px @ 4px 4px
        if (await botClient.GetUserProfilePhotos(
                user.Id,
                limit: 1,
                cancellationToken: cancellationToken
            ) is not { TotalCount: > 0 } photos)
        {
            return null;
        }

        var photo = photos.Photos[0]
            .MinBy(x => Math.Abs((x.Width - 116) + (x.Height - 116)));

        using var ms = new MemoryStream();
        await botClient.GetInfoAndDownloadFile(photo!.FileId, ms, cancellationToken);

        ms.Position = 0;
        var image = await Image.LoadAsync(ms, cancellationToken);

        if (image.Width != 116 || image.Height != 116)
        {
            image.Mutate(x => x.Resize(116, 116));
        }

        return image;
    }

    public static async Task GenerateUserImageAsync(Stream image, Image<Rgba32> badge,
        CancellationToken cancellationToken)
    {
        // 172px x 88px @ 124px 0px
        var encoded = await Image.LoadAsync(image, cancellationToken);

        if (encoded.Width != 172 || encoded.Height != 88)
        {
            encoded.Mutate(x => x.Resize(172, 88));
        }

        badge.Mutate(x => x.DrawImage(encoded, new Point(124, 0), 1.0f));
    }

    public static void GenerateBarcode(string username, Image<Rgba32> badge)
    {
        // 70px x 60px @ 226px x 92px
        var content = $"https://t.me/{username}";
        var writer = new ZXing.ImageSharp.BarcodeWriter<Rgba32>
        {
            Options = new EncodingOptions
            {
                Width = 0,
                Height = 0,
                PureBarcode = true,
                NoPadding = true
            },
            Format = BarcodeFormat.QR_CODE
        };

        var barcode = writer.Write(content);
        barcode.Mutate(x => x.Resize(60, 60));
        badge.Mutate(x => x.DrawImage(barcode, new Point(226 + 5, 92), 1.0f));
    }

    public static void GenerateId(long id, Image<Rgba32> badge)
    {
        // 65px x 28px @ 124px 92px
        var family = Assets.FontCollection.Get("Galactic Simple");
        var font = family.CreateFont(12);

        var content = new StringBuilder();
        while (id != 0)
        {
            content.Append(LETTERS[(byte)id % LETTERS.Length]);
            id >>= 8;
        }

        var strId = content.ToString();
        var offset = Measure(strId, font, 65, 28);
        badge.Mutate(x => x.DrawText(
            strId,
            font,
            Color.Red,
            new PointF(124, 92) + offset
        ));
    }

    private static PointF Measure(string text, Font font, float width, float height)
    {
        var bounds = TextMeasurer.MeasureBounds(text, new TextOptions(font));
        var offset = new Vector2(width, height) / 2 - bounds.Size / 2;
        return new PointF(Math.Max(offset.X, 0), Math.Max(offset.Y, 0));
    }
}