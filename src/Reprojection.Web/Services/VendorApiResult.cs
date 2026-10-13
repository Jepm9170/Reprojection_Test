namespace Reprojection.Web.Services;

public enum DataSource { Live, Mock }

/// <summary>
/// Everything about one vendor call: the parsed model, the raw JSON exactly as received,
/// and diagnostics. Keeping the raw JSON is deliberate — while we reverse-engineer a
/// proprietary API, being able to see what actually came back is half the job.
/// </summary>
public sealed class VendorApiResult<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? RawJson { get; init; }
    public int? StatusCode { get; init; }
    public string? Error { get; init; }
    public DataSource Source { get; init; }
    public string RequestUrl { get; init; } = string.Empty;
    public long ElapsedMs { get; init; }
    public DateTimeOffset RetrievedAt { get; init; } = DateTimeOffset.Now;
    public MappingReport? Mapping { get; init; }
}
