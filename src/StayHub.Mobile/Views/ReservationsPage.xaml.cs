using StayHub.Mobile.Data;
using StayHub.Mobile.Models;

namespace StayHub.Mobile.Views;

public partial class ReservationsPage : ContentPage
{
    public ReservationsPage()
    {
        InitializeComponent();
        Reservations = DemoData.Reservations;
        BindingContext = this;
    }

    public IReadOnlyList<ReservationOverview> Reservations { get; }
}
