using GolfManager.Shared.DTOs.Course;

namespace GolfManager.Mobile.Services;

public interface ICourseService
{
    Task<List<HoleGpsResponse>?> GetHoleGpsAsync(string courseId);
}
