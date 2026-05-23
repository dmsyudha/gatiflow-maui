namespace GatiFlow.Maui.Crashes;

internal sealed record CrashReportPayload(
    string Message,
    string? StackTrace,
    string ExceptionType,
    bool IsFatal,
    string DeviceId,
    string? UserId,
    string OsName,
    string OsVersion,
    string AppVersion,
    Dictionary<string, object?>? Metadata,
    DateTimeOffset Timestamp
);
