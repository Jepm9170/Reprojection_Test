using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Reprojection.Web.Configuration;
using Reprojection.Web.Models.Vendor;

namespace Reprojection.Web.Services;

/// <summary>
/// Typed HttpClient for the vendor API. BaseAddress, timeout and auth headers are set in
/// Program.cs from the "VendorApi" config section, so this class only deals with requests.
/// </summary>
public sealed class VendorPolicyApiClient(
    HttpClient http,
    IOptions<VendorApiOptions> options,
    IHostEnvironment env,
    ILogger<VendorPolicyApiClient> logger) : IVendorPolicyApiClient
{
    private readonly VendorApiOptions _opt = options.Value;

    // ---------------------------------------------------------------- API #1 (GET)
    public async Task<VendorApiResult<PolicyInquiryResponse>> GetPolicyAsync(string policyNumber, CancellationToken ct = default)
    {
        var relative = _opt.Endpoints.GetPolicyData.Replace("{policyNumber}", Uri.EscapeDataString(policyNumber));
        var sw = Stopwatch.StartNew();

        string raw;
        int? status;
        string requestUrl;

        if (_opt.UseMockData)
        {
            var file = ResolveMockFile($"policy-{policyNumber}.json", "policy-sample.json");
            requestUrl = $"mock://{Path.GetFileName(file)}";
            if (file is null)
                return Fail<PolicyInquiryResponse>("No mock file found. Add MockData/policy-sample.json.", null, requestUrl, sw, DataSource.Mock);

            raw = await File.ReadAllTextAsync(file, ct);
            status = 200;
        }
        else
        {
            requestUrl = new Uri(http.BaseAddress!, relative).ToString();
            try
            {
                using var response = await http.GetAsync(relative, ct);
                status = (int)response.StatusCode;
                raw = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Vendor GET {Url} returned {Status}", requestUrl, status);
                    return Fail<PolicyInquiryResponse>($"Vendor returned HTTP {status} {response.ReasonPhrase}.", status, requestUrl, sw, DataSource.Live, raw);
                }
            }
            catch (TaskCanceledException) when (!ct.IsCancellationRequested)
            {
                return Fail<PolicyInquiryResponse>($"Timed out after {_opt.TimeoutSeconds}s.", null, requestUrl, sw, DataSource.Live);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Vendor GET {Url} failed", requestUrl);
                return Fail<PolicyInquiryResponse>($"Could not reach the vendor API: {ex.Message}", null, requestUrl, sw, DataSource.Live);
            }
        }

        return ParseAndMap<PolicyInquiryResponse>(raw, status, requestUrl, sw, _opt.UseMockData ? DataSource.Mock : DataSource.Live);
    }

    // --------------------------------------------------------------- API #2 (POST)
    public async Task<VendorApiResult<string>> RunReprojectionAsync<TRequest>(TRequest request, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var body = JsonSerializer.Serialize(request, VendorJson.Options);

        if (_opt.UseMockData)
        {
            var file = ResolveMockFile("reprojection-sample.json");
            var url = "mock://reprojection-sample.json";
            if (file is null)
                return Fail<string>("No mock reprojection response yet. Add MockData/reprojection-sample.json.", null, url, sw, DataSource.Mock);

            var mockRaw = await File.ReadAllTextAsync(file, ct);
            return new VendorApiResult<string> { Success = true, Data = mockRaw, RawJson = mockRaw, StatusCode = 200, Source = DataSource.Mock, RequestUrl = url, ElapsedMs = sw.ElapsedMilliseconds };
        }

        var requestUrl = new Uri(http.BaseAddress!, _opt.Endpoints.RunReprojection).ToString();
        try
        {
            using var content = new StringContent(body, Encoding.UTF8, "application/json");
            using var response = await http.PostAsync(_opt.Endpoints.RunReprojection, content, ct);
            var raw = await response.Content.ReadAsStringAsync(ct);
            var status = (int)response.StatusCode;

            if (!response.IsSuccessStatusCode)
                return Fail<string>($"Vendor returned HTTP {status} {response.ReasonPhrase}.", status, requestUrl, sw, DataSource.Live, raw);

            return new VendorApiResult<string> { Success = true, Data = raw, RawJson = raw, StatusCode = status, Source = DataSource.Live, RequestUrl = requestUrl, ElapsedMs = sw.ElapsedMilliseconds };
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return Fail<string>($"Timed out after {_opt.TimeoutSeconds}s.", null, requestUrl, sw, DataSource.Live);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Vendor POST {Url} failed", requestUrl);
            return Fail<string>($"Could not reach the vendor API: {ex.Message}", null, requestUrl, sw, DataSource.Live);
        }
    }

    // ------------------------------------------------------------------- helpers
    private VendorApiResult<T> ParseAndMap<T>(string raw, int? status, string url, Stopwatch sw, DataSource source)
    {
        try
        {
            var data = JsonSerializer.Deserialize<T>(raw, VendorJson.Options);
            if (data is null)
                return Fail<T>("The response body was empty or 'null'.", status, url, sw, source, raw);

            return new VendorApiResult<T>
            {
                Success = true,
                Data = data,
                RawJson = VendorJson.PrettyPrint(raw),
                StatusCode = status,
                Source = source,
                RequestUrl = url,
                ElapsedMs = sw.ElapsedMilliseconds,
                Mapping = JsonMappingInspector.Compare(raw, data)
            };
        }
        catch (JsonException ex)
        {
            // ex.Path + line number point straight at the field whose type doesn't match the model.
            logger.LogError(ex, "Failed to map vendor JSON at {Path}", ex.Path);
            var where = ex.Path is null ? "" : $" at '{ex.Path}' (line {ex.LineNumber + 1})";
            return Fail<T>($"JSON could not be mapped to {typeof(T).Name}{where}: {ex.Message}", status, url, sw, source, raw);
        }
    }

    private string? ResolveMockFile(params string[] candidates)
    {
        var folder = Path.Combine(env.ContentRootPath, _opt.MockDataFolder);
        return candidates
            .Select(name => Path.Combine(folder, name))
            .FirstOrDefault(File.Exists);
    }

    private static VendorApiResult<T> Fail<T>(string error, int? status, string url, Stopwatch sw, DataSource source, string? raw = null) => new()
    {
        Success = false,
        Error = error,
        StatusCode = status,
        RequestUrl = url,
        Source = source,
        ElapsedMs = sw.ElapsedMilliseconds,
        RawJson = raw is null ? null : VendorJson.PrettyPrint(raw)
    };

    /// <summary>Applies BaseAddress, timeout and auth from config. Called from Program.cs.</summary>
    public static void Configure(HttpClient client, VendorApiOptions opt)
    {
        client.BaseAddress = new Uri(opt.BaseUrl.EndsWith('/') ? opt.BaseUrl : opt.BaseUrl + "/");
        client.Timeout = TimeSpan.FromSeconds(opt.TimeoutSeconds);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (string.IsNullOrWhiteSpace(opt.Auth.ApiKey)) return;

        switch (opt.Auth.Scheme.ToLowerInvariant())
        {
            case "bearer":
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", opt.Auth.ApiKey);
                break;
            case "apikey":
                client.DefaultRequestHeaders.Add(opt.Auth.HeaderName, opt.Auth.ApiKey);
                break;
        }
    }
}
