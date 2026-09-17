using StayHub.Mobile.Services;
using StayHub.Mobile.Views;

namespace StayHub.Mobile;

public partial class AppShell
{
    public AppShell(
        TodayPage todayPage,
        PropertiesPage propertiesPage,
        ReservationsPage reservationsPage,
        AssistantPage assistantPage,
        AccountPage accountPage)
    {
        InitializeComponent();
        TodayContent.Content = todayPage;
        PropertiesContent.Content = propertiesPage;
        AssistantContent.Content = assistantPage;
        ReservationsContent.Content = reservationsPage;
        AccountContent.Content = accountPage;

        AddStaffChatToolbarItem(todayPage);
        AddStaffChatToolbarItem(propertiesPage);
        AddStaffChatToolbarItem(reservationsPage);
        AddStaffChatToolbarItem(accountPage);
    }

    private static void AddStaffChatToolbarItem(ContentPage page)
    {
        var staffChatItem = new ToolbarItem
        {
            Text = "💬",
            Order = ToolbarItemOrder.Primary,
            Priority = 0
        };

        SemanticProperties.SetDescription(staffChatItem, "Open staff chat");
        staffChatItem.Clicked += async (_, _) =>
            await page.DisplayAlertAsync(
                "Staff chat",
                "Team messaging will be available here soon.",
                "OK");

        page.ToolbarItems.Add(staffChatItem);
    }
}
