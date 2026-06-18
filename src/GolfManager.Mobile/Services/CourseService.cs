using GolfManager.Shared.DTOs.Course;

namespace GolfManager.Mobile.Services;

public class CourseService : ICourseService
{
    private readonly IApiService _api;

    public CourseService(IApiService api) => _api = api;

    public Task<List<HoleGpsResponse>?> GetHoleGpsAsync(string courseId)
        => _api.GetAsync<List<HoleGpsResponse>>($"/api/v1/courses/{courseId}/holes/gps");
}
