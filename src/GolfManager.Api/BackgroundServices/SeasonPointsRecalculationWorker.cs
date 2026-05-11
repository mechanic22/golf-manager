using GolfManager.Core.Services;
using GolfManager.Services.Event;

namespace GolfManager.Api.BackgroundServices;

public sealed class SeasonPointsRecalculationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISeasonPointsRecalculationQueue _queue;
    private readonly ILogger<SeasonPointsRecalculationWorker> _logger;

    public SeasonPointsRecalculationWorker(
        IServiceProvider serviceProvider,
        ISeasonPointsRecalculationQueue queue,
        ILogger<SeasonPointsRecalculationWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Season points recalculation worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await _queue.DequeueAsync(stoppingToken);
                using var scope = _serviceProvider.CreateScope();
                var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                var userId = string.IsNullOrWhiteSpace(workItem.RequestedBy) ? "system-worker" : workItem.RequestedBy;

                var updated = await eventService.RecalculateSeasonTeamStandingsAsync(workItem.SeasonId, workItem.LeagueId, userId);
                _logger.LogInformation(
                    "Processed season points job for season {SeasonId}: updated {Count} teams",
                    workItem.SeasonId,
                    updated);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing season points recalculation work item");
            }
        }

        _logger.LogInformation("Season points recalculation worker stopped");
    }
}
