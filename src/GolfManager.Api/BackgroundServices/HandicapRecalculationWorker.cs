using GolfManager.Core.Services;
using GolfManager.Services.Event;

namespace GolfManager.Api.BackgroundServices;

public sealed class HandicapRecalculationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHandicapRecalculationQueue _queue;
    private readonly ILogger<HandicapRecalculationWorker> _logger;

    public HandicapRecalculationWorker(
        IServiceProvider serviceProvider,
        IHandicapRecalculationQueue queue,
        ILogger<HandicapRecalculationWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Handicap recalculation worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await _queue.DequeueAsync(stoppingToken);
                using var scope = _serviceProvider.CreateScope();
                var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                var userId = string.IsNullOrWhiteSpace(workItem.RequestedBy) ? "system-worker" : workItem.RequestedBy;

                if (string.IsNullOrWhiteSpace(workItem.GolferId))
                {
                    var count = await eventService.RecalculateEventHandicapsAsync(workItem.SeasonId, workItem.EventId, workItem.LeagueId, userId);
                    _logger.LogInformation(
                        "Processed handicap event job for season {SeasonId}, event {EventId}: recalculated {Count} golfers",
                        workItem.SeasonId,
                        workItem.EventId,
                        count);
                }
                else
                {
                    var changed = await eventService.RecalculateEventGolferHandicapAsync(workItem.SeasonId, workItem.EventId, workItem.GolferId, workItem.LeagueId, userId);
                    _logger.LogInformation(
                        "Processed handicap golfer job for season {SeasonId}, event {EventId}, golfer {GolferId}: changed={Changed}",
                        workItem.SeasonId,
                        workItem.EventId,
                        workItem.GolferId,
                        changed);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing handicap recalculation work item");
            }
        }

        _logger.LogInformation("Handicap recalculation worker stopped");
    }
}
