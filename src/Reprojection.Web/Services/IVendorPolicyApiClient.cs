using Reprojection.Web.Models.Vendor;

namespace Reprojection.Web.Services;

public interface IVendorPolicyApiClient
{
    /// <summary>API #1 (GET): pulls the in-force policy JSON and maps it to our models.</summary>
    Task<VendorApiResult<PolicyInquiryResponse>> GetPolicyAsync(string policyNumber, CancellationToken ct = default);

    /// <summary>
    /// API #2 (POST): submits reprojection input and returns the vendor's raw response.
    /// Configured and callable; response models get added once we have a real sample.
    /// </summary>
    Task<VendorApiResult<string>> RunReprojectionAsync<TRequest>(TRequest request, CancellationToken ct = default);
}
