using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StayHub.Business.Interfaces;

namespace StayHub.Api.IntegrationTests.Infrastructure;

internal sealed class StayHubWebApplicationFactory(
    string connectionString,
    StubExternalReservationClient externalReservationClient)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:StayHubDb", connectionString);
        builder.UseSetting("DatabaseInitialization:ApplyMigrations", "true");
        builder.UseSetting("SeedData:DemoDataEnabled", "false");
        builder.UseSetting("AutomaticSynchronization:Enabled", "false");
        builder.UseSetting("Jwt:Issuer", "StayHub.Api.IntegrationTests");
        builder.UseSetting("Jwt:Audience", "StayHub.Api.IntegrationTests.Client");
        builder.UseSetting("Jwt:Key", "StayHub-Integration-Test-Key-At-Least-32-Characters!");
        builder.UseSetting("Jwt:ExpirationMinutes", "15");
        builder.UseSetting("SeedAdmin:Username", IntegrationTestFixture.AdminUsername);
        builder.UseSetting("SeedAdmin:Email", "integration-admin@stayhub.test");
        builder.UseSetting("SeedAdmin:Password", IntegrationTestFixture.AdminPassword);
        builder.UseSetting("Logging:LogLevel:Default", "Warning");
        builder.UseSetting("Logging:LogLevel:Microsoft.EntityFrameworkCore", "Warning");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IExternalReservationClient>();
            services.AddSingleton<IExternalReservationClient>(externalReservationClient);
        });
    }
}
