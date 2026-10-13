using Reprojection.Web.Models.Vendor;
using Reprojection.Web.Services;

namespace Reprojection.Web.Models;

public sealed class PolicyPageViewModel
{
    public string? PolicyNumber { get; init; }
    public string? ValidationMessage { get; init; }
    public bool UsingMockData { get; init; }
    public VendorApiResult<PolicyInquiryResponse>? Result { get; init; }

    public PolicyInfo? Policy => Result?.Data?.Policy;
    public bool HasSearched => Result is not null;
}
