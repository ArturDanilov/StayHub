using System.Collections.ObjectModel;
using StayHub.Contracts.Users;
using StayHub.Mobile.Models;
using StayHub.Mobile.Services;

namespace StayHub.Mobile.Views;

public partial class ReservationsPage : ContentPage
{
    private readonly IReservationsService _reservationsService;
    private readonly IReservationFormService _reservationFormService;
    private readonly IAuthService _authService;
    private readonly IAppNavigator _navigator;
    private readonly ToolbarItem _addReservationItem;
    private readonly ReservationSearchCriteria _criteria = new();
    private ReservationFormOptions? _filterOptions;
    private CancellationTokenSource? _searchCancellation;
    private bool _hasLoaded;
    private int _totalPages;

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
        _addReservationItem = new ToolbarItem("Add", null, () => OnAddReservationClicked(null, EventArgs.Empty));
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

    private async Task LoadReservationsAsync(CancellationToken cancellationToken = default)
    {
        SetLoadingState(true);
        try
        {
            var result = await _reservationsService.GetAllAsync(_criteria, cancellationToken);
            Reservations.Clear();
            foreach (var reservation in result.Items)
                Reservations.Add(reservation);

            _criteria.Page = result.Page;
            _totalPages = result.TotalPages;
            _hasLoaded = true;

            CountLabel.Text = result.TotalCount.ToString();

            SummaryLabel.Text = result.TotalCount == 0
                ? "No bookings found"
                : $"Showing {(result.Page - 1) * result.PageSize + 1}–{(result.Page - 1) * result.PageSize + result.Items.Count} of {result.TotalCount}";

            PageLabel.Text = _totalPages == 0
                ? "Page 0"
                : $"{result.Page} / {_totalPages}";

            PaginationPanel.IsVisible = _totalPages > 1;

            PreviousButton.IsEnabled = result.Page > 1;
            NextButton.IsEnabled = result.Page < _totalPages;

            EmptyLabel.IsVisible = result.TotalCount == 0;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
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

    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        _searchCancellation?.Cancel();
        _searchCancellation?.Dispose();
        _searchCancellation = new CancellationTokenSource();
        var token = _searchCancellation.Token;
        try
        {
            await Task.Delay(400, token);
            _criteria.Search = e.NewTextValue;
            _criteria.Page = 1;
            await LoadReservationsAsync(token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
    }

    private async void OnFiltersClicked(object? sender, EventArgs e)
    {
        try
        {
            _filterOptions ??= await _reservationFormService.GetOptionsAsync();
            await Navigation.PushModalAsync(new NavigationPage(new ReservationFiltersPage(
                _criteria,
                _filterOptions,
                () => LoadReservationsAsync())));
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
    }

    private async void OnPreviousClicked(object? sender, EventArgs e)
    {
        if (_criteria.Page <= 1)
            return;
        _criteria.Page--;
        await LoadReservationsAsync();
    }

    private async void OnNextClicked(object? sender, EventArgs e)
    {
        if (_criteria.Page >= _totalPages)
            return;
        _criteria.Page++;
        await LoadReservationsAsync();
    }

    private async void OnAddReservationClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new CreateReservationPage(
            _reservationsService, _reservationFormService, _authService, _navigator, RefreshAfterChangeAsync));
    }

    private async Task RefreshAfterChangeAsync()
    {
        _criteria.Page = 1;
        await LoadReservationsAsync();
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

    private async void OnRefreshing(object? sender, EventArgs e) => await LoadReservationsAsync();
    private async void OnRetryClicked(object? sender, EventArgs e) => await LoadReservationsAsync();

    private async void OnReservationSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not ReservationOverview reservation)
            return;

        ReservationsCollection.SelectedItem = null;
        var role = await _authService.GetRoleAsync();
        await Navigation.PushAsync(new ReservationDetailPage(
            reservation,
            role,
            _reservationsService,
            _reservationFormService,
            _authService,
            _navigator,
            RefreshAfterChangeAsync));
    }
}
