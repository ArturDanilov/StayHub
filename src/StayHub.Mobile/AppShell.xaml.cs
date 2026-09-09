namespace StayHub.Mobile;

using StayHub.Contracts.Users;
using StayHub.Mobile.Views;
using StayHub.Mobile.Services;

public partial class AppShell : Shell
{
    private readonly IAuthService _authService;

    public AppShell(
        PropertiesPage propertiesPage,
        ReservationsPage reservationsPage,
        AssistantPage assistantPage,
        UsersPage usersPage,
        IAuthService authService)
    {
        _authService = authService;
        InitializeComponent();
        PropertiesContent.Content = propertiesPage;
        ReservationsContent.Content = reservationsPage;
        AssistantContent.Content = assistantPage;
        UsersContent.Content = usersPage;
        UsersContent.IsVisible = false;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        UsersContent.IsVisible = await _authService.GetRoleAsync() == UserRoles.Admin;
    }
}
