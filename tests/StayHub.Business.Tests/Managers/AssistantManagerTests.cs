using StayHub.Business.Interfaces;
using StayHub.Business.Managers;
using StayHub.Business.Models;
using StayHub.Contracts.Assistant;
using StayHub.Domain.Models;
using Xunit;

namespace StayHub.Business.Tests.Managers;

public sealed class AssistantManagerTests
{
    [Fact]
    public async Task ChatAsync_BuildsReadOnlySnapshotWithoutGuestContactDetails()
    {
        var repository = new FakeReservationRepository
        {
            Reservations = [CreateReservation()]
        };
        var client = new FakeAssistantClient { Answer = "Сегодня заезжает Toni Danilov." };
        var manager = new AssistantManager(repository, client);

        var result = await manager.ChatAsync(
            new AssistantChatRequest { Message = "Кто сегодня заезжает?" },
            Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(client.Answer, result.Answer);
        Assert.Equal(100, repository.LastQuery?.PageSize);

        var systemPrompt = Assert.Single(client.Messages, message => message.Role == "system").Content;
        Assert.Contains("read-only", systemPrompt);
        Assert.Contains("Toni Danilov", systemPrompt);
        Assert.Contains("StayHub Lake Resort", systemPrompt);
        Assert.DoesNotContain("toni.danilov@example.com", systemPrompt);
        Assert.DoesNotContain("+49 151 00000003", systemPrompt);
    }

    [Fact]
    public async Task ChatAsync_KeepsOnlyLastEightValidHistoryMessages()
    {
        var repository = new FakeReservationRepository();
        var client = new FakeAssistantClient();
        var manager = new AssistantManager(repository, client);
        var history = Enumerable.Range(1, 10)
            .Select(index => new AssistantConversationMessage(
                index % 2 == 0 ? "assistant" : "user",
                $"message-{index}"))
            .ToList();

        await manager.ChatAsync(
            new AssistantChatRequest
            {
                Message = "Aktuelle Frage",
                History = history
            },
            Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(10, client.Messages.Count);
        Assert.DoesNotContain(client.Messages, message => message.Content == "message-1");
        Assert.DoesNotContain(client.Messages, message => message.Content == "message-2");
        Assert.Contains(client.Messages, message => message.Content == "message-3");
        Assert.Equal("Aktuelle Frage", client.Messages[^1].Content);
        Assert.Equal("user", client.Messages[^1].Role);
    }

    private static Reservation CreateReservation()
    {
        var property = new Property { Id = 1, Name = "StayHub Lake Resort" };
        var guest = new Guest
        {
            Id = 1,
            FirstName = "Toni",
            LastName = "Danilov",
            Email = "toni.danilov@example.com",
            Phone = "+49 151 00000003"
        };
        var source = new Source { Id = 1, Name = "Mock PMS" };

        return new Reservation
        {
            Id = 1,
            ExternalId = "MOCK-1001",
            SourceId = source.Id,
            Source = source,
            GuestId = guest.Id,
            Guest = guest,
            PropertyId = property.Id,
            Property = property,
            ArrivalDate = new DateOnly(2026, 7, 25),
            DepartureDate = new DateOnly(2026, 7, 30),
            Status = ReservationStatus.CheckedIn
        };
    }

    private sealed class FakeAssistantClient : IAssistantClient
    {
        public string Answer { get; set; } = "Antwort";
        public IReadOnlyList<AssistantPromptMessage> Messages { get; private set; } = [];

        public Task<string> CompleteAsync(
            IReadOnlyList<AssistantPromptMessage> messages,
            CancellationToken cancellationToken = default)
        {
            Messages = messages;
            return Task.FromResult(Answer);
        }
    }

    private sealed class FakeReservationRepository : IReservationRepository
    {
        public IReadOnlyList<Reservation> Reservations { get; init; } = [];
        public ReservationQuery? LastQuery { get; private set; }

        public Task<PagedResult<Reservation>> GetAllAsync(
            ReservationQuery query,
            CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            return Task.FromResult(new PagedResult<Reservation>(Reservations, Reservations.Count));
        }

        public Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<Reservation?> GetByExternalIdAsync(
            int sourceId,
            string externalId,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<bool> HasDateConflictAsync(
            int propertyId,
            DateOnly arrivalDate,
            DateOnly departureDate,
            int? excludedReservationId = null,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<bool> ExternalIdExistsAsync(
            int sourceId,
            string externalId,
            int? excludedReservationId = null,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<Reservation> AddAsync(
            Reservation reservation,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public void Delete(Reservation reservation) => throw new NotSupportedException();
    }
}
