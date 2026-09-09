using StayHub.Mobile.Models;
using StayHub.Mobile.Services;

namespace StayHub.Mobile.Views;

public partial class UserEditorPage : ContentPage
{
    private readonly UserOverview? _user;
    private readonly IUsersService _usersService;
    private readonly IAuthService _authService;
    private readonly IAppNavigator _navigator;
    private readonly Func<Task> _onSaved;

    public UserEditorPage(
        UserOverview? user,
        IUsersService usersService,
        IAuthService authService,
        IAppNavigator navigator,
        Func<Task> onSaved)
    {
        _user = user;
        _usersService = usersService;
        _authService = authService;
        _navigator = navigator;
        _onSaved = onSaved;
        InitializeComponent();
        UsernameEntry.Keyboard = Keyboard.Create(KeyboardFlags.None);
        PasswordEntry.Keyboard = Keyboard.Create(KeyboardFlags.None);
        ConfigureForm();
    }

    private void ConfigureForm()
    {
        if (_user is null)
        {
            RolePicker.SelectedIndex = 2;
            return;
        }

        TitleLabel.Text = _user.Username;
        SubtitleLabel.Text = "Manage role, access and password";
        SaveButton.Text = "Save changes";
        UsernameEntry.Text = _user.Username;
        UsernameEntry.IsEnabled = false;
        EmailEntry.Text = _user.Email;
        EmailEntry.IsEnabled = false;
        RolePicker.SelectedItem = _user.Role;
        ActiveRow.IsVisible = true;
        ActiveSwitch.IsToggled = _user.IsActive;
        PasswordHint.IsVisible = true;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        if (!ValidateForm())
            return;

        SetBusy(true);
        try
        {
            var role = (string)RolePicker.SelectedItem;
            if (_user is null)
            {
                await _usersService.CreateAsync(
                    UsernameEntry.Text!.Trim(),
                    EmailEntry.Text!.Trim(),
                    PasswordEntry.Text!,
                    role);
            }
            else
            {
                if (_user.Role != role)
                    await _usersService.UpdateRoleAsync(_user.Id, role);
                if (_user.IsActive != ActiveSwitch.IsToggled)
                    await _usersService.UpdateStatusAsync(_user.Id, ActiveSwitch.IsToggled);
                if (!string.IsNullOrWhiteSpace(PasswordEntry.Text))
                    await _usersService.ResetPasswordAsync(_user.Id, PasswordEntry.Text);
            }

            await _onSaved();
            await Navigation.PopAsync();
        }
        catch (ApiException exception)
        {
            ErrorLabel.Text = exception.Message;
            ErrorLabel.IsVisible = true;
        }
        catch (UnauthorizedAccessException)
        {
            await _authService.LogoutAsync();
            _navigator.ShowLogin();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private bool ValidateForm()
    {
        if (_user is null
            && (string.IsNullOrWhiteSpace(UsernameEntry.Text)
                || UsernameEntry.Text.Trim().Length < 3
                || string.IsNullOrWhiteSpace(EmailEntry.Text)))
        {
            ShowError("Enter a username of at least 3 characters and a valid email.");
            return false;
        }

        if (RolePicker.SelectedItem is not string)
        {
            ShowError("Choose a role.");
            return false;
        }

        if ((_user is null || !string.IsNullOrEmpty(PasswordEntry.Text))
            && (PasswordEntry.Text?.Length ?? 0) < 8)
        {
            ShowError("Password must contain at least 8 characters.");
            return false;
        }

        return true;
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void SetBusy(bool isBusy)
    {
        SaveButton.IsEnabled = !isBusy;
        LoadingIndicator.IsVisible = isBusy;
        LoadingIndicator.IsRunning = isBusy;
    }
}
