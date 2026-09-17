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
    private readonly ReservationSearchCriteria _criteria = new();
    private ReservationFormOptions? _filterOptions;
    private bool _filtersInitialized;
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
        BindingContext = this;
    }

    public ObservableCollection<ReservationOverview> Reservations { get; } = [];

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var role = await _authService.GetRoleAsync();
        AddReservationButton.IsVisible = role is UserRoles.Admin or UserRoles.Receptionist;

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

    private async void OnFiltersClicked(object? sender, EventArgs e)
    {
        if (FilterPanel.IsVisible)
        {
            FilterPanel.IsVisible = false;
            return;
        }

        try
        {
            _filterOptions ??= await _reservationFormService.GetOptionsAsync();
            if (!_filtersInitialized)
                InitializeFilters(_filterOptions);

            FilterPanel.IsVisible = true;
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

    private void InitializeFilters(ReservationFormOptions options)
    {
        StatusPicker.ItemsSource = new[] { "All statuses", "Confirmed", "Checked in", "Checked out", "Cancelled" };
        PropertyPicker.ItemsSource = new object[] { "All properties" }.Concat(options.Properties).ToList();
        SourcePicker.ItemsSource = new object[] { "All sources" }.Concat(options.Sources).ToList();
        SortPicker.ItemsSource = new[] { "Arrival", "Departure", "Guest", "Property", "Created" };

        SearchEntry.Text = _criteria.Search;
        StatusPicker.SelectedIndex = _criteria.Status.HasValue ? (int)_criteria.Status.Value : 0;
        PropertyPicker.SelectedItem = _criteria.PropertyId.HasValue
            ? options.Properties.FirstOrDefault(item => item.Id == _criteria.PropertyId)
            : PropertyPicker.ItemsSource.Cast<object>().First();
        SourcePicker.SelectedItem = _criteria.SourceId.HasValue
            ? options.Sources.FirstOrDefault(item => item.Id == _criteria.SourceId)
            : SourcePicker.ItemsSource.Cast<object>().First();
        SortPicker.SelectedIndex = (int)_criteria.SortBy;
        _filtersInitialized = true;
    }

    private async void OnApplyFiltersClicked(object? sender, EventArgs e)
    {
        _criteria.Search = string.IsNullOrWhiteSpace(SearchEntry.Text) ? null : SearchEntry.Text.Trim();
        _criteria.Status = StatusPicker.SelectedIndex > 0
            ? (StayHub.Contracts.Reservations.ReservationStatusContract)StatusPicker.SelectedIndex
            : null;
        _criteria.PropertyId = (PropertyPicker.SelectedItem as PropertyOption)?.Id;
        _criteria.SourceId = (SourcePicker.SelectedItem as SourceOption)?.Id;
        _criteria.SortBy = (StayHub.Contracts.Reservations.ReservationSortBy)Math.Max(0, SortPicker.SelectedIndex);
        _criteria.Page = 1;

        FilterErrorLabel.IsVisible = false;
        FilterPanel.IsVisible = false;
        SearchEntry.Unfocus();
        UpdateFilterButton();
        await LoadReservationsAsync();
    }

    private async void OnClearFiltersClicked(object? sender, EventArgs e)
    {
        SearchEntry.Text = string.Empty;
        StatusPicker.SelectedIndex = 0;
        PropertyPicker.SelectedIndex = 0;
        SourcePicker.SelectedIndex = 0;
        SortPicker.SelectedIndex = 0;

        _criteria.Search = null;
        _criteria.Status = null;
        _criteria.PropertyId = null;
        _criteria.SourceId = null;
        _criteria.ArrivalFrom = null;
        _criteria.ArrivalTo = null;
        _criteria.SortBy = StayHub.Contracts.Reservations.ReservationSortBy.ArrivalDate;
        _criteria.SortDirection = StayHub.Contracts.Reservations.SortDirection.Ascending;
        _criteria.Page = 1;

        FilterPanel.IsVisible = false;
        SearchEntry.Unfocus();
        UpdateFilterButton();
        await LoadReservationsAsync();
    }

    private void UpdateFilterButton()
    {
        var hasActiveFilters =
            !string.IsNullOrWhiteSpace(_criteria.Search) ||
            _criteria.Status.HasValue ||
            _criteria.PropertyId.HasValue ||
            _criteria.SourceId.HasValue;

        FilterToggleButton.Text = "⌕";
        FilterToggleButton.Opacity = hasActiveFilters ? 1 : 0.8;
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

    private async void OnReservationTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not ReservationOverview reservation)
            return;

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
