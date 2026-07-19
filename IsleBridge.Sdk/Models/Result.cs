using System.Text.Json;
using System.Text.Json.Serialization;

namespace IsleBridge.Sdk.Models;

/// <summary>
/// A result line from <c>results.ndjson</c> (contract §3): the ack for one command,
/// correlated back to the request by <see cref="Id"/>.
/// </summary>
public sealed class Result
{
    public string Id { get; init; } = "";
    public long Ts { get; init; }
    public string Verb { get; init; } = "";
    public string? Steam { get; init; }
    public bool Ok { get; init; }

    /// <summary>Raw machine-readable code string; branch on <see cref="Code"/> instead of <see cref="Msg"/>.</summary>
    [JsonPropertyName("code")]
    public string CodeRaw { get; init; } = "";

    /// <summary>Free-form human string for logs.</summary>
    public string? Msg { get; init; }

    /// <summary>Verb-specific payload for read verbs; <c>null</c> otherwise.</summary>
    public JsonElement? Data { get; init; }

    /// <summary>Parsed <see cref="CodeRaw"/>; <see cref="ResultCode.Unknown"/> for codes this SDK version doesn't know.</summary>
    [JsonIgnore]
    public ResultCode Code => ResultCodes.Parse(CodeRaw);

    /// <summary>Deserializes <see cref="Data"/> into <typeparamref name="T"/>, or default if absent.</summary>
    public T? DataAs<T>() =>
        Data is { ValueKind: not JsonValueKind.Null and not JsonValueKind.Undefined } d
            ? d.Deserialize<T>(BridgeJson.Options)
            : default;
}

/// <summary>The result <c>code</c> enum from contract §3.</summary>
public enum ResultCode
{
    Unknown,
    Ok,
    BadJson,
    UnknownVerb,
    BadArgs,
    TargetOffline,
    NoPawn,
    NotWhitelisted,
    RateLimited,
    Duplicate,
    InProgress,
    ApplyFailed,
    Crash
}

public static class ResultCodes
{
    public static ResultCode Parse(string? code) => code switch
    {
        "OK" => ResultCode.Ok,
        "BAD_JSON" => ResultCode.BadJson,
        "UNKNOWN_VERB" => ResultCode.UnknownVerb,
        "BAD_ARGS" => ResultCode.BadArgs,
        "TARGET_OFFLINE" => ResultCode.TargetOffline,
        "NO_PAWN" => ResultCode.NoPawn,
        "NOT_WHITELISTED" => ResultCode.NotWhitelisted,
        "RATE_LIMITED" => ResultCode.RateLimited,
        "DUPLICATE" => ResultCode.Duplicate,
        "IN_PROGRESS" => ResultCode.InProgress,
        "APPLY_FAILED" => ResultCode.ApplyFailed,
        "CRASH" => ResultCode.Crash,
        _ => ResultCode.Unknown
    };
}
