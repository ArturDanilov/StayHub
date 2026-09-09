using StayHub.Contracts.Reservations;
using StayHub.Mobile.Models;
using StayHub.Mobile.Services;

namespace StayHub.Mobile.Views;

public partial class ReservationDetailPage : ContentPage
{
    private readonly ReservationOverview _reservation;
    private readonly IReservationsService _reservationsService;
    private readonly IAuthService _authService;
    private readonly IAppNavigator _navigator;
    private readonly IReservationFormService _formService;
    private readonly Func<Task> _onUpdated;

    public ReservationDetailPage(
        ReservationOverview reservation,
        string? role,
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
        PopulateDetails();
        ConfigureStatusUpdate(role);
        ActionsPanel.IsVisible = role is "Admin" or "Receptionist";
        DeleteButton.IsVisible = role == "Admin";
    }

    private async void OnEditClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new EditReservationPage(
            _reservation,
            _reservationsService,
            _formService,
            _authService,
            _navigator,
            _onUpdated));
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        var confirmed = await DisplayAlertAsync(
            "Delete reservation?",
            $"Reservation {_reservation.ExternalId} will be permanently deleted.",
            "Delete",
            "Cancel");
        if (!confirmed)
            return;

        SetBusy(true);
        ErrorLabel.IsVisible = false;
        try
        {
            await _reservationsService.DeleteAsync(_reservation.Id);
            await _onUpdated();
            await Navigation.PopAsync();
        }
        catch (UnauthorizedAccessException)
        {
            await _authService.LogoutAsync();
            _navigator.ShowLogin();
        }
        catch (ApiException exception)
        {
            ErrorLabel.Text = exception.Message;
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void PopulateDetails()
    {
        GuestNameLabel.Text = _reservation.GuestName;
        ExternalIdLabel.Text = $"External ID: {_reservation.ExternalId}";
        StatusLabel.Text = _reservation.StatusLabel;
        StatusBadge.BackgroundColor = Color.FromArgb(_reservation.StatusColor);
        PropertyLabel.Text = _reservation.PropertyName;
        DatesLabel.Text = $"{_reservation.ArrivalDate:dd MMM yyyy} → {_reservation.DepartureDate:dd MMM yyyy}";
        SourceLabel.Text = _reservation.SourceName;
        EmailLabel.Text = _reservation.GuestEmail;
        PhoneLabel.Text = string.IsNullOrWhiteSpace(_reservation.GuestPhone) ? "—" : _reservation.GuestPhone;
    }

    private void ConfigureStatusUpdate(string? role)
    {
        var canManage = role is "Admin" or "Receptionist";
        var transitions = GetAllowedTransitions(_reservation.Status);

        foreach (var status in transitions)
            NextStatusPicker.Items.Add(ToLabel(status));

        NextStatusPicker.SelectedIndex = transitions.Count > 0 ? 0 : -1;
        NextStatusPicker.IsEnabled = canManage && transitions.Count > 0;
        UpdateButton.IsVisible = canManage && transitions.Count > 0;

        if (!canManage)
        {
            PermissionLabel.Text = "Your role has read-only access.";
            PermissionLabel.IsVisible = true;
        }
        else if (transitions.Count == 0)
        {
            PermissionLabel.Text = "This reservation is in a final status.";
            PermissionLabel.IsVisible = true;
        }
    }

    private async void OnUpdateStatusClicked(object? sender, EventArgs e)
    {
        var transitions = GetAllowedTransitions(_reservation.Status);
        if (NextStatusPicker.SelectedIndex < 0 || NextStatusPicker.SelectedIndex >= transitions.Count)
            return;

        SetBusy(true);
        ErrorLabel.IsVisible = false;

        try
        {
            await _reservationsService.UpdateStatusAsync(
                _reservation.Id,
                transitions[NextStatusPicker.SelectedIndex]);
            await _onUpdated();
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

    private void SetBusy(bool isBusy)
    {
        UpdateButton.IsEnabled = !isBusy;
        NextStatusPicker.IsEnabled = !isBusy;
        LoadingIndicator.IsVisible = isBusy;
        LoadingIndicator.IsRunning = isBusy;
        EditButton.IsEnabled = !isBusy;
        DeleteButton.IsEnabled = !isBusy;
    }

    private static IReadOnlyList<ReservationStatusContract> GetAllowedTransitions(
        ReservationStatusContract currentStatus)
    {
        return currentStatus switch
        {
            ReservationStatusContract.Confirmed =>
                [ReservationStatusContract.CheckedIn, ReservationStatusContract.Cancelled],
            ReservationStatusContract.CheckedIn =>
                [ReservationStatusContract.CheckedOut],
            _ => []
        };
    }

    private static string ToLabel(ReservationStatusContract status)
    {
        return status switch
        {
            ReservationStatusContract.CheckedIn => "Checked in",
            ReservationStatusContract.CheckedOut => "Checked out",
            _ => status.ToString()
        };
    }
}
