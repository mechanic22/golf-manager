using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolfManager.Api.Controllers;

[Authorize]
[ApiController]
public abstract class BaseLeagueController : ControllerBase
{
    protected string? LeagueId => HttpContext.Items["LeagueId"] as string;
}
