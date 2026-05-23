namespace GatiFlow.Maui.Services;

/// <summary>
/// Implemented by every pluggable GatiFlow feature (Analytics, Crashes, …).
/// </summary>
public interface IGatiFlowService
{
    /// <summary>Unique name used for logging and service-list configuration.</summary>
    string Name { get; }

    /// <summary>Called once by <see cref="GatiFlow.StartAsync"/> after config is validated.</summary>
    Task StartAsync(GatiFlowConfig config, IGatiFlowTransport transport, CancellationToken ct = default);

    /// <summary>Called on app teardown or explicit <see cref="GatiFlow.StopAsync"/>.</summary>
    Task StopAsync(CancellationToken ct = default);
}
