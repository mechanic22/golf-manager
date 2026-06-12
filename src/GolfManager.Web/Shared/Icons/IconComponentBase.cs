using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Shared.Icons;

/// <summary>
/// Base class for all icon components providing common functionality
/// </summary>
public abstract class IconComponentBase : ComponentBase
{
    /// <summary>
    /// Size of the icon
    /// </summary>
    [Parameter]
    public IconSize Size { get; set; } = IconSize.SM;

    /// <summary>
    /// Fill color for the icon (defaults to currentColor to inherit from parent)
    /// </summary>
    [Parameter]
    public string FillColor { get; set; } = "currentColor";

    /// <summary>
    /// Additional CSS classes to apply to the icon
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Additional HTML attributes
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the complete CSS class string for the icon
    /// </summary>
    protected string Css
    {
        get
        {
            var classes = $"gm-icon size-{(int)Size}";
            if (!string.IsNullOrEmpty(CssClass))
                classes += $" {CssClass}";
            return classes;
        }
    }
}

