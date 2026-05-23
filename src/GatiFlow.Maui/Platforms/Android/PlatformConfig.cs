using Android.Content;
using Android.Content.PM;

namespace GatiFlow.Maui;

// Android-specific extension: reads GatiFlowAppToken from AndroidManifest meta-data.
public static partial class GatiFlowPlatform
{
    /// <summary>
    /// Read the app token from <c>AndroidManifest.xml</c>:
    /// <code>
    /// &lt;meta-data android:name="dev.gatiflow.APP_TOKEN" android:value="mhub_YOUR_TOKEN" /&gt;
    /// </code>
    /// Returns null if the key is absent.
    /// </summary>
    public static string? ReadTokenFromManifest()
    {
        try
        {
            var ctx  = Android.App.Application.Context;
            var info = ctx.PackageManager!.GetApplicationInfo(
                ctx.PackageName!, PackageInfoFlags.MetaData);
            return info.MetaData?.GetString("dev.gatiflow.APP_TOKEN");
        }
        catch
        {
            return null;
        }
    }
}
