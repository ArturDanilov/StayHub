using StayHub.Mobile.Models;
using StayHub.Mobile.Services;

namespace StayHub.Mobile.Views;

public partial class TodayPage : ContentPage
{
    private readonly IOperationsDashboardService _dashboardService;
    private readonly IReservationsService _reservationsService;
    private readonly IReservationFormService _reservationFormService;
    private readonly IAuthService _authService;
    private readonly IAppNavigator _navigator;
    private OperationsDashboardOverview? _dashboard;
    private bool _hasLoaded;

    public TodayPage(
        IOperationsDashboardService dashboardService,
        IReservationsService reservationsService,
        IReservationFormService reservationFormService,
        IAuthService authService,
        IAppNavigator navigator)
    {
        _dashboardService = dashboardService;
        _reservationsService = reservationsService;
        _reservationFormService = reservationFormService;
        _authService = authService;
        _navigator = navigator;
        InitializeComponent();
        BindingContext = this;
    }

    public OperationsDashboardOverview? Dashboard
    {
        get => _dashboard;
        private set
        {
            _dashboard = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSynchronizationAlerts));
            OnPropertyChanged(nameof(SynchronizationStatusTitle));
            OnPropertyChanged(nameof(SynchronizationStatusDetail));
            OnPropertyChanged(nameof(SynchronizationStatusColor));
        }
    }

    public bool HasSynchronizationAlerts => Dashboard?.SynchronizationAlerts.Count > 0;

    public string SynchronizationStatusTitle
    {
        get
        {
            var count = Dashboard?.SynchronizationAlerts.Count ?? 0;
            return count == 0
                ? "Synchronization healthy"
                : $"{count} synchronization {(count == 1 ? "issue" : "issues")}";
        }
    }

    public string SynchronizationStatusDetail
    {
        get
        {
            var latestAlert = Dashboard?.SynchronizationAlerts.FirstOrDefault();
            return latestAlert is null
                ? "No recent issues"
                : latestAlert.ErrorMessage ?? latestAlert.Summary;
        }
    }

    public Color SynchronizationStatusColor => HasSynchronizationAlerts
        ? Color.FromArgb("#B3261E")
        : Color.FromArgb("#2F7968");

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_hasLoaded)
            await LoadDashboardAsync();
    }

    private async Task LoadDashboardAsync()
    {
        SetLoadingState(true);
        try
        {
            Dashboard = await _dashboardService.GetAsync();
            _hasLoaded = true;
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
            DashboardRefreshView.IsRefreshing = false;
        }
    }

    private async void OnBookingTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not int reservationId)
            return;

        try
        {
            var reservation = await _reservationsService.GetByIdAsync(reservationId);
            if (reservation is null)
            {
                await DisplayAlertAsync("Booking unavailable", "This booking no longer exists.", "OK");
                await LoadDashboardAsync();
                return;
            }

            var role = await _authService.GetRoleAsync();
            await Navigation.PushAsync(new ReservationDetailPage(
                reservation,
                role,
                _reservationsService,
                _reservationFormService,
                _authService,
                _navigator,
                LoadDashboardAsync));
        }
        catch (UnauthorizedAccessException)
        {
            await _authService.LogoutAsync();
            _navigator.ShowLogin();
        }
        catch (ApiException exception)
        {
            await DisplayAlertAsync("Could not open booking", exception.Message, "OK");
        }
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        DashboardRefreshView.IsVisible = !isLoading;
        if (isLoading)
            ErrorPanel.IsVisible = false;
    }

    private async void OnRefreshing(object? sender, EventArgs e) => await LoadDashboardAsync();
    private async void OnRetryClicked(object? sender, EventArgs e) => await LoadDashboardAsync();

    private async void OnSynchronizationStatusTapped(object? sender, TappedEventArgs e)
    {
        if (!HasSynchronizationAlerts || Dashboard is null)
            return;

        await Navigation.PushAsync(new SynchronizationAlertsPage(Dashboard.SynchronizationAlerts));
    }
}
