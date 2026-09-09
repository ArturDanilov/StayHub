using StayHub.Contracts.Reservations;
using StayHub.Mobile.Models;
using StayHub.Mobile.Services;

namespace StayHub.Mobile.Views;

public partial class CreateReservationPage : ContentPage
{
    private readonly IReservationsService _reservationsService;
    private readonly IReservationFormService _formService;
    private readonly IAuthService _authService;
    private readonly IAppNavigator _navigator;
    private readonly Func<Task> _onCreated;
    private bool _hasLoaded;

    public CreateReservationPage(
        IReservationsService reservationsService,
        IReservationFormService formService,
        IAuthService authService,
        IAppNavigator navigator,
        Func<Task> onCreated)
    {
        _reservationsService = reservationsService;
        _formService = formService;
        _authService = authService;
        _navigator = navigator;
        _onCreated = onCreated;
        InitializeComponent();

        ArrivalDatePicker.Date = DateTime.Today.AddDays(1);
        DepartureDatePicker.Date = DateTime.Today.AddDays(2);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_hasLoaded)
            await LoadOptionsAsync();
    }

    private async Task LoadOptionsAsync()
    {
        SetBusy(true);
        try
        {
            var options = await _formService.GetOptionsAsync();
            PropertyPicker.ItemsSource = options.Properties.ToList();
            GuestPicker.ItemsSource = options.Guests.ToList();
            SourcePicker.ItemsSource = options.Sources.ToList();
            _hasLoaded = true;
        }
        catch (UnauthorizedAccessException)
        {
            await _authService.LogoutAsync();
            _navigator.ShowLogin();
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

    private async void OnNewGuestClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new CreateGuestPage(
            _formService,
            async guest =>
            {
                var guests = (GuestPicker.ItemsSource as IEnumerable<GuestOption> ?? [])
                    .Append(guest)
                    .OrderBy(item => item.Name)
                    .ToList();
                GuestPicker.ItemsSource = guests;
                GuestPicker.SelectedItem = guest;
                await Task.CompletedTask;
            }));
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        var externalId = ExternalIdEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(externalId))
        {
            ShowError("External ID is required.");
            return;
        }

        if (PropertyPicker.SelectedItem is not PropertyOption property
            || GuestPicker.SelectedItem is not GuestOption guest
            || SourcePicker.SelectedItem is not SourceOption source)
        {
            ShowError("Select a property, guest and source.");
            return;
        }

        var arrivalDate = ArrivalDatePicker.Date ?? DateTime.Today.AddDays(1);
        var departureDate = DepartureDatePicker.Date ?? arrivalDate.AddDays(1);

        if (departureDate <= arrivalDate)
        {
            ShowError("Departure date must be later than arrival date.");
            return;
        }

        SetBusy(true);
        try
        {
            await _reservationsService.CreateAsync(new CreateReservationRequest(
                externalId,
                source.Id,
                DateOnly.FromDateTime(arrivalDate),
                DateOnly.FromDateTime(departureDate),
                property.Id,
                guest.Id));
            await _onCreated();
            await Navigation.PopAsync();
        }
        catch (UnauthorizedAccessException)
        {
            await _authService.LogoutAsync();
            _navigator.ShowLogin();
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

    private void OnArrivalDateSelected(object? sender, DateChangedEventArgs e)
    {
        if (e.NewDate is { } arrivalDate
            && (DepartureDatePicker.Date is null || DepartureDatePicker.Date <= arrivalDate))
            DepartureDatePicker.Date = arrivalDate.AddDays(1);
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
