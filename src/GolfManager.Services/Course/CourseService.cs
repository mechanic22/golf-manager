using GolfManager.Data;
using GolfManager.Core.Entities;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Course;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GolfManager.Services.Course;

public class CourseService : ICourseService
{
    private readonly GolfManagerDbContext _context;
    private readonly ILogger<CourseService> _logger;

    public CourseService(GolfManagerDbContext context, ILogger<CourseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<List<CourseResponse>>> GetCoursesAsync(
        string? search = null, int page = 1, int pageSize = 25)
    {
        try
        {
            var query = _context.Courses
                .Where(c => !c.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(lower) ||
                    (c.City != null && c.City.ToLower().Contains(lower)) ||
                    (c.State != null && c.State.ToLower().Contains(lower)));
            }

            var total = await query.CountAsync();
            var courses = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = courses.Select(c => MapToResponse(c)).ToList();
            return ApiResponse<List<CourseResponse>>.SuccessResponse(
                responses, $"Found {total} course(s)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving courses");
            return ApiResponse<List<CourseResponse>>.ErrorResponse("Failed to retrieve courses", ex.Message);
        }
    }

    public async Task<ApiResponse<CourseResponse>> GetCourseAsync(
        string courseId, bool includeTees = false, bool includeHoles = false)
    {
        try
        {
            var course = await BuildCourseQuery(includeTees, includeHoles)
                .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);

            if (course == null)
                return ApiResponse<CourseResponse>.ErrorResponse("Course not found");

            return ApiResponse<CourseResponse>.SuccessResponse(MapToResponse(course, includeTees, includeHoles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving course {CourseId}", courseId);
            return ApiResponse<CourseResponse>.ErrorResponse("Failed to retrieve course", ex.Message);
        }
    }

    public async Task<ApiResponse<CourseResponse>> GetCourseByKeyAsync(
        string key, bool includeTees = false, bool includeHoles = false)
    {
        try
        {
            var course = await BuildCourseQuery(includeTees, includeHoles)
                .FirstOrDefaultAsync(c => c.Key == key && !c.IsDeleted);

            if (course == null)
                return ApiResponse<CourseResponse>.ErrorResponse("Course not found");

            return ApiResponse<CourseResponse>.SuccessResponse(MapToResponse(course, includeTees, includeHoles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving course by key {Key}", key);
            return ApiResponse<CourseResponse>.ErrorResponse("Failed to retrieve course", ex.Message);
        }
    }

    public async Task<ApiResponse<CourseResponse>> CreateCourseAsync(
        CreateCourseRequest request, string currentUserId)
    {
        try
        {
            var key = string.IsNullOrWhiteSpace(request.Key)
                ? GenerateKey(request.Name)
                : request.Key.ToLowerInvariant();

            // Ensure key uniqueness
            if (await _context.Courses.AnyAsync(c => c.Key == key && !c.IsDeleted))
                return ApiResponse<CourseResponse>.ErrorResponse($"A course with key '{key}' already exists");

            var course = new Core.Entities.Course
            {
                Id = Guid.NewGuid().ToString(),
                Key = key,
                Name = request.Name,
                Description = request.Description,
                Address = request.Address,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                Country = request.Country,
                PhoneNumber = request.PhoneNumber,
                WebsiteUrl = request.WebsiteUrl,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                NumberOfHoles = request.NumberOfHoles,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Course created: {CourseId} ({Name})", course.Id, course.Name);
            return ApiResponse<CourseResponse>.SuccessResponse(MapToResponse(course), "Course created");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course");
            return ApiResponse<CourseResponse>.ErrorResponse("Failed to create course", ex.Message);
        }
    }

    public async Task<ApiResponse<CourseResponse>> UpdateCourseAsync(
        string courseId, UpdateCourseRequest request, string currentUserId)
    {
        try
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);

            if (course == null)
                return ApiResponse<CourseResponse>.ErrorResponse("Course not found");

            course.Name = request.Name;
            course.Description = request.Description;
            course.Address = request.Address;
            course.City = request.City;
            course.State = request.State;
            course.PostalCode = request.PostalCode;
            course.Country = request.Country;
            course.PhoneNumber = request.PhoneNumber;
            course.WebsiteUrl = request.WebsiteUrl;
            course.Latitude = request.Latitude;
            course.Longitude = request.Longitude;
            course.NumberOfHoles = request.NumberOfHoles;
            course.UpdatedAt = DateTime.UtcNow;
            course.UpdatedBy = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Course updated: {CourseId}", courseId);
            return ApiResponse<CourseResponse>.SuccessResponse(MapToResponse(course), "Course updated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course {CourseId}", courseId);
            return ApiResponse<CourseResponse>.ErrorResponse("Failed to update course", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteCourseAsync(string courseId, string currentUserId)
    {
        try
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);

            if (course == null)
                return ApiResponse<bool>.ErrorResponse("Course not found");

            course.IsDeleted = true;
            course.UpdatedAt = DateTime.UtcNow;
            course.UpdatedBy = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Course soft-deleted: {CourseId}", courseId);
            return ApiResponse<bool>.SuccessResponse(true, "Course deleted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting course {CourseId}", courseId);
            return ApiResponse<bool>.ErrorResponse("Failed to delete course", ex.Message);
        }
    }

    public async Task<ApiResponse<List<TeeResponse>>> GetTeesAsync(string courseId, bool includeHoles = false)
    {
        try
        {
            if (!await _context.Courses.AnyAsync(c => c.Id == courseId && !c.IsDeleted))
                return ApiResponse<List<TeeResponse>>.ErrorResponse("Course not found");

            var query = _context.Tees.Where(t => t.CourseId == courseId && !t.IsDeleted);

            if (includeHoles)
                query = query.Include(t => t.HoleTees.Where(ht => !ht.IsDeleted));

            var tees = await query.OrderBy(t => t.Name).ToListAsync();
            var responses = tees.Select(t => MapTeeToResponse(t, includeHoles)).ToList();
            return ApiResponse<List<TeeResponse>>.SuccessResponse(responses, $"Found {responses.Count} tee(s)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tees for course {CourseId}", courseId);
            return ApiResponse<List<TeeResponse>>.ErrorResponse("Failed to retrieve tees", ex.Message);
        }
    }

    public async Task<ApiResponse<TeeResponse>> GetTeeAsync(
        string courseId, string teeId, bool includeHoles = false)
    {
        try
        {
            var query = _context.Tees.Where(t => t.CourseId == courseId && t.Id == teeId && !t.IsDeleted);

            if (includeHoles)
                query = query.Include(t => t.HoleTees.Where(ht => !ht.IsDeleted));

            var tee = await query.FirstOrDefaultAsync();
            if (tee == null)
                return ApiResponse<TeeResponse>.ErrorResponse("Tee not found");

            return ApiResponse<TeeResponse>.SuccessResponse(MapTeeToResponse(tee, includeHoles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tee {TeeId}", teeId);
            return ApiResponse<TeeResponse>.ErrorResponse("Failed to retrieve tee", ex.Message);
        }
    }

    public async Task<ApiResponse<TeeResponse>> CreateTeeAsync(
        string courseId, CreateTeeRequest request, string currentUserId)
    {
        try
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);
            if (course == null)
                return ApiResponse<TeeResponse>.ErrorResponse("Course not found");

            if (await _context.Tees.AnyAsync(t =>
                    t.CourseId == courseId && t.Name == request.Name && !t.IsDeleted))
                return ApiResponse<TeeResponse>.ErrorResponse($"A tee named '{request.Name}' already exists on this course");

            var tee = new Tee
            {
                Id = Guid.NewGuid().ToString(),
                CourseId = courseId,
                Name = request.Name,
                HtmlColorCode = request.HtmlColorCode,
                RatingOut = request.RatingOut,
                SlopeOut = request.SlopeOut,
                RatingIn = request.RatingIn,
                SlopeIn = request.SlopeIn,
                YardsOut = request.YardsOut,
                YardsIn = request.YardsIn,
                ParOut = request.ParOut,
                ParIn = request.ParIn,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            };

            _context.Tees.Add(tee);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tee created: {TeeId} ({Name}) on course {CourseId}", tee.Id, tee.Name, courseId);
            return ApiResponse<TeeResponse>.SuccessResponse(MapTeeToResponse(tee), "Tee created");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tee on course {CourseId}", courseId);
            return ApiResponse<TeeResponse>.ErrorResponse("Failed to create tee", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteTeeAsync(string courseId, string teeId, string currentUserId)
    {
        try
        {
            var tee = await _context.Tees
                .FirstOrDefaultAsync(t => t.CourseId == courseId && t.Id == teeId && !t.IsDeleted);

            if (tee == null)
                return ApiResponse<bool>.ErrorResponse("Tee not found");

            tee.IsDeleted = true;
            tee.UpdatedAt = DateTime.UtcNow;
            tee.UpdatedBy = currentUserId;

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Tee deleted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tee {TeeId}", teeId);
            return ApiResponse<bool>.ErrorResponse("Failed to delete tee", ex.Message);
        }
    }

    // ── Private helpers ─────────────────────────────────────────────────────

    private IQueryable<Core.Entities.Course> BuildCourseQuery(bool includeTees, bool includeHoles)
    {
        var query = _context.Courses.AsQueryable();

        if (includeTees && includeHoles)
        {
            query = query.Include(c => c.Tees.Where(t => !t.IsDeleted))
                         .ThenInclude(t => t.HoleTees.Where(ht => !ht.IsDeleted));
        }
        else if (includeTees)
        {
            query = query.Include(c => c.Tees.Where(t => !t.IsDeleted));
        }

        return query;
    }

    private static CourseResponse MapToResponse(
        Core.Entities.Course course, bool includeTees = false, bool includeHoles = false)
    {
        var response = new CourseResponse
        {
            Id = course.Id,
            Key = course.Key,
            Name = course.Name,
            Description = course.Description,
            Address = course.Address,
            City = course.City,
            State = course.State,
            PostalCode = course.PostalCode,
            Country = course.Country,
            PhoneNumber = course.PhoneNumber,
            WebsiteUrl = course.WebsiteUrl,
            Latitude = course.Latitude,
            Longitude = course.Longitude,
            NumberOfHoles = course.NumberOfHoles,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt
        };

        if (includeTees && course.Tees != null)
        {
            response.Tees = course.Tees
                .Where(t => !t.IsDeleted)
                .OrderBy(t => t.Name)
                .Select(t => MapTeeToResponse(t, includeHoles))
                .ToList();
        }

        return response;
    }

    private static TeeResponse MapTeeToResponse(Tee tee, bool includeHoles = false)
    {
        var response = new TeeResponse
        {
            Id = tee.Id,
            CourseId = tee.CourseId,
            Name = tee.Name,
            HtmlColorCode = tee.HtmlColorCode,
            RatingOut = tee.RatingOut,
            SlopeOut = tee.SlopeOut,
            RatingIn = tee.RatingIn,
            SlopeIn = tee.SlopeIn,
            YardsOut = tee.YardsOut,
            YardsIn = tee.YardsIn,
            ParOut = tee.ParOut,
            ParIn = tee.ParIn
        };

        if (includeHoles && tee.HoleTees != null)
        {
            response.Holes = tee.HoleTees
                .Where(ht => !ht.IsDeleted)
                .OrderBy(ht => ht.HoleNumber)
                .Select(ht => new HoleTeeResponse
                {
                    Id = ht.Id,
                    TeeId = ht.TeeId,
                    HoleNumber = ht.HoleNumber,
                    Par = ht.Par,
                    Yardage = ht.Yardage,
                    Handicap = ht.Handicap
                })
                .ToList();
        }

        return response;
    }

    private static string GenerateKey(string name)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            name.ToLowerInvariant().Trim(), @"[^a-z0-9]+", "-")
            .Trim('-');
    }
}
