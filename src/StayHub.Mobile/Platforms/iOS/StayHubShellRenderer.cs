using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using UIKit;

namespace StayHub.Mobile;

public sealed class StayHubShellRenderer : ShellRenderer
{
    protected override IShellTabBarAppearanceTracker CreateTabBarAppearanceTracker()
    {
        return new StayHubTabBarAppearanceTracker();
    }
}

public sealed class StayHubTabBarAppearanceTracker : ShellTabBarAppearanceTracker
{
    public StayHubTabBarAppearanceTracker() : base()
    {
    }

    public override void SetAppearance(
        UITabBarController controller,
        ShellAppearance appearance)
    {
        base.SetAppearance(controller, appearance);

        var tabBar = controller.TabBar;
        
        controller.AdditionalSafeAreaInsets =
            new UIEdgeInsets(0, 0, 12, 0);

        var tabAppearance = tabBar.StandardAppearance;

        ConfigureLayout(tabAppearance.StackedLayoutAppearance);
        ConfigureLayout(tabAppearance.InlineLayoutAppearance);
        ConfigureLayout(tabAppearance.CompactInlineLayoutAppearance);

        tabBar.StandardAppearance = tabAppearance;
        tabBar.ScrollEdgeAppearance = tabAppearance;

        if (tabBar.Items is null)
            return;

        foreach (var item in tabBar.Items)
        {
            item.TitlePositionAdjustment = new UIOffset(0, -10);
        }
    }

    private static void ConfigureLayout(UITabBarItemAppearance appearance)
    {
        appearance.Normal.TitlePositionAdjustment =
            new UIOffset(0, -10);

        appearance.Normal.TitleTextAttributes =
            new UIStringAttributes
            {
                Font = UIFont.SystemFontOfSize(
                    14,
                    UIFontWeight.Semibold)
            };

        appearance.Selected.TitlePositionAdjustment =
            new UIOffset(0, -10);

        appearance.Selected.TitleTextAttributes =
            new UIStringAttributes
            {
                Font = UIFont.SystemFontOfSize(
                    14,
                    UIFontWeight.Bold)
            };
    }
}
