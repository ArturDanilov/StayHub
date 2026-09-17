using StayHub.Contracts.Users;
using StayHub.Mobile.Services;

namespace StayHub.Mobile.Views;

public partial class AccountPage : ContentPage
{
    private readonly IAuthService _authService;
    private readonly IAppNavigator _navigator;
    private readonly SynchronizationPage _synchronizationPage;
    private readonly UsersPage _usersPage;

    public AccountPage(
        IAuthService authService,
        IAppNavigator navigator,
        SynchronizationPage synchronizationPage,
        UsersPage usersPage)
    {
        _authService = authService;
        _navigator = navigator;
        _synchronizationPage = synchronizationPage;
        _usersPage = usersPage;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var role = await _authService.GetRoleAsync();
        RoleLabel.Text = string.IsNullOrWhiteSpace(role) ? "Signed in" : role;

        UsersRow.IsVisible = role == UserRoles.Admin;
        SynchronizationRow.IsVisible = role is UserRoles.Admin or UserRoles.Receptionist;
        ManagementSection.IsVisible = UsersRow.IsVisible || SynchronizationRow.IsVisible;
    }

    private async void OnUsersTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(_usersPage);
    }

    private async void OnSynchronizationTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(_synchronizationPage);
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        await _authService.LogoutAsync();
        _navigator.ShowLogin();
    }
}
