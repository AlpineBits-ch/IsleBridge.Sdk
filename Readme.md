# IsleBridge

A C# bridge that sits between **The Isle: Evrima** (via the CnCBridge Lua plugin) and your own microservice.

```
isle server → CnCBridge (Lua plugin) → IsleBridge.Api (C# proxy) → your microservice
```

---

## Projects

| Project            | What it is |
|--------------------|---|
| **IsleBridge.Sdk** | C# SDK for the consuming microservice. Typed verb methods, live `IAsyncEnumerable` streams, automatic result correlation. No HTTP surface. |

---

## IsleBridge.Api

A fire-and-forget proxy — it validates and relays, it does not interpret. All typing and
command/result correlation lives in the SDK.

### Configuration (`config.json`)

Only `PluginBasePath` is required; the rest have defaults.

```json
{
  "PluginBasePath": "/mnt/cncbridge",
  "PollIntervalMs": 500,
  "StatsInterval": 5,
  "StatsHeartbeatMs": 5000
}
```

| Key | Default | Meaning |
|---|---|---|
| `PluginBasePath` | `.` | On-box path to `.../Mods/CnCBridge`; all streams live under `Saved/`. |
| `PollIntervalMs` | `500` | How often the tail pumps poll the out-streams. |
| `StatsInterval` | `5` | Interval (s) requested when the periodic stats stream is turned on. |
| `StatsHeartbeatMs` | `5000` | How often the `stats.on` flood-guard flag is refreshed. |

### Endpoints

| Method | Route | Purpose |
|---|---|---|
| `POST` | `/api/v1/command` | Append a command envelope (`{id?, ts?, verb, steam?, args?}`). Injects `id`/`ts` if omitted. Returns `202 { id }`. |
| `GET` | `/api/v1/stream/chat` | SSE live chat feed. |
| `GET` | `/api/v1/stream/events` | SSE join / leave / death feed. |
| `GET` | `/api/v1/stream/stats` | SSE periodic per-player stats (subscribing here drives the flood guard). |
| `GET` | `/api/v1/stream/results` | SSE per-command result acks. |

Each SSE line is one `data: <raw-ndjson>` event. Command envelopes are appended **verbatim**,
so engine field names (e.g. the PascalCase skin customizer keys) are preserved exactly.

### Flood guard

The periodic stats stream is off by default. While at least one client is subscribed to
`/api/v1/stream/stats`, the Api sends `stats{mode:on}` and keeps `Saved/stats.on` fresh; when
the last reader disconnects it sends `stats{mode:off}` and deletes the flag. Stats can never
flood the disk with nobody listening.

### Running with Docker

`compose.yaml` mounts the plugin folder and config into the container:

```yaml
services:
  islebridge.api:
    image: islebridge.api
    build:
      context: .
      dockerfile: IsleBridge.Api/Dockerfile
    user: "1001:1001"
    ports:
      - "8080:8080"
    volumes:
      - /home/isle/isle-server/TheIsle/Binaries/Win64/Mods/CnCBridge:/mnt/cncbridge:rw
      - ./config.json:/app/config.json:ro
    restart: unless-stopped
```

```bash
docker compose up --build
```

---

## IsleBridge.Sdk (SDK)

Reference the project (or its NuGet package once published). It targets `net10.0` and uses a
`FrameworkReference` to `Microsoft.AspNetCore.App` (it ships hosted services and uses
`IHttpClientFactory` / DI / options / logging), so it's intended for server-side hosts.

### Registration

```csharp
builder.Services.AddIsleBridge(o =>
{
    o.BaseAddress = new Uri("http://islebridge:8080");
    // optional tuning:
    o.DefaultCommandTimeout = TimeSpan.FromSeconds(5);
    o.SlowCommandTimeout    = TimeSpan.FromSeconds(8);   // swap, setmutations
});
```

This registers `IBridgeClient`, the three streams, and a background pump that correlates
results — so command calls are just `await`.

### Sending commands

Fire-and-forget on the wire, awaitable in your code. Action verbs return a `Result`
(inspect `Result.Code`); read verbs return typed data (throw `BridgeCommandException` on
`ok:false`); everything throws `BridgeTimeoutException` if no result arrives in time.

