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

    public override void ViewDidLayoutSubviews()
    {
        base.ViewDidLayoutSubviews();

        var tabBarController = FindTabBarController(this);
        if (tabBarController is not null)
            StayHubTabBarAppearanceTracker.ConfigureItems(tabBarController.TabBar);
    }

    private static UITabBarController? FindTabBarController(UIViewController controller)
    {
        if (controller is UITabBarController tabBarController)
            return tabBarController;

        foreach (var child in controller.ChildViewControllers)
        {
            var nestedTabBarController = FindTabBarController(child);
            if (nestedTabBarController is not null)
                return nestedTabBarController;
        }

        return null;
    }
}

public sealed class StayHubTabBarAppearanceTracker : ShellTabBarAppearanceTracker
{
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

        ConfigureItems(tabBar);
    }

    internal static void ConfigureItems(UITabBar tabBar)
    {
        if (tabBar.Items is null)
            return;

        foreach (var item in tabBar.Items)
        {
            var title = item.Title ?? item.AccessibilityLabel;
            var isAssistant = title == "AI";
            item.AccessibilityLabel = title;
            item.Image = GetTabIcon(title);
            item.SelectedImage = GetTabIcon(title);
            item.Title = null;
            item.ImageInsets = isAssistant
                ? new UIEdgeInsets(2, 0, -2, 0)
                : new UIEdgeInsets(6, 0, -6, 0);
        }
    }

    private static UIImage? GetTabIcon(string? title)
    {
        var symbolName = title switch
        {
            "Today" => "calendar",
            "Properties" => "building.2",
            "AI" => "sparkles",
            "Bookings" => "book.closed",
            "Sync" => "arrow.triangle.2.circlepath",
            "Users" => "person.2",
            "Account" => "person.crop.circle",
            _ => null
        };

        return symbolName is null ? null : UIImage.GetSystemImage(symbolName);
    }

    private static void ConfigureLayout(UITabBarItemAppearance appearance)
    {
        appearance.Normal.TitleTextAttributes = new UIStringAttributes
        {
            ForegroundColor = UIColor.Clear
        };

        appearance.Selected.TitleTextAttributes = new UIStringAttributes
        {
            ForegroundColor = UIColor.Clear
        };
    }
}
