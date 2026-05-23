using System.Net.Http.Json;
using System.Text.Json;

namespace GatiFlow.Maui.Internal;

/// <summary>
/// Thin HTTP wrapper shared by all SDK services.
/// Swallows all network errors — the SDK must never crash the host app.
/// </summary>
internal sealed class GatiFlowClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly GatiFlowConfig _config;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    internal GatiFlowClient(GatiFlowConfig config)
    {
        _config = config;
        _http = new HttpClient
        {
            BaseAddress = new Uri(config.BaseUrl.TrimEnd('/') + "/"),
            Timeout     = TimeSpan.FromSeconds(15),
        };
        _http.DefaultRequestHeaders.Add("x-app-token", config.AppToken);
        _http.DefaultRequestHeaders.Add("x-sdk",       "maui/1.0.0");
    }

    /// <summary>
    /// POST JSON to <paramref name="path"/>. Silently no-ops on any error.
    /// </summary>
    internal async Task PostAsync<T>(string path, T payload, CancellationToken ct = default)
    {
        try
        {
            using var response = await _http
                .PostAsJsonAsync(path, payload, JsonOpts, ct)
                .ConfigureAwait(false);

            if (_config.DebugLogging)
                GatiFlowLogger.Log($"POST {path} → {(int)response.StatusCode}");
        }
        catch (OperationCanceledException) { /* intentional shutdown */ }
        catch (Exception ex)
        {
            if (_config.DebugLogging)
                GatiFlowLogger.Log($"[GatiFlow] POST {path} failed: {ex.Message}");
        }
    }

    public void Dispose() => _http.Dispose();
}
