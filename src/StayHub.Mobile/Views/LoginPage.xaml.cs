using StayHub.Mobile.Services;

namespace StayHub.Mobile.Views;

public partial class LoginPage : ContentPage
{
    private readonly IAuthService _authService;
    private readonly IAppNavigator _navigator;
    private bool _sessionChecked;

    public LoginPage(IAuthService authService, IAppNavigator navigator)
    {
        _authService = authService;
        _navigator = navigator;
        InitializeComponent();
        UsernameEntry.Keyboard = Keyboard.Create(KeyboardFlags.None);
        PasswordEntry.Keyboard = Keyboard.Create(KeyboardFlags.None);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_sessionChecked)
            return;

        _sessionChecked = true;
        SetBusy(true);

        try
        {
            if (await _authService.HasValidSessionAsync())
                _navigator.ShowMain();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task LoginAsync()
    {
        ErrorLabel.IsVisible = false;

        if (string.IsNullOrWhiteSpace(UsernameEntry.Text)
            || string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            ShowError("Enter username and password.");
            return;
        }

        SetBusy(true);

        try
        {
            await _authService.LoginAsync(
                UsernameEntry.Text.Trim(),
                PasswordEntry.Text);

            PasswordEntry.Text = string.Empty;
            _navigator.ShowMain();
        }
        catch (ApiException exception)
        {
            ShowError(exception.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool isBusy)
    {
        LoginButton.IsEnabled = !isBusy;
        UsernameEntry.IsEnabled = !isBusy;
        PasswordEntry.IsEnabled = !isBusy;
        LoadingIndicator.IsVisible = isBusy;
        LoadingIndicator.IsRunning = isBusy;
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        await LoginAsync();
    }

    private void OnUsernameCompleted(object? sender, EventArgs e)
    {
        PasswordEntry.Focus();
    }

    private async void OnPasswordCompleted(object? sender, EventArgs e)
    {
        await LoginAsync();
    }
}
