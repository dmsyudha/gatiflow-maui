namespace GatiFlow.Maui.Analytics;

internal sealed record AnalyticsEventPayload(
    string EventName,
    Dictionary<string, object?> Properties,
    string DeviceId,
    string? UserId,
    string OsName,
    string OsVersion,
    string AppVersion,
    DateTimeOffset Timestamp
);
