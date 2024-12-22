using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;

namespace InkyBot.EInk.Services;

internal sealed class TagDatabaseService : ITagDatabaseService
{
    private readonly ConcurrentDictionary<PhysicalAddress, TagRecord> _tags = new();

    public bool TryGetTag(PhysicalAddress address, [NotNullWhen(true)] out TagRecord? tag)
    {
        return _tags.TryGetValue(address, out tag);
    }

    internal void AddOrUpdate(TagRecord record)
    {
        if (record.Address is null) return;

        _tags[record.Address] = record;
    }
}