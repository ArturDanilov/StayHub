using System.Collections.ObjectModel;
using StayHub.Mobile.Models;
using StayHub.Mobile.Services;

namespace StayHub.Mobile.Views;

public partial class UsersPage : ContentPage
{
    private readonly IUsersService _usersService;
    private readonly IAuthService _authService;
    private readonly IAppNavigator _navigator;
    private IReadOnlyList<UserOverview> _allUsers = [];
    private bool _hasLoaded;

    public UsersPage(IUsersService usersService, IAuthService authService, IAppNavigator navigator)
    {
        _usersService = usersService;
        _authService = authService;
        _navigator = navigator;
        InitializeComponent();
        BindingContext = this;
    }

    public ObservableCollection<UserOverview> Users { get; } = [];

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_hasLoaded)
            await LoadAsync();
    }

    private async Task LoadAsync()
    {
        SetBusy(true);
        try
        {
            _allUsers = await _usersService.GetAllAsync();
            _hasLoaded = true;
            ApplyFilter();
        }
        catch (UnauthorizedAccessException)
        {
            await _authService.LogoutAsync();
            _navigator.ShowLogin();
        }
        catch (ApiException exception)
        {
            ErrorLabel.Text = exception.Message;
            ErrorPanel.IsVisible = true;
        }
        finally
        {
            SetBusy(false);
            UsersRefreshView.IsRefreshing = false;
        }
    }

    private void ApplyFilter()
    {
        var search = UserSearchBar.Text?.Trim();
        var filtered = string.IsNullOrWhiteSpace(search)
            ? _allUsers
            : _allUsers.Where(user =>
                user.Username.Contains(search, StringComparison.OrdinalIgnoreCase)
                || user.Email.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

        Users.Clear();
        foreach (var user in filtered)
            Users.Add(user);

        SummaryLabel.Text = $"{Users.Count} of {_allUsers.Count} users";
        EmptyLabel.IsVisible = _hasLoaded && Users.Count == 0;
    }

    private void SetBusy(bool isBusy)
    {
        LoadingIndicator.IsVisible = isBusy;
        LoadingIndicator.IsRunning = isBusy;
        UsersCollection.IsVisible = !isBusy;
        if (isBusy)
        {
            ErrorPanel.IsVisible = false;
            EmptyLabel.IsVisible = false;
        }
    }

    private async Task OpenEditorAsync(UserOverview? user)
    {
        await Navigation.PushAsync(new UserEditorPage(
            user,
            _usersService,
            _authService,
            _navigator,
            async () =>
        {
            _hasLoaded = false;
            await LoadAsync();
        }));
    }

    private async void OnAddClicked(object? sender, EventArgs e) => await OpenEditorAsync(null);
    private async void OnRefreshing(object? sender, EventArgs e) => await LoadAsync();
    private async void OnRetryClicked(object? sender, EventArgs e) => await LoadAsync();
    private void OnSearchChanged(object? sender, TextChangedEventArgs e) => ApplyFilter();

    private async void OnUserSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not UserOverview user)
            return;

        UsersCollection.SelectedItem = null;
        await OpenEditorAsync(user);
    }
}
