using Foundation;

namespace GatiFlow.Maui;

// Mac Catalyst shares the same Info.plist pattern as iOS.
public static partial class GatiFlowPlatform
{
    /// <summary>
    /// Read the app token from <c>Info.plist</c>:
    /// <code>
    /// &lt;key&gt;GatiFlowAppToken&lt;/key&gt;
    /// &lt;string&gt;mhub_YOUR_TOKEN&lt;/string&gt;
    /// </code>
    /// Returns null if the key is absent.
    /// </summary>
    public static string? ReadTokenFromPlist()
    {
        return NSBundle.MainBundle.InfoDictionary?["GatiFlowAppToken"]?.ToString();
    }
}
