using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Course;

namespace GolfManager.Services.Course;

public interface ICourseService
{
    Task<ApiResponse<PagedResponse<CourseResponse>>> GetCoursesAsync(string? search = null, int page = 1, int pageSize = 25);
    Task<ApiResponse<CourseResponse>> GetCourseAsync(string courseId, bool includeTees = false, bool includeHoles = false);
    Task<ApiResponse<CourseResponse>> GetCourseByKeyAsync(string key, bool includeTees = false, bool includeHoles = false);
    Task<ApiResponse<CourseResponse>> CreateCourseAsync(CreateCourseRequest request, string currentUserId);
    Task<ApiResponse<CourseResponse>> UpdateCourseAsync(string courseId, UpdateCourseRequest request, string currentUserId);
    Task<ApiResponse<bool>> DeleteCourseAsync(string courseId, string currentUserId);
    Task<ApiResponse<List<TeeResponse>>> GetTeesAsync(string courseId, bool includeHoles = false);
    Task<ApiResponse<TeeResponse>> GetTeeAsync(string courseId, string teeId, bool includeHoles = false);
    Task<ApiResponse<TeeResponse>> CreateTeeAsync(string courseId, CreateTeeRequest request, string currentUserId);
    Task<ApiResponse<bool>> DeleteTeeAsync(string courseId, string teeId, string currentUserId);
}
