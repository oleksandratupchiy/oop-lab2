using Foundation;

namespace MauiApp24
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        // 👇 Замініть MauiApp на Microsoft.Maui.Hosting.MauiApp
        protected override Microsoft.Maui.Hosting.MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}