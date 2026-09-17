using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StayHub.Api.IntegrationTests.Infrastructure;
using StayHub.Contracts.Authentication;
using StayHub.Contracts.Reservations;
using StayHub.Contracts.Synchronization;
using StayHub.Contracts.Users;
using StayHub.Dal.Data;
using StayHub.Domain.Models;
using Xunit;

namespace StayHub.Api.IntegrationTests;

public sealed class StayHubApiTests(IntegrationTestFixture fixture)
    : IClassFixture<IntegrationTestFixture>
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        await fixture.ResetDatabaseAsync();
        using var client = fixture.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                IntegrationTestFixture.AdminUsername,
                IntegrationTestFixture.AdminPassword),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(
            TestContext.Current.CancellationToken);
        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));
        Assert.Equal(UserRoles.Admin, login.Role);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutJwt_ReturnsUnauthorized()
    {
        await fixture.ResetDatabaseAsync();
        using var client = fixture.CreateClient();

        var response = await client.GetAsync(
            "/api/properties",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteReservation_AsViewer_ReturnsForbidden()
    {
        await fixture.ResetDatabaseAsync();
        using var client = fixture.CreateClient();
        await AuthorizeAsync(client, IntegrationTestFixture.AdminUsername, IntegrationTestFixture.AdminPassword);

        const string viewerPassword = "Viewer_Test123!";
        var createUserResponse = await client.PostAsJsonAsync(
            "/api/users",
            new CreateUserRequest(
                "integration-viewer",
                "integration-viewer@stayhub.test",
                viewerPassword,
                UserRoles.Viewer),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, createUserResponse.StatusCode);

        await AuthorizeAsync(client, "integration-viewer", viewerPassword);
        var response = await client.DeleteAsync(
            "/api/reservations/999",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ReadinessHealthCheck_WithRealDatabase_ReturnsOk()
    {
        await fixture.ResetDatabaseAsync();
        using var client = fixture.CreateClient();

        var response = await client.GetAsync(
            "/health/ready",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Synchronization_WhenRepeated_DoesNotDuplicateReservation()
    {
        await fixture.ResetDatabaseAsync();
        var sourceId = await AddSynchronizationPrerequisitesAsync();
        fixture.ExternalReservations.Reservations =
        [
            new ExternalReservationResponse(
                "PMS-1001",
                "Integration Test Hotel",
                "Anna",
                "Tester",
                "anna.tester@stayhub.test",
                null,
                new DateOnly(2026, 10, 1),
                new DateOnly(2026, 10, 4),
                ReservationStatusContract.Confirmed)
        ];
        using var client = fixture.CreateClient();
        await AuthorizeAsync(client, IntegrationTestFixture.AdminUsername, IntegrationTestFixture.AdminPassword);

        var firstResponse = await client.PostAsync(
            $"/api/synchronization/sources/{sourceId}",
            null,
            TestContext.Current.CancellationToken);
        var secondResponse = await client.PostAsync(
            $"/api/synchronization/sources/{sourceId}",
            null,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        var firstRun = await firstResponse.Content.ReadFromJsonAsync<SynchronizationRunResponse>(
            TestContext.Current.CancellationToken);
        var secondRun = await secondResponse.Content.ReadFromJsonAsync<SynchronizationRunResponse>(
            TestContext.Current.CancellationToken);
        Assert.NotNull(firstRun);
        Assert.NotNull(secondRun);
        Assert.Equal(1, firstRun.CreatedCount);
        Assert.Equal(1, secondRun.UnchangedCount);

        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StayHubDbContext>();
        var reservationCount = await db.Reservations.CountAsync(
            reservation => reservation.SourceId == sourceId
                           && reservation.ExternalId == "PMS-1001",
            TestContext.Current.CancellationToken);
        Assert.Equal(1, reservationCount);
    }

    private async Task<int> AddSynchronizationPrerequisitesAsync()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StayHubDbContext>();
        var createdAt = new DateTime(2026, 9, 17, 12, 0, 0, DateTimeKind.Utc);
        var source = new Source
        {
            Name = "Integration PMS",
            SourceType = "PMS",
            Url = "https://integration-pms.test",
            IsEnabled = true,
            CreatedAtUtc = createdAt
        };
        db.Sources.Add(source);
        db.Properties.Add(new Property
        {
            Name = "Integration Test Hotel",
            City = "Munich",
            CountryCode = "DE",
            CreatedAtUtc = createdAt
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return source.Id;
    }

    private static async Task AuthorizeAsync(
        HttpClient client,
        string username,
        string password)
    {
        client.DefaultRequestHeaders.Authorization = null;
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(username, password),
            TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(
                        TestContext.Current.CancellationToken)
                    ?? throw new InvalidOperationException("The login response was empty.");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.AccessToken);
    }
}
