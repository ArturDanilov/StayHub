using Foundation;
using UIKit;

namespace StayHub.Mobile;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() =>
        MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(
        UIApplication application,
        NSDictionary? launchOptions)
    {
        var result = base.FinishedLaunching(application, launchOptions);

        AddKeyboardDismissGesture();

        return result;
    }

    private static void AddKeyboardDismissGesture()
    {
        var window = UIApplication.SharedApplication
            .ConnectedScenes
            .OfType<UIWindowScene>()
            .SelectMany(scene => scene.Windows)
            .FirstOrDefault(window => window.IsKeyWindow);

        if (window is null)
            return;

        var tapGesture = new UITapGestureRecognizer(() =>
        {
            window.EndEditing(true);
        })
        {
            CancelsTouchesInView = false
        };

        window.AddGestureRecognizer(tapGesture);
    }
}
