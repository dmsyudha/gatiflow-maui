# GatiFlow SDK for .NET MAUI

Crash reporting, analytics, and app distribution for .NET MAUI apps — iOS, Android, and Mac Catalyst from a single C# codebase.

## Installation

### NuGet (recommended)

```xml
<!-- .csproj -->
<PackageReference Include="GatiFlow.Maui" Version="1.*" />
```

Or via CLI:
```bash
dotnet add package GatiFlow.Maui
```

### GitHub Packages (pre-release)

```xml
<!-- nuget.config -->
<packageSources>
  <add key="gatiflow" value="https://nuget.pkg.github.com/dmsyudha/index.json" />
</packageSources>
```

---

## Quick Start

### Option 1 — MAUI Builder (recommended)

Add one line to `MauiProgram.cs`:

```csharp
using GatiFlow.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseGatiFlow("mhub_YOUR_APP_TOKEN");   // ← add this

        return builder.Build();
    }
}
```

The SDK starts automatically when the app launches on every platform.

### Option 2 — Token from platform config (zero-code)

Store the token in your platform config files and call `UseGatiFlow()` without arguments:

**Android** — `Platforms/Android/AndroidManifest.xml`:
```xml
<application ...>
    <meta-data
        android:name="dev.gatiflow.APP_TOKEN"
        android:value="mhub_YOUR_APP_TOKEN" />
</application>
```

**iOS / Mac Catalyst** — `Platforms/iOS/Info.plist` & `Platforms/MacCatalyst/Info.plist`:
```xml
<key>GatiFlowAppToken</key>
<string>mhub_YOUR_APP_TOKEN</string>
```

Then in `MauiProgram.cs`:
```csharp
builder.UseGatiFlow();   // reads token from platform config automatically
```

### Option 3 — Manual start

```csharp
var config = new GatiFlowConfig.Builder("mhub_YOUR_APP_TOKEN")
    .SetDebugLogging(true)       // verbose logs in debug builds
    .SetFlushIntervalMs(15_000)  // flush analytics every 15 s
    .Build();

await GatiFlow.StartAsync(config);
```

---

## Analytics

```csharp
// Track a custom event
await GatiFlow.Analytics.TrackEvent("button_tap", new()
{
    ["screen"]   = "HomeScreen",
    ["element"]  = "subscribe_cta",
});

// Track a screen / page view
await GatiFlow.Analytics.TrackScreen("HomeScreen");

// Track a purchase
await GatiFlow.Analytics.TrackEvent("purchase", new()
{
    ["amount"]   = 9.99,
    ["currency"] = "USD",
    ["plan"]     = "pro_monthly",
});

// Associate with a logged-in user
GatiFlow.SetUserId("user_42");

// Clear on logout
GatiFlow.SetUserId(null);

// Disable / re-enable analytics (e.g. consent flow)
GatiFlow.Analytics.SetEnabled(false);
```

---

## Crash Reporting

Unhandled exceptions are captured **automatically** — no code required beyond `UseGatiFlow()`. For handled errors:

```csharp
// Report an Exception
try
{
    await CheckoutService.ProcessAsync(cart);
}
catch (Exception ex)
{
    await GatiFlow.Crashes.TrackError(ex, new()
    {
        ["cart_id"] = cart.Id,
        ["items"]   = cart.Count,
    });
}

// Report a plain message
await GatiFlow.Crashes.TrackError("Payment provider returned unexpected status 402.");

// Disable crash reporting (e.g. debug builds)
GatiFlow.Crashes.SetEnabled(false);
```

---

## Advanced Config

```csharp
var config = new GatiFlowConfig.Builder("mhub_YOUR_APP_TOKEN")
    // Self-hosted or staging ingest endpoint
    .SetBaseUrl("https://ingest.yourcompany.com")
    // Print all SDK logs (disable in production!)
    .SetDebugLogging(true)
    // How often to batch-upload events (default 30 s)
    .SetFlushIntervalMs(15_000)
    // Max events per upload batch (default 100)
    .SetMaxEventBatchSize(50)
    // Max crash reports to hold locally before dropping (default 50)
    .SetMaxCrashQueueSize(25)
    .Build();
```

### Select services

```csharp
// Analytics only (no crash reporting)
var config = new GatiFlowConfig.Builder("mhub_YOUR_APP_TOKEN")
    .SetServices(new AnalyticsService())
    .Build();
```

---

## Debug Logging

Enable `SetDebugLogging(true)` during development to see SDK activity in the debug output:

```
[GatiFlow] Starting GatiFlow SDK (token: mhub_Y****0ABC)
[GatiFlow] Analytics service started.
[GatiFlow] Crashes service started.
[GatiFlow] POST sdk/v1/events → 202
```

---

## Requirements

| Platform     | Minimum version   |
|--------------|-------------------|
| Android      | API 21 (5.0)      |
| iOS          | 13.0              |
| Mac Catalyst | 13.1              |
| .NET         | 9.0               |
| .NET MAUI    | 9.0               |

---

## Compatibility with Xamarin

If you are still on **Xamarin.iOS** / **Xamarin.Android** (not MAUI), use the native SDKs directly:
- iOS: [gatiflow-ios](https://github.com/dmsyudha/gatiflow-ios) — Swift Package Manager
- Android: [gatiflow-android](https://github.com/dmsyudha/gatiflow-android) — JitPack

The MAUI SDK wraps the same ingest API — event format is identical across all platforms.

---

## Example App

Open `GatiFlow.Maui.sln` in Visual Studio 2022 (17.8+) or JetBrains Rider and run the `GatiFlow.Maui.Example` target on your preferred platform.

---

## License

MIT © GatiFlow
