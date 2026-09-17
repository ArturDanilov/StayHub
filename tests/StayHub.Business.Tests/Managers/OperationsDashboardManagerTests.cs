using StayHub.Business.Interfaces;
using StayHub.Business.Managers;
using StayHub.Business.Models;
using Xunit;

namespace StayHub.Business.Tests.Managers;

public sealed class OperationsDashboardManagerTests
{
    [Fact]
    public async Task GetAsync_UsesBerlinBusinessDateAndConfiguredPreviewWindow()
    {
        var repository = new DashboardRepositoryStub();
        var timeProvider = new FixedTimeProvider(
            new DateTimeOffset(2026, 1, 14, 23, 30, 0, TimeSpan.Zero));
        var manager = new OperationsDashboardManager(repository, timeProvider);

        var response = await manager.GetAsync(TestContext.Current.CancellationToken);

        Assert.Equal(new DateOnly(2026, 1, 15), response.BusinessDate);
        Assert.Equal(response.BusinessDate, repository.BusinessDate);
        Assert.Equal(new DateOnly(2026, 1, 22), repository.UpcomingThrough);
        Assert.Equal(5, repository.PreviewLimit);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class DashboardRepositoryStub : IOperationsDashboardRepository
    {
        private static readonly DashboardSectionResult<DashboardBooking> EmptySection =
            new(0, []);

        public DateOnly BusinessDate { get; private set; }
        public DateOnly UpcomingThrough { get; private set; }
        public int PreviewLimit { get; private set; }

        public Task<OperationsDashboardData> GetAsync(
            DateOnly businessDate,
            DateOnly upcomingThrough,
            int previewLimit,
            CancellationToken cancellationToken = default)
        {
            BusinessDate = businessDate;
            UpcomingThrough = upcomingThrough;
            PreviewLimit = previewLimit;

            return Task.FromResult(new OperationsDashboardData(
                EmptySection,
                EmptySection,
                EmptySection,
                EmptySection,
                []));
        }
    }
}
