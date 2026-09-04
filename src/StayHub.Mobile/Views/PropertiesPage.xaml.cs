using StayHub.Mobile.Data;
using StayHub.Mobile.Models;

namespace StayHub.Mobile.Views;

public partial class PropertiesPage : ContentPage
{
    public PropertiesPage()
    {
        InitializeComponent();
        Properties = DemoData.Properties;
        BindingContext = this;
    }

    public IReadOnlyList<PropertyOverview> Properties { get; }
}
