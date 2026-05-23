using GatiFlow.Maui.Analytics;
using GatiFlow.Maui.Crashes;
using GatiFlow.Maui.Services;

namespace GatiFlow.Maui;

/// <summary>
/// Immutable configuration snapshot passed to every GatiFlow service.
/// Build one with <see cref="Builder"/>:
/// <code>
/// var config = new GatiFlowConfig.Builder("mhub_YOUR_TOKEN")
///     .SetDebugLogging(true)
///     .Build();
/// </code>
/// </summary>
public sealed class GatiFlowConfig
{
    /// <summary>Your app token from the GatiFlow dashboard.</summary>
    public string AppToken { get; }

    /// <summary>Base URL for the ingest API. Defaults to the GatiFlow cloud endpoint.</summary>
    public string BaseUrl { get; }

    /// <summary>Emit verbose SDK logs to the console. Keep false in production.</summary>
    public bool DebugLogging { get; }

    /// <summary>Maximum number of crash reports to queue locally before dropping oldest.</summary>
    public int MaxCrashQueueSize { get; }

    /// <summary>Maximum events per upload batch.</summary>
    public int MaxEventBatchSize { get; }

    /// <summary>How often (ms) to auto-flush the event queue.</summary>
    public int FlushIntervalMs { get; }

    /// <summary>Services registered for this SDK instance.</summary>
    public IReadOnlyList<IGatiFlowService> Services { get; }

    private GatiFlowConfig(Builder b)
    {
        AppToken         = b.AppToken;
        BaseUrl          = b.BaseUrl;
        DebugLogging     = b.DebugLogging;
        MaxCrashQueueSize = b.MaxCrashQueueSize;
        MaxEventBatchSize = b.MaxEventBatchSize;
        FlushIntervalMs  = b.FlushIntervalMs;
        Services         = b.Services.AsReadOnly();
    }

    // ── Builder ────────────────────────────────────────────────────────────────

    public sealed class Builder
    {
        internal readonly string AppToken;
        internal string BaseUrl          = "https://ingest.gatiflow.dev";
        internal bool   DebugLogging     = false;
        internal int    MaxCrashQueueSize = 50;
        internal int    MaxEventBatchSize = 100;
        internal int    FlushIntervalMs   = 30_000;
        internal List<IGatiFlowService> Services = [new AnalyticsService(), new CrashesService()];

        /// <param name="appToken">Required. App token from the GatiFlow dashboard.</param>
        public Builder(string appToken)
        {
            if (string.IsNullOrWhiteSpace(appToken))
                throw new ArgumentException("App token must not be empty.", nameof(appToken));
            AppToken = appToken;
        }

        /// <summary>Override the ingest endpoint (self-hosted or staging).</summary>
        public Builder SetBaseUrl(string baseUrl)               { BaseUrl = baseUrl; return this; }

        /// <summary>Print verbose SDK logs. Disable in production.</summary>
        public Builder SetDebugLogging(bool enabled)            { DebugLogging = enabled; return this; }

        /// <summary>Replace the default service list.</summary>
        public Builder SetServices(params IGatiFlowService[] services)
        {
            Services = [..services];
            return this;
        }

        public Builder SetMaxCrashQueueSize(int size)           { MaxCrashQueueSize = size; return this; }
        public Builder SetMaxEventBatchSize(int size)           { MaxEventBatchSize = size; return this; }
        public Builder SetFlushIntervalMs(int ms)               { FlushIntervalMs = ms; return this; }

        public GatiFlowConfig Build() => new(this);
    }
}
