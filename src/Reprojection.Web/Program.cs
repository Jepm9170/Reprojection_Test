using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Reprojection.Web.Configuration;
using Reprojection.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ----- Vendor API configuration (validated at startup so a bad config fails fast)
builder.Services
    .AddOptions<VendorApiOptions>()
    .Bind(builder.Configuration.GetSection(VendorApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ----- Typed HttpClient: one place that owns BaseAddress, timeout and auth headers
builder.Services.AddHttpClient<IVendorPolicyApiClient, VendorPolicyApiClient>((sp, client) =>
{
    var opt = sp.GetRequiredService<IOptions<VendorApiOptions>>().Value;
    VendorPolicyApiClient.Configure(client, opt);
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Money and dates render in en-US regardless of the server's regional settings.
var culture = new CultureInfo("en-US");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = [culture],
    SupportedUICultures = [culture]
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Policy}/{action=Index}/{policyNumber?}");

app.Run();
