namespace IsleBridge.Integration;

/// <summary>Configuration for the IsleBridge SDK.</summary>
public sealed class IsleBridgeOptions
{
    /// <summary>Base address of the IsleBridge.Api instance, e.g. <c>http://islebridge:8080</c>.</summary>
    public Uri BaseAddress { get; set; } = new("http://localhost:8080");

    /// <summary>Default correlation timeout for most verbs.</summary>
    public TimeSpan DefaultCommandTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>Correlation timeout for the slow verbs (<c>swap</c>, <c>setmutations</c>).</summary>
    public TimeSpan SlowCommandTimeout { get; set; } = TimeSpan.FromSeconds(8);

    /// <summary>
    /// Enable the §5 skin-reapply flow. Requires an <see cref="ISkinStore"/> to be
    /// registered so the SDK knows which customizer to reapply per steam on join.
    /// </summary>
    public bool EnableSkinReapply { get; set; }

    /// <summary>How long to wait after a <c>join</c> before reapplying the skin (contract §5).</summary>
    public TimeSpan SkinReapplyDelay { get; set; } = TimeSpan.FromSeconds(4);
}

/// <summary>Named HttpClient identifiers used by the SDK.</summary>
internal static class ClientNames
{
    public const string Command = "islebridge";
    public const string Stream = "islebridge-stream";
}

/// <summary>Relative Api paths.</summary>
internal static class ApiRoutes
{
    public const string Command = "api/v1/command";
    public const string ChatStream = "api/v1/stream/chat";
    public const string EventsStream = "api/v1/stream/events";
    public const string StatsStream = "api/v1/stream/stats";
    public const string ResultsStream = "api/v1/stream/results";
}
