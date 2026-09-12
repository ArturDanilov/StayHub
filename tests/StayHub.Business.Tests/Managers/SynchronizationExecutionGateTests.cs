using StayHub.Business.Managers;
using Xunit;

namespace StayHub.Business.Tests.Managers;

public sealed class SynchronizationExecutionGateTests
{
    [Fact]
    public void TryAcquire_SameSourceTwice_BlocksSecondExecution()
    {
        var gate = new SynchronizationExecutionGate();
        using var firstExecution = gate.TryAcquire(1);

        using var secondExecution = gate.TryAcquire(1);

        Assert.NotNull(firstExecution);
        Assert.Null(secondExecution);
    }

    [Fact]
    public void TryAcquire_AfterLeaseIsDisposed_AllowsNextExecution()
    {
        var gate = new SynchronizationExecutionGate();
        var firstExecution = gate.TryAcquire(1);
        Assert.NotNull(firstExecution);
        firstExecution.Dispose();

        using var nextExecution = gate.TryAcquire(1);

        Assert.NotNull(nextExecution);
    }

    [Fact]
    public void TryAcquire_DifferentSources_AllowsParallelExecutions()
    {
        var gate = new SynchronizationExecutionGate();

        using var firstSourceExecution = gate.TryAcquire(1);
        using var secondSourceExecution = gate.TryAcquire(2);

        Assert.NotNull(firstSourceExecution);
        Assert.NotNull(secondSourceExecution);
    }
}
