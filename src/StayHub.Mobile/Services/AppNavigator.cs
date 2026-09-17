using StayHub.Mobile.Views;

namespace StayHub.Mobile.Services;

public sealed class AppNavigator(IServiceProvider services) : IAppNavigator
{
    public void ShowMain()
    {
        ReplaceRoot(services.GetRequiredService<AppShell>());
    }

    public void ShowLogin()
    {
        ReplaceRoot(services.GetRequiredService<LoginPage>());
    }

    private static void ReplaceRoot(Page page)
    {
        var window = Application.Current?.Windows.FirstOrDefault()
                     ?? throw new InvalidOperationException("The application window is not available.");

        window.Page = page;
    }
}
