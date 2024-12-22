using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace InkyBot.EInk;

internal static class Assets
{
    public static Image<Rgba32>? SmallTemplateImage { get; }
    public static Image<Rgba32>? LargeTemplateImage { get; }
    public static FontCollection FontCollection { get; } = new();

    static Assets()
    {
        var assembly = typeof(Assets).Assembly;

        if (assembly.GetManifestResourceStream("InkyBot.EInk.Assets.small.png") is { } small)
        {
            SmallTemplateImage = Image.Load<Rgba32>(small);
        }

        if (assembly.GetManifestResourceStream("InkyBot.EInk.Assets.large.png") is { } large)
        {
            LargeTemplateImage = Image.Load<Rgba32>(large);
        }

        foreach (var name in assembly.GetManifestResourceNames())
        {
            if (name.EndsWith(".ttf"))
            {
                FontCollection.Add(assembly.GetManifestResourceStream(name)!);
            }
        }
    }
}