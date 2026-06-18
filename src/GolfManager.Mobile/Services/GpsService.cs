using GolfManager.Shared.DTOs.Course;

namespace GolfManager.Mobile.Services;

public class GpsService : IGpsService
{
    private const int PollingIntervalMs = 3000;
    private const double ChangeThresholdYards = 30;

    private CancellationTokenSource? _cts;
    private HoleGpsResponse? _lastDetectedHole;

    public event Action<Location>? LocationUpdated;
    public Location? LastLocation { get; private set; }

    public async Task StartAsync()
    {
        var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted) return;

        _cts = new CancellationTokenSource();
        _ = PollLoopAsync(_cts.Token);
    }

    public void Stop() => _cts?.Cancel();

    private async Task PollLoopAsync(CancellationToken ct)
    {
        var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(5));
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var loc = await Geolocation.GetLocationAsync(request, ct);
                if (loc != null)
                {
                    LastLocation = loc;
                    LocationUpdated?.Invoke(loc);
                }
            }
            catch (FeatureNotSupportedException) { Stop(); return; }
            catch (PermissionException) { Stop(); return; }
            catch (OperationCanceledException) { return; }
            catch { /* continue polling on transient errors */ }

            await Task.Delay(PollingIntervalMs, ct).ConfigureAwait(false);
        }
    }

    public HoleGpsResponse? DetectCurrentHole(IList<HoleGpsResponse> holes)
    {
        if (LastLocation == null || holes.Count == 0) return null;

        var nearest = holes
            .Where(h => h.TeeLatitude.HasValue && h.TeeLongitude.HasValue)
            .OrderBy(h => DistanceService.HaversineYards(
                LastLocation.Latitude, LastLocation.Longitude,
                h.TeeLatitude!.Value, h.TeeLongitude!.Value))
            .FirstOrDefault();

        if (nearest == null) return _lastDetectedHole;

        if (_lastDetectedHole == null)
        {
            _lastDetectedHole = nearest;
            return nearest;
        }

        var distToLast = DistanceService.HaversineYards(
            LastLocation.Latitude, LastLocation.Longitude,
            _lastDetectedHole.TeeLatitude!.Value, _lastDetectedHole.TeeLongitude!.Value);

        var distToNearest = DistanceService.HaversineYards(
            LastLocation.Latitude, LastLocation.Longitude,
            nearest.TeeLatitude!.Value, nearest.TeeLongitude!.Value);

        if (distToNearest < distToLast - ChangeThresholdYards)
            _lastDetectedHole = nearest;

        return _lastDetectedHole;
    }
}
