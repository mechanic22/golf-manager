using GolfManager.Api.Authorization;
using GolfManager.Services.Course;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GolfManager.Api.Controllers;

[ApiController]
[Route("api/v1/courses")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(ICourseService courseService, ILogger<CoursesController> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }

    /// <summary>
    /// Search / list all courses
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResponse<CourseResponse>>>> GetCourses(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var response = await _courseService.GetCoursesAsync(search, page, pageSize);
        return Ok(response);
    }

    /// <summary>
    /// Get a course by ID
    /// </summary>
    [HttpGet("{courseId}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> GetCourse(
        string courseId,
        [FromQuery] bool includeTees = false,
        [FromQuery] bool includeHoles = false)
    {
        var response = await _courseService.GetCourseAsync(courseId, includeTees, includeHoles);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }

    /// <summary>
    /// Get a course by its URL-friendly key
    /// </summary>
    [HttpGet("by-key/{key}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> GetCourseByKey(
        string key,
        [FromQuery] bool includeTees = false,
        [FromQuery] bool includeHoles = false)
    {
        var response = await _courseService.GetCourseByKeyAsync(key, includeTees, includeHoles);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }

    /// <summary>
    /// Create a new course (global admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationConstants.Policies.GlobalAdmin)]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> CreateCourse(
        [FromBody] CreateCourseRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse<CourseResponse>.ErrorResponse("User not authenticated"));

        var response = await _courseService.CreateCourseAsync(request, userId);
        if (!response.Success)
            return BadRequest(response);

        return CreatedAtAction(nameof(GetCourse),
            new { courseId = response.Data!.Id },
            response);
    }

    /// <summary>
    /// Update an existing course (global admin only)
    /// </summary>
    [HttpPut("{courseId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.GlobalAdmin)]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> UpdateCourse(
        string courseId,
        [FromBody] UpdateCourseRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse<CourseResponse>.ErrorResponse("User not authenticated"));

        var response = await _courseService.UpdateCourseAsync(courseId, request, userId);
        if (!response.Success)
            return response.Message.Contains("not found") ? NotFound(response) : BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Soft-delete a course (global admin only)
    /// </summary>
    [HttpDelete("{courseId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.GlobalAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCourse(string courseId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));

        var response = await _courseService.DeleteCourseAsync(courseId, userId);
        if (!response.Success)
            return response.Message.Contains("not found") ? NotFound(response) : BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Get GPS data for all holes on a course (mobile distance calculator)
    /// </summary>
    [HttpGet("{courseId}/holes/gps")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<HoleGpsResponse>>>> GetHoleGps(string courseId)
    {
        var response = await _courseService.GetHoleGpsAsync(courseId);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }

    // ── Tee endpoints ────────────────────────────────────────────────────────

    /// <summary>
    /// Get all tees for a course
    /// </summary>
    [HttpGet("{courseId}/tees")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<TeeResponse>>>> GetTees(
        string courseId,
        [FromQuery] bool includeHoles = false)
    {
        var response = await _courseService.GetTeesAsync(courseId, includeHoles);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }

    /// <summary>
    /// Get a specific tee on a course
    /// </summary>
    [HttpGet("{courseId}/tees/{teeId}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<TeeResponse>>> GetTee(
        string courseId, string teeId,
        [FromQuery] bool includeHoles = false)
    {
        var response = await _courseService.GetTeeAsync(courseId, teeId, includeHoles);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }

    /// <summary>
    /// Add a tee to a course (global admin only)
    /// </summary>
    [HttpPost("{courseId}/tees")]
    [Authorize(Policy = AuthorizationConstants.Policies.GlobalAdmin)]
    public async Task<ActionResult<ApiResponse<TeeResponse>>> CreateTee(
        string courseId,
        [FromBody] CreateTeeRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse<TeeResponse>.ErrorResponse("User not authenticated"));

        var response = await _courseService.CreateTeeAsync(courseId, request, userId);
        if (!response.Success)
            return response.Message.Contains("not found") ? NotFound(response) : BadRequest(response);

        return CreatedAtAction(nameof(GetTee),
            new { courseId, teeId = response.Data!.Id },
            response);
    }

    /// <summary>
    /// Remove a tee from a course (global admin only)
    /// </summary>
    [HttpDelete("{courseId}/tees/{teeId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.GlobalAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTee(string courseId, string teeId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));

        var response = await _courseService.DeleteTeeAsync(courseId, teeId, userId);
        if (!response.Success)
            return response.Message.Contains("not found") ? NotFound(response) : BadRequest(response);

        return Ok(response);
    }
}
