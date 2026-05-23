namespace GatiFlow.Maui.Example.Views;

public partial class AnalyticsPage : ContentPage
{
    public AnalyticsPage() => InitializeComponent();

    private async void OnTrackEventClicked(object sender, EventArgs e)
    {
        await GatiFlow.Analytics.TrackEvent("button_tap", new()
        {
            ["screen"]  = "AnalyticsDemo",
            ["button"]  = "track_event",
            ["version"] = AppInfo.VersionString,
        });
        SetStatus("Event 'button_tap' sent ✓");
    }

    private async void OnTrackScreenClicked(object sender, EventArgs e)
    {
        await GatiFlow.Analytics.TrackScreen("AnalyticsDemo");
        SetStatus("Screen view 'AnalyticsDemo' sent ✓");
    }

    private async void OnTrackPurchaseClicked(object sender, EventArgs e)
    {
        await GatiFlow.Analytics.TrackEvent("purchase", new()
        {
            ["amount"]   = 9.99,
            ["currency"] = "USD",
            ["sku"]      = "pro_monthly",
        });
        SetStatus("Event 'purchase' ($9.99) sent ✓");
    }

    private void OnSetUserIdClicked(object sender, EventArgs e)
    {
        GatiFlow.SetUserId("user_demo_123");
        SetStatus("User ID set to 'user_demo_123' ✓");
    }

    private void OnClearUserIdClicked(object sender, EventArgs e)
    {
        GatiFlow.SetUserId(null);
        SetStatus("User ID cleared ✓");
    }

    private void SetStatus(string msg)
    {
        StatusLabel.Text      = msg;
        StatusLabel.TextColor = Colors.Green;
    }
}
