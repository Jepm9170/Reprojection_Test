using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Reprojection.Web.Configuration;
using Reprojection.Web.Models;
using Reprojection.Web.Services;

namespace Reprojection.Web.Controllers;

public sealed partial class PolicyController(
    IVendorPolicyApiClient vendor,
    IOptions<VendorApiOptions> options) : Controller
{
    // Letters, digits, dash, underscore. Keeps junk out of the vendor URL and the mock file path.
    [GeneratedRegex("^[A-Za-z0-9_-]{1,30}$")]
    private static partial Regex PolicyNumberPattern();

    // GET /Policy  or  /Policy/Index/UL1000234  or  /Policy?policyNumber=UL1000234
    [HttpGet]
    public async Task<IActionResult> Index(string? policyNumber, CancellationToken ct)
    {
        var mock = options.Value.UseMockData;
        policyNumber = policyNumber?.Trim();

        if (string.IsNullOrEmpty(policyNumber))
            return View(new PolicyPageViewModel { UsingMockData = mock });

        if (!PolicyNumberPattern().IsMatch(policyNumber))
            return View(new PolicyPageViewModel
            {
                PolicyNumber = policyNumber,
                UsingMockData = mock,
                ValidationMessage = "Policy numbers can contain letters, digits, '-' and '_' (max 30)."
            });

        var result = await vendor.GetPolicyAsync(policyNumber, ct);

        return View(new PolicyPageViewModel
        {
            PolicyNumber = policyNumber,
            UsingMockData = mock,
            Result = result
        });
    }
}
