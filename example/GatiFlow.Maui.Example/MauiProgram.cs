using GatiFlow.Maui;
using Microsoft.Extensions.Logging;

namespace GatiFlow.Maui.Example;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf",    "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf",   "OpenSansSemibold");
            })

            // ── GatiFlow SDK ───────────────────────────────────────────────────────
            // Option A: pass token directly
            .UseGatiFlow("mhub_YOUR_APP_TOKEN", opts => opts.SetDebugLogging(true));

            // Option B: read token from AndroidManifest.xml / Info.plist
            // .UseGatiFlow();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
