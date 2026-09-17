using StayHub.Contracts.Reservations;
using StayHub.Mobile.Models;
using StayHub.Mobile.Services;

namespace StayHub.Mobile.Views;

public partial class EditReservationPage : ContentPage
{
    private readonly ReservationOverview _reservation;
    private readonly IReservationsService _reservationsService;
    private readonly IReservationFormService _formService;
    private readonly IAuthService _authService;
    private readonly IAppNavigator _navigator;
    private readonly Func<Task> _onUpdated;
    private bool _hasLoaded;

    public EditReservationPage(
        ReservationOverview reservation,
        IReservationsService reservationsService,
        IReservationFormService formService,
        IAuthService authService,
        IAppNavigator navigator,
        Func<Task> onUpdated)
    {
        _reservation = reservation;
        _reservationsService = reservationsService;
        _formService = formService;
        _authService = authService;
        _navigator = navigator;
        _onUpdated = onUpdated;
        InitializeComponent();

        ReservationCaption.Text = $"Booking #{reservation.Id}";
        ExternalIdEntry.Text = reservation.ExternalId;
        ArrivalDatePicker.Date = reservation.ArrivalDate.ToDateTime(TimeOnly.MinValue);
        DepartureDatePicker.Date = reservation.DepartureDate.ToDateTime(TimeOnly.MinValue);
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
            PropertyPicker.SelectedItem = options.Properties.FirstOrDefault(item => item.Id == _reservation.PropertyId);
            GuestPicker.SelectedItem = options.Guests.FirstOrDefault(item => item.Id == _reservation.GuestId);
            SourcePicker.SelectedItem = options.Sources.FirstOrDefault(item => item.Id == _reservation.SourceId);
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

        var arrival = ArrivalDatePicker.Date ?? DateTime.Today;
        var departure = DepartureDatePicker.Date ?? arrival.AddDays(1);
        if (departure <= arrival)
        {
            ShowError("Departure date must be later than arrival date.");
            return;
        }

        SetBusy(true);
        try
        {
            await _reservationsService.UpdateAsync(_reservation.Id, new UpdateReservationRequest(
                externalId,
                source.Id,
                DateOnly.FromDateTime(arrival),
                DateOnly.FromDateTime(departure),
                property.Id,
                guest.Id));
            await _onUpdated();
            await Navigation.PopToRootAsync();
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
        if (e.NewDate is { } arrival && (DepartureDatePicker.Date is null || DepartureDatePicker.Date <= arrival))
            DepartureDatePicker.Date = arrival.AddDays(1);
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
