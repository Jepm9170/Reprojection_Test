using System.Text.Json;

namespace Reprojection.Web.Services;

/// <summary>
/// Compares the raw vendor JSON with what our models actually captured.
///   Unmapped  = fields the vendor sent that no model property received (data silently dropped).
///   Missing   = model properties the vendor never sent (model may be wrong, or field is optional).
/// Array elements are collapsed to "[]" so a list of 40 coverages reports each field once.
/// </summary>
public static class JsonMappingInspector
{
    public static MappingReport Compare<T>(string rawJson, T model)
    {
        using var rawDoc = JsonDocument.Parse(rawJson);
        var mapped = JsonSerializer.SerializeToElement(model, VendorJson.Options);

        var rawLeaves = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var rawContainers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        Flatten(rawDoc.RootElement, "", rawLeaves, rawContainers);

        var modelLeaves = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var modelContainers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        Flatten(mapped, "", modelLeaves, modelContainers);

        var unmapped = rawLeaves.Keys
            .Where(p => !modelLeaves.ContainsKey(p) && !modelContainers.Contains(p))
            .Select(p => rawLeaves[p])
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();

        // For missing fields, report the highest ancestor the JSON lacks entirely
        // (e.g. "policy.values" rather than 16 separate "policy.values.*" lines).
        var missing = modelLeaves.Keys
            .Where(p => !rawLeaves.ContainsKey(p) && !rawContainers.Contains(p))
            .Select(p => TopMostAbsent(p, rawLeaves, rawContainers))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();

        return new MappingReport(unmapped, missing, rawLeaves.Count);
    }

    private static void Flatten(JsonElement el, string path,
        Dictionary<string, string> leaves, HashSet<string> containers)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Object:
                if (path.Length > 0) containers.Add(path);
                var any = false;
                foreach (var prop in el.EnumerateObject())
                {
                    any = true;
                    Flatten(prop.Value, path.Length == 0 ? prop.Name : $"{path}.{prop.Name}", leaves, containers);
                }
                if (!any && path.Length > 0) leaves.TryAdd(path, path);
                break;

            case JsonValueKind.Array:
                if (path.Length > 0) containers.Add(path);
                var items = 0;
                foreach (var item in el.EnumerateArray())
                {
                    items++;
                    Flatten(item, $"{path}[]", leaves, containers);
                }
                if (items == 0) leaves.TryAdd(path, path);
                break;

            default:
                leaves.TryAdd(path, path);
                break;
        }
    }

    private static string TopMostAbsent(string path, Dictionary<string, string> rawLeaves, HashSet<string> rawContainers)
    {
        var result = path;
        var current = path;
        while (true)
        {
            var cut = Math.Max(current.LastIndexOf('.'), current.LastIndexOf("[]", StringComparison.Ordinal));
            if (cut <= 0) return result;
            current = current[..cut];
            if (rawContainers.Contains(current) || rawLeaves.ContainsKey(current)) return result;
            result = current;
        }
    }
}

public sealed record MappingReport(
    IReadOnlyList<string> UnmappedFields,
    IReadOnlyList<string> MissingFields,
    int FieldsReceived)
{
    public bool IsClean => UnmappedFields.Count == 0 && MissingFields.Count == 0;
}
