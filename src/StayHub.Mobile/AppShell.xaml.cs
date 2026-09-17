using StayHub.Mobile.Services;
using StayHub.Mobile.Views;

namespace StayHub.Mobile;

public partial class AppShell
{
    private readonly AssistantPage _assistantPage;

    public AppShell(
        TodayPage todayPage,
        PropertiesPage propertiesPage,
        ReservationsPage reservationsPage,
        AssistantPage assistantPage,
        AccountPage accountPage)
    {
        _assistantPage = assistantPage;
        InitializeComponent();
        TodayContent.Content = todayPage;
        PropertiesContent.Content = propertiesPage;
        ReservationsContent.Content = reservationsPage;
        AccountContent.Content = accountPage;

        AddAssistantToolbarItem(todayPage);
        AddAssistantToolbarItem(propertiesPage);
        AddAssistantToolbarItem(reservationsPage);
        AddAssistantToolbarItem(accountPage);
    }

    private void AddAssistantToolbarItem(ContentPage page)
    {
        var assistantItem = new ToolbarItem
        {
            Text = "💬",
            Order = ToolbarItemOrder.Primary,
            Priority = 0
        };

        SemanticProperties.SetDescription(assistantItem, "Open AI chat");
        assistantItem.Clicked += OnAssistantClicked;
        page.ToolbarItems.Add(assistantItem);
    }

    private async void OnAssistantClicked(object? sender, EventArgs e)
    {
        if (Navigation.NavigationStack.LastOrDefault() == _assistantPage)
            return;

        await Navigation.PushAsync(_assistantPage);
    }
}
