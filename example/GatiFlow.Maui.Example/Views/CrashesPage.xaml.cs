namespace GatiFlow.Maui.Example.Views;

public partial class CrashesPage : ContentPage
{
    public CrashesPage() => InitializeComponent();

    private async void OnTrackHandledClicked(object sender, EventArgs e)
    {
        try
        {
            // Simulate a handled error
            throw new InvalidOperationException("Simulated handled exception from MAUI example.");
        }
        catch (Exception ex)
        {
            await GatiFlow.Crashes.TrackError(ex);
            SetStatus("Handled exception reported ✓");
        }
    }

    private async void OnTrackMessageClicked(object sender, EventArgs e)
    {
        await GatiFlow.Crashes.TrackError("Something went wrong in the checkout flow.");
        SetStatus("Error message reported ✓");
    }

    private async void OnTrackWithMetadataClicked(object sender, EventArgs e)
    {
        try
        {
            throw new HttpRequestException("Network timeout after 15s");
        }
        catch (Exception ex)
        {
            await GatiFlow.Crashes.TrackError(ex, new()
            {
                ["endpoint"] = "/api/checkout",
                ["retries"]  = 3,
                ["userId"]   = "user_demo_123",
            });
            SetStatus("Error with metadata reported ✓");
        }
    }

    private void OnThrowUnhandledClicked(object sender, EventArgs e)
    {
        // This will be caught by the global UnhandledException handler
        // registered in CrashesService.StartAsync
        throw new Exception("Test unhandled exception — captured by GatiFlow automatically.");
    }

    private void SetStatus(string msg)
    {
        StatusLabel.Text      = msg;
        StatusLabel.TextColor = Colors.Green;
    }
}
