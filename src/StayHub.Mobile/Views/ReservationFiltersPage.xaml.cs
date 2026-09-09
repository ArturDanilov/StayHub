using StayHub.Contracts.Reservations;
using StayHub.Mobile.Models;

namespace StayHub.Mobile.Views;

public partial class ReservationFiltersPage : ContentPage
{
    private readonly ReservationSearchCriteria _criteria;
    private readonly Func<Task> _onApply;

    public ReservationFiltersPage(
        ReservationSearchCriteria criteria,
        ReservationFormOptions options,
        Func<Task> onApply)
    {
        _criteria = criteria;
        _onApply = onApply;
        InitializeComponent();

        StatusPicker.ItemsSource = new[] { "All statuses", "Confirmed", "Checked in", "Checked out", "Cancelled" };
        PropertyPicker.ItemsSource = new object[] { "All properties" }.Concat(options.Properties.Cast<object>()).ToList();
        SourcePicker.ItemsSource = new object[] { "All sources" }.Concat(options.Sources.Cast<object>()).ToList();
        SortPicker.ItemsSource = new[] { "Arrival", "Departure", "Guest", "Property", "Created" };
        DirectionPicker.ItemsSource = new[] { "Ascending", "Descending" };
        PopulateSelections(options);
    }

    private void PopulateSelections(ReservationFormOptions options)
    {
        StatusPicker.SelectedIndex = _criteria.Status.HasValue ? (int)_criteria.Status.Value : 0;
        PropertyPicker.SelectedItem = _criteria.PropertyId.HasValue
            ? options.Properties.FirstOrDefault(item => item.Id == _criteria.PropertyId)
            : PropertyPicker.ItemsSource.Cast<object>().First();
        SourcePicker.SelectedItem = _criteria.SourceId.HasValue
            ? options.Sources.FirstOrDefault(item => item.Id == _criteria.SourceId)
            : SourcePicker.ItemsSource.Cast<object>().First();
        FromSwitch.IsToggled = _criteria.ArrivalFrom.HasValue;
        ToSwitch.IsToggled = _criteria.ArrivalTo.HasValue;
        FromDatePicker.Date = (_criteria.ArrivalFrom ?? DateOnly.FromDateTime(DateTime.Today)).ToDateTime(TimeOnly.MinValue);
        ToDatePicker.Date = (_criteria.ArrivalTo ?? DateOnly.FromDateTime(DateTime.Today.AddMonths(1))).ToDateTime(TimeOnly.MinValue);
        SortPicker.SelectedIndex = (int)_criteria.SortBy;
        DirectionPicker.SelectedIndex = (int)_criteria.SortDirection;
    }

    private async void OnApplyClicked(object? sender, EventArgs e)
    {
        DateOnly? from = FromSwitch.IsToggled && FromDatePicker.Date.HasValue
            ? DateOnly.FromDateTime(FromDatePicker.Date.Value)
            : null;
        DateOnly? to = ToSwitch.IsToggled && ToDatePicker.Date.HasValue
            ? DateOnly.FromDateTime(ToDatePicker.Date.Value)
            : null;
        if (from.HasValue && to.HasValue && from > to)
        {
            ErrorLabel.Text = "The start date must not be later than the end date.";
            ErrorLabel.IsVisible = true;
            return;
        }

        _criteria.Status = StatusPicker.SelectedIndex > 0
            ? (ReservationStatusContract)StatusPicker.SelectedIndex
            : null;
        _criteria.PropertyId = (PropertyPicker.SelectedItem as PropertyOption)?.Id;
        _criteria.SourceId = (SourcePicker.SelectedItem as SourceOption)?.Id;
        _criteria.ArrivalFrom = from;
        _criteria.ArrivalTo = to;
        _criteria.SortBy = (ReservationSortBy)Math.Max(0, SortPicker.SelectedIndex);
        _criteria.SortDirection = (SortDirection)Math.Max(0, DirectionPicker.SelectedIndex);
        _criteria.Page = 1;
        await _onApply();
        await Navigation.PopModalAsync();
    }

    private void OnResetClicked(object? sender, EventArgs e)
    {
        StatusPicker.SelectedIndex = 0;
        PropertyPicker.SelectedIndex = 0;
        SourcePicker.SelectedIndex = 0;
        FromSwitch.IsToggled = false;
        ToSwitch.IsToggled = false;
        SortPicker.SelectedIndex = 0;
        DirectionPicker.SelectedIndex = 0;
        ErrorLabel.IsVisible = false;
    }
}
