using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reprojection.Web.Services;

/// <summary>One place for the serializer settings used against the vendor API.</summary>
public static class VendorJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        // Vendors frequently send numbers as strings ("500000.00"); accept both.
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly JsonSerializerOptions Pretty = new() { WriteIndented = true };

    /// <summary>Re-indents raw JSON for display. Returns the input unchanged if it is not valid JSON.</summary>
    public static string PrettyPrint(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement, Pretty);
        }
        catch (JsonException)
        {
            return json;
        }
    }
}