```csharp
public class RosterService(IBridgeClient bridge)
{
    public async Task ExampleAsync(string steam)
    {
        // Action verbs — use the Species / VitalName constants to avoid typos.
        Result swap = await bridge.SwapAsync(steam, Species.Tyrannosaurus, growth: 1.0);
        if (swap.Code == ResultCode.NotWhitelisted) { /* ... */ }

        await bridge.SetStatsAsync(steam, new SetStatsArgs { Growth = 0.75, Health = 9000, Prime = true });
        await bridge.SetVitalAsync(steam, VitalName.Thirst, 900);
        await bridge.TeleportAsync(steam, x: 12345, y: 67890, z: 22500, yaw: 90);
        await bridge.NotifyAsync(steam, "Welcome back!");

        // Read verbs — typed payloads.
        PlayersData players = await bridge.GetPlayersAsync();
        StatsSnapshot stats = await bridge.GetStatsAsync(steam);
        PositionData pos     = await bridge.GetPosAsync(steam);
        PingData ping        = await bridge.PingAsync();

        // Skin: fully typed customizer (engine field names handled internally).
        await bridge.SetSkinAsync(steam, new SkinCustomizer
        {
            BodyColor = new SkinColor(0.8, 0.2, 0.1),
            PatternIndex = 1
        });

        // Escape hatch for any verb this SDK version doesn't wrap:
        Result raw = await bridge.SendAsync("someNewVerb", steam, new { foo = 1 });
    }
}
```

### Consuming streams

Streams are `IAsyncEnumerable<T>` — no HTTP, offsets, or reconnection to manage.

```csharp
public class ChatWatcher(IChatStream chat, IEventStream events) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await foreach (ChatMessage msg in chat.StreamAsync(ct))
        {
            if (msg.ChatMode == ChatMode.Global && msg.Text.StartsWith('!'))
                // parse your !commands here (chat-command handling is your side)
        }
    }
}

// events:
await foreach (GameEvent e in events.StreamAsync(ct))
    if (e.Kind == EventKind.Join) { /* ... */ }

// stats: simply enumerating keeps the server-side flood guard fed
await foreach (StatsSnapshot s in stats.StreamAsync(ct)) { /* ... */ }
```

### Skin persistence (relog reapply)

Direct-write skins revert on relog, so the C# side owns persistence. Implement `ISkinStore`
over your DB and enable the reapply flow — the SDK reapplies on `join` (after a delay) and for
everyone already online at boot:

```csharp
public sealed class MySkinStore(MyDb db) : ISkinStore
{
    public async Task<SkinCustomizer?> GetAsync(string steam, CancellationToken ct = default)
        => await db.GetDesiredSkin(steam, ct);
}

builder.Services.AddSingleton<ISkinStore, MySkinStore>();
builder.Services.AddIsleBridge(o =>
{
    o.BaseAddress = new Uri("http://islebridge:8080");
    o.EnableSkinReapply = true;
    o.SkinReapplyDelay = TimeSpan.FromSeconds(4);
});
```

### Named constants

`Species` (shipped roster + provisional in-dev list, with `IsPlayable` / `IsKnown` helpers) and
`VitalName` back the typo-prone string params. Both methods still accept any raw string / full
class path.

### Error handling

| Type | When |
|---|---|
| `BridgeTimeoutException` | No result correlated to the command's id within the timeout. Do **not** blindly resend — retries should reuse the same logical operation. |
| `BridgeCommandException` | A read verb came back with `ok:false`; carries the `Result` and `ResultCode`. |

---

## Verb reference (SDK methods)

| Verb | Method | Returns |
|---|---|---|
| swap | `SwapAsync` | `Result` |
| setstats | `SetStatsAsync` | `Result` |
| prime / unprime | `PrimeAsync` / `UnprimeAsync` | `Result` |
| teleport | `TeleportAsync` | `Result` |
| heal / kill | `HealAsync` / `KillAsync` | `Result` |
| setgrowth | `SetGrowthAsync` | `Result` |
| setvital | `SetVitalAsync` | `Result` |
| setskin | `SetSkinAsync` | `Result` |
| setmutations | `SetMutationsAsync` | `Result` |
| notify | `NotifyAsync` | `Result` |
| getpos | `GetPosAsync` | `PositionData` |
| getstats | `GetStatsAsync` | `StatsSnapshot` |
| getskin | `GetSkinAsync` | `SkinCustomizer` |
| getmutations | `GetMutationsAsync` | `MutationsData` |
| players | `GetPlayersAsync` | `PlayersData` |
| ping | `PingAsync` | `PingData` |
| *(any)* | `SendAsync` | `Result` |

---

## Notes & limits (from the contract)

- Real chat-box / global delivery isn't possible from Lua — `notify` is a HUD popup only; use RCON for real chat.
- `death` events are poll-based, lagged, and lossy — use RCON/KillFeed for exact attribution.
- `swap` may flip gender (no setter); `unprime` may not stick past 75% growth.
- `PatternIndex` is per-species range-validated by the engine; a bad index silently drops the whole skin — you own the valid range.
- Delivery is at-least-once: reuse an `id` when retrying the *same* logical command; the plugin dedups and returns `DUPLICATE`.

## Build

```bash
dotnet build IsleBridge.Api.sln
```

Requires the .NET 10 SDK.