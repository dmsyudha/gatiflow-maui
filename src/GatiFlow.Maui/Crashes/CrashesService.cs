using GatiFlow.Maui.Services;

namespace GatiFlow.Maui.Crashes;

/// <summary>
/// Captures unhandled and handled exceptions. Accessed via <see cref="GatiFlow.Crashes"/>.
/// </summary>
public sealed class CrashesService : IGatiFlowService
{
    public string Name => "crashes";

    private GatiFlowConfig?      _config;
    private IGatiFlowTransport?  _client;
    private string?         _userId;
    private bool            _enabled = true;

    // ── IGatiFlowService ───────────────────────────────────────────────────────

    Task IGatiFlowService.StartAsync(GatiFlowConfig config, IGatiFlowTransport client, CancellationToken ct)
    {
        _config = config;
        _client = client;

        // Hook global unhandled exceptions
        AppDomain.CurrentDomain.UnhandledException        += OnUnhandledException;
        TaskScheduler.UnobservedTaskException             += OnUnobservedTaskException;

        if (config.DebugLogging) GatiFlowLogger.Log("Crashes service started.");
        return Task.CompletedTask;
    }

    Task IGatiFlowService.StopAsync(CancellationToken ct)
    {
        AppDomain.CurrentDomain.UnhandledException        -= OnUnhandledException;
        TaskScheduler.UnobservedTaskException             -= OnUnobservedTaskException;
        if (_config?.DebugLogging == true) GatiFlowLogger.Log("Crashes service stopped.");
        return Task.CompletedTask;
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Report a handled exception.
    /// <code>
    /// try { await RiskyOp(); }
    /// catch (Exception ex) { GatiFlow.Crashes.TrackError(ex); }
    /// </code>
    /// </summary>
    public Task TrackError(Exception exception, Dictionary<string, object?>? metadata = null) =>
        SendReport(exception, isFatal: false, metadata);

    /// <summary>Report an error by message string (useful for non-Exception errors).</summary>
    public Task TrackError(string message, Dictionary<string, object?>? metadata = null) =>
        SendReport(new Exception(message), isFatal: false, metadata);

    /// <summary>Enable or disable crash reporting at runtime.</summary>
    public void SetEnabled(bool enabled) => _enabled = enabled;

    /// <summary>Associate a user ID with all subsequent crash reports. Pass null to clear.</summary>
    public void SetUserId(string? userId) => _userId = userId;

    // ── Private helpers ────────────────────────────────────────────────────────

    private Task SendReport(Exception ex, bool isFatal, Dictionary<string, object?>? metadata)
    {
        if (!_enabled || _client is null || _config is null) return Task.CompletedTask;

        var payload = new CrashReportPayload(
            Message:       ex.Message,
            StackTrace:    ex.StackTrace,
            ExceptionType: ex.GetType().FullName ?? ex.GetType().Name,
            IsFatal:       isFatal,
            DeviceId:      DeviceInfo.Current.Name ?? "unknown",
            UserId:        _userId,
            OsName:        DeviceInfo.Current.Platform.ToString(),
            OsVersion:     DeviceInfo.Current.VersionString,
            AppVersion:    AppInfo.Current.VersionString,
            Metadata:      metadata,
            Timestamp:     DateTimeOffset.UtcNow
        );

        return _client.PostAsync("sdk/v1/crashes", payload);
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            _ = SendReport(ex, isFatal: e.IsTerminating, metadata: null);
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _ = SendReport(e.Exception, isFatal: false, metadata: null);
        e.SetObserved(); // prevent process termination on unobserved task exceptions
    }
}
