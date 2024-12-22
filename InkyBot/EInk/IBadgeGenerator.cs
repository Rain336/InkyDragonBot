using Telegram.Bot.Types;

namespace InkyBot.EInk;

public interface IBadgeGenerator
{
    Task GenerateBadgesAsync(
        TagRecord tag,
        User user,
        Stream? image = null,
        CancellationToken cancellationToken = default);
}