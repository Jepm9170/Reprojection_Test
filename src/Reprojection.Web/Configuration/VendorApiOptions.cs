using System.ComponentModel.DataAnnotations;

namespace Reprojection.Web.Configuration;

/// <summary>
/// Bound from the "VendorApi" section of appsettings.json.
/// Keep secrets (ApiKey) out of appsettings — use User Secrets locally and
/// environment variables / Key Vault on the server, e.g. VendorApi__Auth__ApiKey.
/// </summary>
public sealed class VendorApiOptions
{
    public const string SectionName = "VendorApi";

    [Required, Url]
    public string BaseUrl { get; set; } = string.Empty;

    [Range(1, 600)]
    public int TimeoutSeconds { get; set; } = 60;

    public VendorAuthOptions Auth { get; set; } = new();

    [Required]
    public VendorEndpointOptions Endpoints { get; set; } = new();

    /// <summary>When true, the client reads JSON from MockDataFolder instead of calling the vendor.</summary>
    public bool UseMockData { get; set; }

    public string MockDataFolder { get; set; } = "MockData";
}

public sealed class VendorAuthOptions
{
    /// <summary>"ApiKey" (custom header), "Bearer", or "None".</summary>
    public string Scheme { get; set; } = "ApiKey";
    public string HeaderName { get; set; } = "X-Api-Key";
    public string ApiKey { get; set; } = string.Empty;
}

public sealed class VendorEndpointOptions
{
    /// <summary>GET — returns the policy JSON. "{policyNumber}" is substituted at call time.</summary>
    [Required]
    public string GetPolicyData { get; set; } = string.Empty;

    /// <summary>POST — submits the reprojection input JSON. Wired up, not yet used by the UI.</summary>
    [Required]
    public string RunReprojection { get; set; } = string.Empty;
}
