using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Testing;
using Respawn;
using StayHub.Api.Seed;
using Testcontainers.MsSql;
using Xunit;

namespace StayHub.Api.IntegrationTests.Infrastructure;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    public const string AdminUsername = "integration-admin";
    public const string AdminPassword = "StayHub_Test123!";

    private readonly MsSqlContainer _database = new MsSqlBuilder(
            "mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("StayHub_Integration123!")
        .Build();
    private StayHubWebApplicationFactory? _application;
    private SqlConnection? _connection;
    private Respawner? _respawner;

    public StubExternalReservationClient ExternalReservations { get; } = new();

    public IServiceProvider Services =>
        _application?.Services
        ?? throw new InvalidOperationException("The test application has not started.");

    public HttpClient CreateClient()
    {
        return _application?.CreateClient(
                   new WebApplicationFactoryClientOptions { AllowAutoRedirect = false })
               ?? throw new InvalidOperationException("The test application has not started.");
    }

    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync();
        _application = new StayHubWebApplicationFactory(
            _database.GetConnectionString(),
            ExternalReservations);

        using var client = CreateClient();
        using var response = await client.GetAsync("/health/ready");
        response.EnsureSuccessStatusCode();

        _connection = new SqlConnection(_database.GetConnectionString());
        await _connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(
            _connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                TablesToIgnore = ["__EFMigrationsHistory"]
            });
    }

    public async Task ResetDatabaseAsync()
    {
        if (_connection is null || _respawner is null || _application is null)
            throw new InvalidOperationException("The test fixture has not been initialized.");

        await _respawner.ResetAsync(_connection);
        ExternalReservations.Reservations = [];
        await DatabaseSeeder.SeedAsync(_application.Services);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
        if (_application is not null)
            await _application.DisposeAsync();
        await _database.DisposeAsync();
    }
}
