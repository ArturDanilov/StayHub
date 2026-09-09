using StayHub.Contracts.Guests;
using StayHub.Mobile.Models;
using StayHub.Mobile.Services;

namespace StayHub.Mobile.Views;

public partial class CreateGuestPage : ContentPage
{
    private readonly IReservationFormService _formService;
    private readonly Func<GuestOption, Task> _onCreated;

    public CreateGuestPage(IReservationFormService formService, Func<GuestOption, Task> onCreated)
    {
        _formService = formService;
        _onCreated = onCreated;
        InitializeComponent();
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var firstName = FirstNameEntry.Text?.Trim();
        var lastName = LastNameEntry.Text?.Trim();
        var email = EmailEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
        {
            ShowError("First name, last name and email are required.");
            return;
        }

        SetBusy(true);
        try
        {
            var guest = await _formService.CreateGuestAsync(new CreateGuestRequest(
                firstName, lastName, email, string.IsNullOrWhiteSpace(PhoneEntry.Text) ? null : PhoneEntry.Text.Trim()));
            await _onCreated(guest);
            await Navigation.PopAsync();
        }
        catch (ApiException exception)
        {
            ShowError(exception.Message);
        }
        catch (UnauthorizedAccessException)
        {
            ShowError("Your session has expired. Return to the login screen and sign in again.");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        LoadingIndicator.IsVisible = busy;
        LoadingIndicator.IsRunning = busy;
        SaveButton.IsEnabled = !busy;
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}
