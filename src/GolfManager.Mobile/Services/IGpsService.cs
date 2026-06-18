using GolfManager.Shared.DTOs.Course;

namespace GolfManager.Mobile.Services;

public interface IGpsService
{
    event Action<Location>? LocationUpdated;
    Location? LastLocation { get; }
    Task StartAsync();
    void Stop();
    HoleGpsResponse? DetectCurrentHole(IList<HoleGpsResponse> holes);
}
