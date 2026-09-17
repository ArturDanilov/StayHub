using StayHub.Mobile.Models;

namespace StayHub.Mobile.Views;

public partial class SynchronizationAlertsPage : ContentPage
{
    public SynchronizationAlertsPage(IReadOnlyList<SynchronizationAlertOverview> alerts)
    {
        Alerts = alerts;
        InitializeComponent();
        BindingContext = this;
    }

    public IReadOnlyList<SynchronizationAlertOverview> Alerts { get; }
}
