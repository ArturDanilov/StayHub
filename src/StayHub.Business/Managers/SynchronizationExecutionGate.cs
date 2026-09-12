using System.Collections.Concurrent;
using StayHub.Business.Interfaces;

namespace StayHub.Business.Managers;

public sealed class SynchronizationExecutionGate : ISynchronizationExecutionGate
{
    private readonly ConcurrentDictionary<int, byte> _runningSources = new();

    public IDisposable? TryAcquire(int sourceId)
    {
        return _runningSources.TryAdd(sourceId, 0)
            ? new Lease(_runningSources, sourceId)
            : null;
    }

    private sealed class Lease(
        ConcurrentDictionary<int, byte> runningSources,
        int sourceId) : IDisposable
    {
        private int _isDisposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
                runningSources.TryRemove(sourceId, out _);
        }
    }
}
