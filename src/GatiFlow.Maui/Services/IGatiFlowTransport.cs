namespace GatiFlow.Maui.Services;

/// <summary>
/// Public abstraction over the SDK's HTTP transport.
/// Passed to <see cref="IGatiFlowService.StartAsync"/> so services can post payloads
/// without depending on the internal <c>GatiFlowClient</c> implementation.
/// </summary>
public interface IGatiFlowTransport
{
    /// <summary>POST a JSON payload to <paramref name="path"/>. Never throws.</summary>
    Task PostAsync<T>(string path, T payload, CancellationToken ct = default);
}
