using System.Collections.Concurrent;
using IsleBridge.Integration.Models;

namespace IsleBridge.Integration.Internal;

/// <summary>
/// Bridges the fire-and-forget command POST to the asynchronous results stream. A command
/// registers its <c>id</c> here before sending; the <see cref="ResultPump"/> completes it
/// when the matching result line arrives. This is the correlation the SDK hides from callers
/// (contract §4).
/// </summary>
internal sealed class CommandCorrelator
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<Result>> _pending = new();

    public Task<Result> Register(string id)
    {
        var tcs = new TaskCompletionSource<Result>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending[id] = tcs;
        return tcs.Task;
    }

    public void Complete(Result result)
    {
        // Ignore results for ids we don't know: redeliveries, or results predating this
        // process. Idempotent by design (contract §4 rule 2).
        if (result.Id is { Length: > 0 } && _pending.TryRemove(result.Id, out var tcs))
            tcs.TrySetResult(result);
    }

    public void Forget(string id) => _pending.TryRemove(id, out _);
}
