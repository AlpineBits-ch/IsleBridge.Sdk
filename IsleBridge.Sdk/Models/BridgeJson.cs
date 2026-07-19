using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace IsleBridge.Sdk.Models;

/// <summary>
/// Shared serializer settings for the wire contract: camelCase for envelope / stream
/// fields (<c>id</c>, <c>ts</c>, <c>modeName</c>, …), case-insensitive reads, and null
/// omission. Engine field names that are PascalCase (the skin customizer) carry explicit
/// <c>[JsonPropertyName]</c> attributes so they survive this policy unchanged.
/// </summary>
public static class BridgeJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
        // Explicit resolver so the SDK also works in hosts that disable reflection-based
        // serialization by default (trimmed / AOT apps).
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };
}
