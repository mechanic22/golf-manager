using System.Security.Cryptography;
using GolfManager.Core.Services;

namespace GolfManager.Services.Common;

/// <summary>
/// Service for generating short, URL-friendly IDs (8 characters by default)
/// Uses the same algorithm as Holy Grail v1
/// </summary>
public class ShortIdService : IShortIdService
{
    // Allowed characters - excludes I and O to avoid confusion with 1 and 0
    private const string AllowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZ0123456789";
    
    /// <summary>
    /// Generate a cryptographically secure short ID
    /// </summary>
    /// <param name="length">Length of the ID (default: 8)</param>
    /// <returns>Short ID string (e.g., "A3K9P2XZ")</returns>
    public string GenerateId(int length = 8)
    {
        if (length <= 0)
            throw new ArgumentException("Length must be greater than 0", nameof(length));
            
        var chars = new char[length];
        var randomBytes = new byte[length];
        
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        
        for (int i = 0; i < length; i++)
        {
            chars[i] = AllowedChars[randomBytes[i] % AllowedChars.Length];
        }
        
        return new string(chars);
    }
}
