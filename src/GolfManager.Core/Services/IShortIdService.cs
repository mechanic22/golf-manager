namespace GolfManager.Core.Services;

/// <summary>
/// Service for generating short, URL-friendly IDs
/// </summary>
public interface IShortIdService
{
    /// <summary>
    /// Generate a unique short identifier
    /// </summary>
    /// <param name="length">Length of the ID (default: 8)</param>
    /// <returns>Short ID string</returns>
    string GenerateId(int length = 8);
}
