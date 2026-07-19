using IsleBridge.Integration.Models;

namespace IsleBridge.Integration;

/// <summary>No result line correlated to the command's id within the timeout (contract §4).</summary>
public sealed class BridgeTimeoutException(string verb, string id, TimeSpan timeout)
    : Exception($"No result for {verb} (id={id}) within {timeout.TotalSeconds:0.#}s")
{
    public string Verb { get; } = verb;
    public string Id { get; } = id;
}

/// <summary>A command completed but the bridge reported failure (<c>ok:false</c>).</summary>
public sealed class BridgeCommandException(Result result)
    : Exception($"{result.Verb} failed: {result.CodeRaw}{(result.Msg is null ? "" : $" — {result.Msg}")}")
{
    public Result Result { get; } = result;
    public ResultCode Code => Result.Code;
}
