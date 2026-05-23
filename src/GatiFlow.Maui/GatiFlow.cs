using GatiFlow.Maui.Analytics;
using GatiFlow.Maui.Crashes;
using GatiFlow.Maui.Internal;
using GatiFlow.Maui.Services;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace GatiFlow.Maui;

/// <summary>
/// Main entry point for the GatiFlow SDK.
///
/// <para><b>Option 1 — MAUI builder (recommended)</b></para>
/// <code>
/// // MauiProgram.cs
/// builder.UseGatiFlow("mhub_YOUR_TOKEN");
/// </code>
///
/// <para><b>Option 2 — manual start</b></para>
/// <code>
/// await GatiFlow.StartAsync(new GatiFlowConfig.Builder("mhub_YOUR_TOKEN").Build());
/// </code>
///
/// <para><b>Option 3 — token from platform config</b></para>
/// <code>
/// await GatiFlow.StartAsync(); // reads from AndroidManifest / Info.plist
/// </code>
/// </summary>
public static class GatiFlow
{
    private static GatiFlowClient?     _client;   // kept typed for Dispose()
    private static GatiFlowConfig?     _config;
    private static bool                _started;

    // ── Public service accessors ───────────────────────────────────────────────

    /// <summary>Analytics service for tracking events and screen views.</summary>
    public static AnalyticsService Analytics { get; } = new();

    /// <summary>Crashes service for handled and unhandled exception reporting.</summary>
    public static CrashesService   Crashes   { get; } = new();

    // ── Initialization ─────────────────────────────────────────────────────────

    /// <summary>
    /// Start GatiFlow with an explicit config.
    /// Safe to call multiple times — subsequent calls are no-ops.
    /// </summary>
    public static Task StartAsync(GatiFlowConfig config, CancellationToken ct = default)
        => InitAsync(config, ct);

    /// <summary>
    /// Start GatiFlow with a token shorthand (uses default config otherwise).
    /// </summary>
    public static Task StartAsync(string appToken, CancellationToken ct = default)
        => InitAsync(new GatiFlowConfig.Builder(appToken).Build(), ct);

    /// <summary>
    /// Start GatiFlow by reading the token from the platform-native config file.
    /// Android: <c>AndroidManifest.xml</c> meta-data key <c>dev.gatiflow.APP_TOKEN</c>.
    /// iOS/macOS: <c>Info.plist</c> key <c>GatiFlowAppToken</c>.
    /// Throws <see cref="InvalidOperationException"/> if no token is found.
    /// </summary>
    public static Task StartAsync(CancellationToken ct = default)
    {
        var token = GatiFlowPlatform.ReadTokenFromPlatform()
            ?? throw new InvalidOperationException(
                "GatiFlow: no app token found. " +
                "Add dev.gatiflow.APP_TOKEN to AndroidManifest.xml (Android) " +
                "or GatiFlowAppToken to Info.plist (iOS/macOS).");

        return InitAsync(new GatiFlowConfig.Builder(token).Build(), ct);
    }

    /// <summary>Gracefully stop all services and release resources.</summary>
    public static async Task StopAsync(CancellationToken ct = default)
    {
        if (!_started || _config is null) return;

        foreach (var svc in _config.Services)
        {
            try { await svc.StopAsync(ct).ConfigureAwait(false); }
            catch { /* never let service teardown crash the app */ }
        }

        _client?.Dispose();
        _client  = null;
        _started = false;
    }

    /// <summary>
    /// Associate a user identity with all subsequent events and crash reports.
    /// Call after login; pass null to clear on logout.
    /// </summary>
    public static void SetUserId(string? userId)
    {
        Analytics.SetUserId(userId);
        Crashes.SetUserId(userId);
    }

    // ── Internal ───────────────────────────────────────────────────────────────

