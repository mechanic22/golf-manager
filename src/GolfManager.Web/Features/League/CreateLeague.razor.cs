using GolfManager.Shared.DTOs.League;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace GolfManager.Web.Features.League;

public partial class CreateLeague : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<CreateLeague> Logger { get; set; } = null!;

    private bool canCreate = false;
    private bool isSubmitting = false;
    private string? errorMessage;
    private string? successMessage;
    private bool keyEditedManually;

    private CreateLeagueRequest request = new();

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        canCreate = true;

        await Task.CompletedTask;
    }

    private void HandleNameInput(ChangeEventArgs args)
    {
        request.Name = args.Value?.ToString() ?? string.Empty;

        if (!keyEditedManually)
        {
            request.Key = SlugifyLeagueKey(request.Name);
        }
    }

    private void HandleKeyInput(ChangeEventArgs args)
    {
        keyEditedManually = true;
        request.Key = SlugifyLeagueKey(args.Value?.ToString() ?? string.Empty);
    }

    private async Task HandleSubmit()
    {
        if (isSubmitting) return;

        try
        {
            isSubmitting = true;
            errorMessage = null;
            successMessage = null;

            request.Name = request.Name.Trim();
            request.Key = SlugifyLeagueKey(request.Key);
            request.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            request.LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim();
            request.CustomDomain = string.IsNullOrWhiteSpace(request.CustomDomain) ? null : request.CustomDomain.Trim().TrimEnd('.').ToLowerInvariant();

            if (!request.UseCustomDomain)
            {
                request.CustomDomain = null;
            }

            Logger.LogInformation("Creating league: {Name} ({Key})", request.Name, request.Key);

            var response = await LeagueService.CreateLeagueAsync(request);
            if (response?.Success == true && response.Data != null)
            {
                successMessage = "League created successfully. Redirecting...";
                Navigation.NavigateTo($"/league/{response.Data.Key}");
                return;
            }

            errorMessage = response?.Message ?? "Failed to create league.";
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to create league: {ex.Message}";
            Logger.LogError(ex, "Error creating league");
        }
        finally
        {
            isSubmitting = false;
        }
    }

    private static string SlugifyLeagueKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var slug = value.Trim().ToLowerInvariant();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", string.Empty);
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-").Trim('-');

        if (slug.Length > 50)
        {
            slug = slug[..50].TrimEnd('-');
        }

        return slug;
    }
}
