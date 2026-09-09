using System.Collections.ObjectModel;
using StayHub.Mobile.Models;
using StayHub.Mobile.Services;
using StayHub.Contracts.Users;

namespace StayHub.Mobile.Views;

public partial class ReservationsPage : ContentPage
{
    private readonly IReservationsService _reservationsService;
    private readonly IAuthService _authService;
    private readonly IAppNavigator _navigator;
    private readonly IReservationFormService _reservationFormService;
    private readonly ToolbarItem _addReservationItem;
    private IReadOnlyList<ReservationOverview> _allReservations = [];
    private bool _hasLoaded;

    public ReservationsPage(
        IReservationsService reservationsService,
        IReservationFormService reservationFormService,
        IAuthService authService,
        IAppNavigator navigator)
    {
        _reservationsService = reservationsService;
        _reservationFormService = reservationFormService;
        _authService = authService;
        _navigator = navigator;
        InitializeComponent();
        _addReservationItem = new ToolbarItem(
            "Add",
            null,
            () => OnAddReservationClicked(null, EventArgs.Empty));
        StatusPicker.SelectedIndex = 0;
        BindingContext = this;
    }

    public ObservableCollection<ReservationOverview> Reservations { get; } = [];

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var role = await _authService.GetRoleAsync();
        var canCreate = role is UserRoles.Admin or UserRoles.Receptionist;
        if (canCreate && !ToolbarItems.Contains(_addReservationItem))
            ToolbarItems.Add(_addReservationItem);
        else if (!canCreate && ToolbarItems.Contains(_addReservationItem))
            ToolbarItems.Remove(_addReservationItem);

        if (!_hasLoaded)
            await LoadReservationsAsync();
    }

    private async void OnAddReservationClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new CreateReservationPage(
            _reservationsService,
            _reservationFormService,
            _authService,
            _navigator,
            async () =>
            {
                _hasLoaded = false;
                await LoadReservationsAsync();
            }));
    }

    private async Task LoadReservationsAsync()
    {
        SetLoadingState(true);

        try
        {
            _allReservations = await _reservationsService.GetAllAsync();
            _hasLoaded = true;
            ApplyFilters();
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
            ReservationsRefreshView.IsRefreshing = false;
        }
    }

    private void ApplyFilters()
    {
        var search = ReservationSearchBar.Text?.Trim();
        var status = StatusPicker.SelectedItem as string;

        var filtered = _allReservations.Where(reservation =>
            MatchesSearch(reservation, search)
            && (status is null || status == "All statuses" || reservation.StatusLabel == status));

        Reservations.Clear();
        foreach (var reservation in filtered)
            Reservations.Add(reservation);

        CountLabel.Text = Reservations.Count.ToString();
        SummaryLabel.Text = $"{Reservations.Count} of {_allReservations.Count} reservations";
        EmptyLabel.IsVisible = _hasLoaded && Reservations.Count == 0;
    }

    private static bool MatchesSearch(ReservationOverview reservation, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return true;

        return reservation.GuestName.Contains(search, StringComparison.OrdinalIgnoreCase)
               || reservation.PropertyName.Contains(search, StringComparison.OrdinalIgnoreCase)
               || reservation.SourceName.Contains(search, StringComparison.OrdinalIgnoreCase)
               || reservation.ExternalId.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        ReservationsCollection.IsVisible = !isLoading;

        if (isLoading)
        {
            ErrorPanel.IsVisible = false;
            EmptyLabel.IsVisible = false;
        }
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadReservationsAsync();
    }

    private async void OnRetryClicked(object? sender, EventArgs e)
    {
        await LoadReservationsAsync();
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void OnStatusChanged(object? sender, EventArgs e)
    {
        ApplyFilters();
    }

    private async void OnReservationSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not ReservationOverview reservation)
            return;

        ReservationsCollection.SelectedItem = null;
        var role = await _authService.GetRoleAsync();
        var detailsPage = new ReservationDetailPage(
            reservation,
            role,
            _reservationsService,
            _authService,
            _navigator,
            async () =>
            {
                _hasLoaded = false;
                await LoadReservationsAsync();
            });

        await Navigation.PushAsync(detailsPage);
    }
}
