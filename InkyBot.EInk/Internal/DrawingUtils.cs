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

    public static void GenerateDisplayName(User user, Image<Rgba32> badge, Rectangle drawArea)
    {
        var family = Assets.FontCollection.Get("Envy Code R");
        var font = family.CreateFont(24);

        var text = $"{user.FirstName} {user.LastName}";
        var offset = Measure(text, font, drawArea.Width, drawArea.Height);
        badge.Mutate(x => x.DrawText(
            text,
            font,
            Color.Black,
            new PointF(drawArea.X, drawArea.Y + offset.Y)
        ));
    }

    public static async Task<Image?> GetTelegramProfilePicture(User user, Rectangle drawArea,
        ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        if (await botClient.GetUserProfilePhotos(
                user.Id,
                limit: 1,
                cancellationToken: cancellationToken
            ) is not { TotalCount: > 0 } photos)
        {
            return null;
        }

        var photo = photos.Photos[0]
            .MinBy(x => Math.Abs((x.Width - drawArea.Width) + (x.Height - drawArea.Height)));

        using var ms = new MemoryStream();
        await botClient.GetInfoAndDownloadFile(photo!.FileId, ms, cancellationToken);

        ms.Position = 0;
        var image = await Image.LoadAsync(ms, cancellationToken);

        if (image.Width != drawArea.Width || image.Height != drawArea.Height)
        {
            ResizeOptions opt = new ResizeOptions();
            opt.Size = new Size(drawArea.Width, drawArea.Height);
            opt.Mode = ResizeMode.Crop;
            image.Mutate(x => x.Resize(opt));
        }

        return image;
    }

    public static async Task GenerateUserImageAsync(Stream image, Image<Rgba32> badge,
        Rectangle drawArea,
        CancellationToken cancellationToken)
    {
        var encoded = await Image.LoadAsync(image, cancellationToken);

        if (encoded.Width != drawArea.Width || encoded.Height != drawArea.Height)
        {
            encoded.Mutate(x => x.Resize(drawArea.Width, drawArea.Height));
        }

        badge.Mutate(x => x.DrawImage(encoded, drawArea.Location, 1.0f));
    }

    public static void GenerateBarcode(string username, Image<Rgba32> badge, Rectangle drawArea)
    {
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
        barcode.Mutate(x => x.Resize(drawArea.Width, 0));
        badge.Mutate(x => x.DrawImage(barcode, new Point(drawArea.X + 5, drawArea.Y), 1.0f));
    }

    public static void GenerateId(long id, Image<Rgba32> badge, Rectangle drawArea)
    {
        var family = Assets.FontCollection.Get("Galactic Simple");
        var font = family.CreateFont(12);

        var content = new StringBuilder();
        while (id != 0)
        {
            content.Append(LETTERS[(byte)id % LETTERS.Length]);
            id >>= 8;
        }

        var strId = content.ToString();
        var offset = Measure(strId, font, drawArea.Width, drawArea.Height);
        badge.Mutate(x => x.DrawText(
            strId,
            font,
            Color.Red,
            drawArea.Location + offset
        ));
    }

    private static PointF Measure(string text, Font font, float width, float height)
    {
        var bounds = TextMeasurer.MeasureBounds(text, new TextOptions(font));
        var offset = new Vector2(width, height) / 2 - bounds.Size / 2;
        return new PointF(Math.Max(offset.X, 0), Math.Max(offset.Y, 0));
    }
}