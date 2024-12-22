using System.Net.NetworkInformation;

namespace InkyBot.EInk;

public interface IOpenEPaperService
{
    IAsyncEnumerable<TagRecord> QueryTagDatabaseAsync(CancellationToken cancellationToken = default);

    Task UploadImage(
        PhysicalAddress address,
        ReadOnlyMemory<byte> image,
        CancellationToken cancellationToken = default);

    Task UploadAndConvertImage(
        PhysicalAddress address,
        Stream image,
        CancellationToken cancellationToken = default);

    Task ResetDisplay(PhysicalAddress address, CancellationToken cancellationToken = default);
}