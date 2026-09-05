namespace StayHub.Mobile;

using StayHub.Mobile.Views;

public partial class AppShell : Shell
{
    public AppShell(
        PropertiesPage propertiesPage,
        ReservationsPage reservationsPage,
        AssistantPage assistantPage)
    {
        InitializeComponent();
        PropertiesContent.Content = propertiesPage;
        ReservationsContent.Content = reservationsPage;
        AssistantContent.Content = assistantPage;
    }
}
