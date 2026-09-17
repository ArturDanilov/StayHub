namespace StayHub.Mobile;

using StayHub.Contracts.Users;
using StayHub.Mobile.Views;
using StayHub.Mobile.Services;

public partial class AppShell : Shell
{
    private readonly IAuthService _authService;

    public AppShell(
        TodayPage todayPage,
        PropertiesPage propertiesPage,
        ReservationsPage reservationsPage,
        AssistantPage assistantPage,
        SynchronizationPage synchronizationPage,
        UsersPage usersPage,
        IAuthService authService)
    {
        _authService = authService;
        InitializeComponent();
        TodayContent.Content = todayPage;
        PropertiesContent.Content = propertiesPage;
        ReservationsContent.Content = reservationsPage;
        AssistantContent.Content = assistantPage;
        SynchronizationContent.Content = synchronizationPage;
        UsersContent.Content = usersPage;
        SynchronizationContent.IsVisible = false;
        UsersContent.IsVisible = false;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var role = await _authService.GetRoleAsync();
        SynchronizationContent.IsVisible = role is UserRoles.Admin or UserRoles.Receptionist;
        UsersContent.IsVisible = role == UserRoles.Admin;
    }
}
