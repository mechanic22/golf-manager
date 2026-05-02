namespace GolfManager.Core.Configuration;

public class CustomDomainVerificationOptions
{
    /// <summary>
    /// Server IP addresses that are allowed for custom domain A/AAAA validation.
    /// </summary>
    public List<string> AllowedIps { get; set; } = new();

    /// <summary>
    /// Whether TXT record verification is required.
    /// </summary>
    public bool RequireTxtRecord { get; set; } = true;

    /// <summary>
    /// TXT record prefix used for verification.
    /// </summary>
    public string TxtRecordPrefix { get; set; } = "_golfmanager";
}
