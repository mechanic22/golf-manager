using System.Text.RegularExpressions;

namespace GolfManager.Shared.Extensions;

/// <summary>
/// String extension methods
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts a string to a URL-friendly slug
    /// </summary>
    /// <param name="str">The string to convert</param>
    /// <param name="maxLength">Maximum length of the slug (default: 50)</param>
    /// <returns>URL-friendly slug</returns>
    public static string ToSlug(this string str, int maxLength = 50)
    {
        if (string.IsNullOrWhiteSpace(str))
            return string.Empty;

        // Convert to lowercase
        str = str.ToLowerInvariant();

        // Remove invalid chars (keep only lowercase letters, numbers, spaces, and hyphens)
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

        // Convert multiple spaces into one space
        str = Regex.Replace(str, @"\s+", " ").Trim();

        // Replace spaces with hyphens
        str = Regex.Replace(str, @"\s", "-");

        // Remove consecutive hyphens
        str = Regex.Replace(str, @"-+", "-");

        // Trim hyphens from start and end
        str = str.Trim('-');

        // Limit to max length
        if (str.Length > maxLength)
            str = str.Substring(0, maxLength).TrimEnd('-');

        return str;
    }
}

