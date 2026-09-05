using System.Collections.ObjectModel;
using StayHub.Mobile.Models;
using StayHub.Mobile.Services;

namespace StayHub.Mobile.Views;

public partial class PropertiesPage : ContentPage
{
    private readonly IPropertiesService _propertiesService;
    private readonly IAuthService _authService;
    private readonly IAppNavigator _navigator;
    private bool _hasLoaded;

    public PropertiesPage(
        IPropertiesService propertiesService,
        IAuthService authService,
        IAppNavigator navigator)
    {
        _propertiesService = propertiesService;
        _authService = authService;
        _navigator = navigator;
        InitializeComponent();
        BindingContext = this;
    }

    public ObservableCollection<PropertyOverview> Properties { get; } = [];

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_hasLoaded)
            return;

        await LoadPropertiesAsync();
    }

    private async Task LoadPropertiesAsync()
    {
        SetLoadingState(true);

        try
        {
            var properties = await _propertiesService.GetAllAsync();

            Properties.Clear();
            foreach (var property in properties)
                Properties.Add(property);

            _hasLoaded = true;
            PropertyCountLabel.Text = Properties.Count.ToString();
            EmptyLabel.IsVisible = Properties.Count == 0;
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
            SetLoadingState(false);
            PropertiesRefreshView.IsRefreshing = false;
        }
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        PropertiesCollection.IsVisible = !isLoading;

        if (isLoading)
        {
            ErrorPanel.IsVisible = false;
            EmptyLabel.IsVisible = false;
        }
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadPropertiesAsync();
    }

    private async void OnRetryClicked(object? sender, EventArgs e)
    {
        await LoadPropertiesAsync();
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        await _authService.LogoutAsync();
        _navigator.ShowLogin();
    }
}
