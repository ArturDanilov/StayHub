using System.Collections.ObjectModel;
using StayHub.Mobile.Models;
using StayHub.Mobile.Services;

namespace StayHub.Mobile.Views;

public partial class SynchronizationPage : ContentPage
{
    private readonly ISynchronizationService _synchronizationService;
    private readonly IAuthService _authService;
    private readonly IAppNavigator _navigator;
    private bool _hasLoaded;

    public SynchronizationPage(
        ISynchronizationService synchronizationService,
        IAuthService authService,
        IAppNavigator navigator)
    {
        _synchronizationService = synchronizationService;
        _authService = authService;
        _navigator = navigator;
        InitializeComponent();
        BindingContext = this;
    }

    public ObservableCollection<SynchronizationRunOverview> Runs { get; } = [];

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_hasLoaded)
            await LoadAsync();
    }

    private async Task LoadAsync()
    {
        SetBusy(true);
        try
        {
            var selectedId = (SourcePicker.SelectedItem as SynchronizationSourceOption)?.Id;
            var sources = await _synchronizationService.GetSourcesAsync();
            SourcePicker.ItemsSource = sources.ToList();
            SourcePicker.SelectedItem = sources.FirstOrDefault(source => source.Id == selectedId)
                                        ?? sources.FirstOrDefault();

            await LoadRunsAsync();
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
            RunsRefreshView.IsRefreshing = false;
        }
    }

    private async Task LoadRunsAsync()
    {
        var runs = await _synchronizationService.GetRecentRunsAsync();
        Runs.Clear();
        foreach (var run in runs)
            Runs.Add(run);
        EmptyLabel.IsVisible = Runs.Count == 0;
    }

    private async void OnSyncClicked(object? sender, EventArgs e)
    {
        if (SourcePicker.SelectedItem is not SynchronizationSourceOption source)
        {
            await DisplayAlertAsync("Source required", "Choose a PMS source first.", "OK");
            return;
        }

        SyncButton.IsEnabled = false;
        SourcePicker.IsEnabled = false;
        SyncButton.Text = "Synchronizing…";
        SyncIndicator.IsVisible = true;
        SyncIndicator.IsRunning = true;
        ErrorPanel.IsVisible = false;
        try
        {
            var result = await _synchronizationService.SynchronizeAsync(source.Id);
            ShowResult(result);
            await LoadRunsAsync();
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
            SyncButton.IsEnabled = true;
            SourcePicker.IsEnabled = true;
            SyncButton.Text = "Sync now";
            SyncIndicator.IsRunning = false;
            SyncIndicator.IsVisible = false;
        }
    }

    private void ShowResult(SynchronizationRunOverview result)
    {
        ResultCard.BindingContext = result;
        ResultCard.IsVisible = true;
    }

    private void OnRunTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is SynchronizationRunOverview run)
            run.IsExpanded = !run.IsExpanded;
    }

    private async void OnViewReservationsClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//Reservations");

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorPanel.IsVisible = true;
    }

    private void SetBusy(bool isBusy)
    {
        LoadingIndicator.IsVisible = isBusy;
        LoadingIndicator.IsRunning = isBusy;
        RunsCollection.IsVisible = !isBusy;
        if (isBusy)
        {
            ErrorPanel.IsVisible = false;
            EmptyLabel.IsVisible = false;
        }
    }

    private async void OnRefreshing(object? sender, EventArgs e) => await LoadAsync();
    private async void OnRetryClicked(object? sender, EventArgs e) => await LoadAsync();
}
