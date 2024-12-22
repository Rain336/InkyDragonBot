using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;

namespace InkyBot.EInk;

public interface ITagDatabaseService
{
    bool TryGetTag(PhysicalAddress address, [NotNullWhen(true)] out TagRecord? tag);
}