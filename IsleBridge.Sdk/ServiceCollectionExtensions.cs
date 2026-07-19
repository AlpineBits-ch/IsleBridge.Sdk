using IsleBridge.Integration.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IsleBridge.Integration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the IsleBridge SDK: the typed <see cref="IBridgeClient"/>, the
    /// <see cref="IChatStream"/> / <see cref="IEventStream"/> / <see cref="IStatsStream"/>
    /// feeds, and the background result-correlation pump. If
    /// <see cref="IsleBridgeOptions.EnableSkinReapply"/> is set, also wires the §5 skin
    /// reapply flow (register your <see cref="ISkinStore"/> beforehand).
    /// </summary>
    public static IServiceCollection AddIsleBridge(
        this IServiceCollection services, Action<IsleBridgeOptions> configure)
    {
        services.Configure(configure);

        var probe = new IsleBridgeOptions();
        configure(probe);

        services.AddHttpClient(ClientNames.Command, (sp, c) =>
        {
            c.BaseAddress = sp.GetRequiredService<IOptions<IsleBridgeOptions>>().Value.BaseAddress;
        });

        services.AddHttpClient(ClientNames.Stream, (sp, c) =>
        {
            c.BaseAddress = sp.GetRequiredService<IOptions<IsleBridgeOptions>>().Value.BaseAddress;
            c.Timeout = Timeout.InfiniteTimeSpan; // SSE connections are long-lived
        });

        services.AddSingleton<SseClient>();
        services.AddSingleton<CommandCorrelator>();

        services.AddSingleton<IBridgeClient, BridgeClient>();
        services.AddSingleton<IChatStream, ChatStream>();
        services.AddSingleton<IEventStream, EventStream>();
        services.AddSingleton<IStatsStream, StatsStream>();

        services.AddHostedService<ResultPump>();

        if (probe.EnableSkinReapply)
            services.AddHostedService<SkinReapplyService>();

        return services;
    }
}
