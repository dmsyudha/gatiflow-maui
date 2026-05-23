## 1.0.0

* Initial release.
* Targets `net9.0-android`, `net9.0-ios`, `net9.0-maccatalyst`.
* `UseGatiFlow()` MAUI builder extension — starts SDK automatically on app launch.
* Analytics: `TrackEvent`, `TrackScreen`, `SetEnabled`, `SetUserId`.
* Crashes: `TrackError`, `SetEnabled`, automatic `UnhandledException` and `UnobservedTaskException` capture.
* `GatiFlowConfig` builder with `BaseUrl`, `DebugLogging`, `Services`, queue and flush controls.
* `GatiFlow.SetUserId` propagates identity to all services.
* Token auto-read from `AndroidManifest.xml` (Android) and `Info.plist` (iOS / Mac Catalyst).
