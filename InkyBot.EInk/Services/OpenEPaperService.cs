using System.Net.Http.Headers;
using System.Net.Mime;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace InkyBot.EInk.Services;

internal sealed class OpenEPaperService(HttpClient client) : IOpenEPaperService
{
    public async IAsyncEnumerable<TagRecord> QueryTagDatabaseAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var next = 0;
        while (true)
        {
            var response = await client.GetAsync(next == 0 ? "/get_db" : $"/get_db?pos={next}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"OpenEPaper Access Point returned: {response}");
            }

            var json = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken
            );

            if (json.RootElement.TryGetProperty("error", out var error))
            {
                throw new InvalidOperationException($"Error reading tags database: {error}");
            }

            if (json.RootElement.TryGetProperty("tags", out var tags) && tags.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in tags.EnumerateArray())
                {
                    if (element.Deserialize<TagRecord>() is { } tag)
                    {
                        yield return tag;
                    }
                }
            }

            if (json.RootElement.TryGetProperty("continu", out var cont) && cont.ValueKind == JsonValueKind.Number)
            {
                next = cont.GetInt32();
            }
            else
            {
                break;
            }
        }
    }

    public Task UploadImage(
        PhysicalAddress address,
        ReadOnlyMemory<byte> image,
        CancellationToken cancellationToken = default)
    {
        var content = new MultipartFormDataContent();

        content.Add(new StringContent(address.ToString())
        {
            Headers =
            {
                ContentType = null
            }
        }, "\"mac\"");

        content.Add(new StringContent("1")
        {
            Headers =
            {
                ContentType = null
            }
        }, "\"dither\"");

        content.Add(new ReadOnlyMemoryContent(image)
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue(MediaTypeNames.Image.Jpeg)
                }
            }, "\"file\"", $"\"{address}.jpeg\"");

        return client.PostAsync("/imgupload", content, cancellationToken);
    }

    public async Task UploadAndConvertImage(
        PhysicalAddress address,
        Stream image,
        CancellationToken cancellationToken = default)
    {
        var loaded = await Image.LoadAsync(image, cancellationToken);

        if (loaded.Width != 296 || loaded.Height != 152)
        {
            loaded.Mutate(x => x.Resize(296, 152));
        }

        using var ms = new MemoryStream();
        await loaded.SaveAsync(ms, new JpegEncoder
        {
            SkipMetadata = true,
            Quality = 100,
            Interleaved = true,
            ColorType = JpegEncodingColor.YCbCrRatio444
        }, cancellationToken);

        await UploadImage(address, ms.GetBuffer().AsMemory(0, (int)ms.Position), cancellationToken);
    }

    public Task ResetDisplay(PhysicalAddress address, CancellationToken cancellationToken = default)
    {
        var content = new MultipartFormDataContent();

        content.Add(new StringContent(address.ToString())
        {
            Headers =
            {
                ContentType = null
            }
        }, "\"mac\"");

        content.Add(new StringContent("reboot")
        {
            Headers =
            {
                ContentType = null
            }
        }, "\"cmd\"");

        return client.PostAsync("/tag_cmd", content, cancellationToken);
    }
}