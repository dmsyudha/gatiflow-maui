using GatiFlow.Maui.Internal;
using GatiFlow.Maui.Services;

namespace GatiFlow.Maui.Analytics;

/// <summary>
/// Records named events and screen views. Accessed via <see cref="GatiFlow.Analytics"/>.
/// </summary>
public sealed class AnalyticsService : IGatiFlowService
{
    public string Name => "analytics";

    private GatiFlowConfig?  _config;
    private GatiFlowClient?  _client;
    private string?          _userId;
    private bool             _enabled = true;

    // ── IGatiFlowService ───────────────────────────────────────────────────────

    Task IGatiFlowService.StartAsync(GatiFlowConfig config, GatiFlowClient client, CancellationToken ct)
    {
        _config = config;
        _client = client;
        if (config.DebugLogging) GatiFlowLogger.Log("Analytics service started.");
        return Task.CompletedTask;
    }

    Task IGatiFlowService.StopAsync(CancellationToken ct)
    {
        if (_config?.DebugLogging == true) GatiFlowLogger.Log("Analytics service stopped.");
        return Task.CompletedTask;
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Track a named event with optional properties.
    /// <code>
    /// GatiFlow.Analytics.TrackEvent("purchase", new() { ["amount"] = 9.99, ["currency"] = "USD" });
    /// </code>
    /// </summary>
    public Task TrackEvent(string name, Dictionary<string, object?>? properties = null)
    {
        if (!_enabled || _client is null || _config is null) return Task.CompletedTask;

        var payload = new AnalyticsEventPayload(
            EventName:  name,
            Properties: properties ?? [],
            DeviceId:   DeviceInfo.Current.Name ?? "unknown",
            UserId:     _userId,
            OsName:     DeviceInfo.Current.Platform.ToString(),
            OsVersion:  DeviceInfo.Current.VersionString,
            AppVersion: AppInfo.Current.VersionString,
            Timestamp:  DateTimeOffset.UtcNow
        );

        return _client.PostAsync("sdk/v1/events", payload);
    }

    /// <summary>
    /// Track a screen / page view.
    /// <code>
    /// GatiFlow.Analytics.TrackScreen("HomeScreen");
    /// </code>
    /// </summary>
    public Task TrackScreen(string screenName, Dictionary<string, object?>? properties = null)
    {
        var props = new Dictionary<string, object?>(properties ?? []) { ["$screen_name"] = screenName };
        return TrackEvent("$screen_view", props);
    }

    /// <summary>Enable or disable analytics at runtime (e.g. based on user consent).</summary>
    public void SetEnabled(bool enabled) => _enabled = enabled;

    /// <summary>Associate a user ID with all subsequent events. Pass null to clear.</summary>
    public void SetUserId(string? userId) => _userId = userId;
}
