using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InkyBot.EInk.Services;

internal sealed class TagDatabaseUpdaterService(
    IOpenEPaperService service,
    ITagDatabaseService tagDatabaseService,
    ILogger<TagDatabaseUpdaterService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await LoadInitialTagDatabaseAsync(stoppingToken);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Could not load Tags Database");
        }
    }

    private async Task LoadInitialTagDatabaseAsync(CancellationToken cancellationToken)
    {
        await foreach (var tag in service.QueryTagDatabaseAsync(cancellationToken))
        {
            ((TagDatabaseService)tagDatabaseService).AddOrUpdate(tag);
        }

        logger.LogInformation("Tag database loaded.");
    }
}