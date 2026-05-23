namespace GatiFlow.Maui.Internal;

internal static class GatiFlowLogger
{
    internal static void Log(string message) =>
        System.Diagnostics.Debug.WriteLine($"[GatiFlow] {message}");
}