    private static async Task InitAsync(GatiFlowConfig config, CancellationToken ct)
    {
        if (_started) return;
        _started = true;
        _config  = config;
        _client  = new GatiFlowClient(config);

        if (config.DebugLogging)
            GatiFlowLogger.Log($"Starting GatiFlow SDK (token: {Mask(config.AppToken)})");

        // Always start the two built-in services, then any extras
        await StartServiceIfRegistered(Analytics, config, ct);
        await StartServiceIfRegistered(Crashes,   config, ct);

        foreach (var svc in config.Services)
        {
            if (svc is AnalyticsService || svc is CrashesService) continue; // already started
            await StartServiceIfRegistered(svc, config, ct);
        }
    }

    private static async Task StartServiceIfRegistered(
        IGatiFlowService service, GatiFlowConfig config, CancellationToken ct)
    {
        // Only start services that appear in the config service list (by name)
        bool requested = config.Services.Any(s => s.Name == service.Name || s == service);
        if (!requested) return;

        try
        {
            await service.StartAsync(config, _client!, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (config.DebugLogging)
                GatiFlowLogger.Log($"Failed to start service '{service.Name}': {ex.Message}");
        }
    }

    private static string Mask(string token) =>
        token.Length <= 8 ? "***" : token[..6] + "****" + token[^4..];
}

// ── Partial helper — platform token reading ────────────────────────────────────

public static partial class GatiFlowPlatform
{
    /// <summary>
    /// Reads the app token from the appropriate platform config file.
    /// Implemented in each Platforms/ folder via partial methods.
    /// </summary>
    public static string? ReadTokenFromPlatform()
    {
#if ANDROID
        return ReadTokenFromManifest();
#elif IOS || MACCATALYST
        return ReadTokenFromPlist();
#else
        return null;
#endif
    }
}

// ── MauiAppBuilder extension ───────────────────────────────────────────────────

public static class GatiFlowMauiExtensions
{
    /// <summary>
    /// Register GatiFlow and start it when the MAUI app launches.
    ///
    /// <code>
    /// // MauiProgram.cs
    /// var builder = MauiApp.CreateBuilder();
    /// builder.UseMauiApp&lt;App&gt;()
    ///        .UseGatiFlow("mhub_YOUR_TOKEN");
    /// </code>
    ///
    /// Or use a full config:
    /// <code>
    /// builder.UseGatiFlow(opts => opts
    ///     .SetDebugLogging(true));
    /// </code>
    /// </summary>
    public static MauiAppBuilder UseGatiFlow(
        this MauiAppBuilder builder,
        string appToken,
        Action<GatiFlowConfig.Builder>? configure = null)
    {
        var b = new GatiFlowConfig.Builder(appToken);
        configure?.Invoke(b);
        var config = b.Build();

        builder.ConfigureLifecycleEvents(events =>
        {
#if ANDROID
            events.AddAndroid(android => android
                .OnCreate((activity, _) => _ = GatiFlow.StartAsync(config)));
#elif IOS || MACCATALYST
            events.AddiOS(ios => ios
                .WillFinishLaunching((app, _) =>
                {
                    _ = GatiFlow.StartAsync(config);
                    return true;
                }));
#endif
        });

        return builder;
    }

    /// <summary>
    /// Reads the token from AndroidManifest.xml / Info.plist and starts GatiFlow.
    /// </summary>
    public static MauiAppBuilder UseGatiFlow(
        this MauiAppBuilder builder,
        Action<GatiFlowConfig.Builder>? configure = null)
    {
        builder.ConfigureLifecycleEvents(events =>
        {
#if ANDROID
            events.AddAndroid(android => android
                .OnCreate((activity, _) => _ = GatiFlow.StartAsync()));
#elif IOS || MACCATALYST
            events.AddiOS(ios => ios
                .WillFinishLaunching((app, _) =>
                {
                    _ = GatiFlow.StartAsync();
                    return true;
                }));
#endif
        });

        return builder;
    }
}
