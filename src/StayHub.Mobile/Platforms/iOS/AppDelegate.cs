using Foundation;
using UIKit;

namespace StayHub.Mobile;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
    {
        ConfigureTabBar();
        return base.FinishedLaunching(application, launchOptions);
    }

    private static void ConfigureTabBar()
    {
        var tabBar = UITabBar.Appearance;
        tabBar.ItemPositioning = UITabBarItemPositioning.Fill;
        tabBar.ItemSpacing = 0;

        var item = UITabBarItem.Appearance;
        item.TitlePositionAdjustment = new UIOffset(0, -11);
        var attributes = new UIStringAttributes
        {
            Font = UIFont.SystemFontOfSize(16, UIFontWeight.Semibold)
        };
        item.SetTitleTextAttributes(attributes, UIControlState.Normal);
        item.SetTitleTextAttributes(attributes, UIControlState.Selected);
    }
}
