using GolfManager.Data;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfManager.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly GolfManagerDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(GolfManagerDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Search for a user by email
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<UserSearchResponse>>> SearchByEmail([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(ApiResponse<UserSearchResponse>.ErrorResponse("Invalid request", "Email is required"));
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        if (user == null)
        {
            // Return a response indicating user doesn't exist
            var notFoundResponse = new UserSearchResponse
            {
                Email = email,
                Exists = false
            };

            return Ok(ApiResponse<UserSearchResponse>.SuccessResponse(notFoundResponse));
        }

        var response = new UserSearchResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Exists = true
        };

        _logger.LogInformation("User search for email {Email}: {Exists}", email, response.Exists);

        return Ok(ApiResponse<UserSearchResponse>.SuccessResponse(response));
    }
}

