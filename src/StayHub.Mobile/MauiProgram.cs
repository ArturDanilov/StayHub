using Microsoft.Extensions.Logging;
using StayHub.Mobile.Configuration;
using StayHub.Mobile.Services;
using StayHub.Mobile.Views;

namespace StayHub.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var apiSettings = ApiSettings.Load();
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton(apiSettings);
        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri(apiSettings.BaseAddress),
            Timeout = TimeSpan.FromSeconds(15)
        });
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IPropertiesService, PropertiesService>();
        builder.Services.AddSingleton<IReservationsService, ReservationsService>();
        builder.Services.AddSingleton<IReservationFormService, ReservationFormService>();
        builder.Services.AddSingleton<IUsersService, UsersService>();
        builder.Services.AddSingleton<IAppNavigator, AppNavigator>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<AppShell>();
        builder.Services.AddTransient<PropertiesPage>();
        builder.Services.AddTransient<ReservationsPage>();
        builder.Services.AddTransient<AssistantPage>();
        builder.Services.AddTransient<UsersPage>();

        return builder.Build();
    }
}
