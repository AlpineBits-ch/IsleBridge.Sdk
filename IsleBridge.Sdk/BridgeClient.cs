using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using IsleBridge.Sdk.Internal;
using IsleBridge.Sdk.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IsleBridge.Sdk;

internal sealed class BridgeClient(
    IHttpClientFactory factory,
    CommandCorrelator correlator,
    IOptions<IsleBridgeOptions> options,
    ILogger<BridgeClient> logger) : IBridgeClient
{
    private readonly IsleBridgeOptions _options = options.Value;

    // --- action verbs -------------------------------------------------------------------

    public Task<Result> SwapAsync(string steam, string species, double? growth = null, CancellationToken ct = default) =>
        SendAsync("swap", steam, new { species, growth }, _options.SlowCommandTimeout, ct);

    public Task<Result> SetStatsAsync(string steam, SetStatsArgs stats, CancellationToken ct = default) =>
        SendAsync("setstats", steam, stats, ct: ct);

    public Task<Result> PrimeAsync(string steam, CancellationToken ct = default) =>
        SendAsync("prime", steam, ct: ct);

    public Task<Result> UnprimeAsync(string steam, CancellationToken ct = default) =>
        SendAsync("unprime", steam, ct: ct);

    public Task<Result> TeleportAsync(string steam, double x, double y, double z, double? yaw = null, CancellationToken ct = default) =>
        SendAsync("teleport", steam, new { x, y, z, yaw }, ct: ct);

    public Task<Result> HealAsync(string steam, CancellationToken ct = default) =>
        SendAsync("heal", steam, ct: ct);

    public Task<Result> KillAsync(string steam, CancellationToken ct = default) =>
        SendAsync("kill", steam, ct: ct);

    public Task<Result> SetGrowthAsync(string steam, double value, CancellationToken ct = default) =>
        SendAsync("setgrowth", steam, new { value }, ct: ct);

    public Task<Result> SetVitalAsync(string steam, string name, double value, CancellationToken ct = default) =>
        SendAsync("setvital", steam, new { name, value }, ct: ct);

    public Task<Result> SetSkinAsync(string steam, SkinCustomizer customizer, CancellationToken ct = default) =>
        SendAsync("setskin", steam, new { customizer }, ct: ct);

    public Task<Result> SetMutationsAsync(string steam, SetMutationsArgs mutations, CancellationToken ct = default) =>
        SendAsync("setmutations", steam, mutations, _options.SlowCommandTimeout, ct);

    public Task<Result> NotifyAsync(string steam, string message, CancellationToken ct = default) =>
        SendAsync("notify", steam, new { message }, ct: ct);

    public Task<Result> DmAsync(string text, string? steam = null, string? sender = null,
        ChatMode mode = ChatMode.Admin, CancellationToken ct = default) =>
        SendAsync("dm", steam, new { text, sender, mode = (int)mode }, ct: ct);

    // --- read verbs ---------------------------------------------------------------------

    public Task<PositionData> GetPosAsync(string steam, CancellationToken ct = default) =>
        ReadAsync<PositionData>("getpos", steam, ct);

    public Task<StatsSnapshot> GetStatsAsync(string steam, CancellationToken ct = default) =>
        ReadAsync<StatsSnapshot>("getstats", steam, ct);

    public Task<SkinCustomizer> GetSkinAsync(string steam, CancellationToken ct = default) =>
        ReadAsync<SkinCustomizer>("getskin", steam, ct);

    public Task<MutationsData> GetMutationsAsync(string steam, CancellationToken ct = default) =>
        ReadAsync<MutationsData>("getmutations", steam, ct);

    public Task<PlayersData> GetPlayersAsync(CancellationToken ct = default) =>
        ReadAsync<PlayersData>("players", null, ct);

    public Task<PingData> PingAsync(CancellationToken ct = default) =>
        ReadAsync<PingData>("ping", null, ct);

    // --- core ---------------------------------------------------------------------------

    public async Task<Result> SendAsync(string verb, string? steam = null, object? args = null,
        TimeSpan? timeout = null, CancellationToken ct = default)
    {
        var id = Guid.CreateVersion7().ToString();
        var effectiveTimeout = timeout ?? _options.DefaultCommandTimeout;

        var awaiter = correlator.Register(id);

        var envelope = new JsonObject
        {
            ["id"] = id,
            ["ts"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ["verb"] = verb
        };
        if (steam is not null) envelope["steam"] = steam;
        if (args is not null) envelope["args"] = JsonSerializer.SerializeToNode(args, BridgeJson.Options);

        try
        {
            var client = factory.CreateClient(ClientNames.Command);
            using var content = new StringContent(envelope.ToJsonString(), Encoding.UTF8, "application/json");
            using var response = await client.PostAsync(ApiRoutes.Command, content, ct);
            response.EnsureSuccessStatusCode();

            return await awaiter.WaitAsync(effectiveTimeout, ct);
        }
        catch (TimeoutException)
        {
            correlator.Forget(id);
            logger.LogWarning("Timeout awaiting {Verb} (id={Id})", verb, id);
            throw new BridgeTimeoutException(verb, id, effectiveTimeout);
        }
        catch
        {
            correlator.Forget(id);
            throw;
        }
    }

    private async Task<T> ReadAsync<T>(string verb, string? steam, CancellationToken ct)
    {
        var result = await SendAsync(verb, steam, ct: ct);
        if (!result.Ok) throw new BridgeCommandException(result);

        var data = result.DataAs<T>();
        if (data is null) throw new BridgeCommandException(result);
        return data;
    }
}
