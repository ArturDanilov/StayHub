namespace StayHub.Business.Interfaces;

public interface ISynchronizationExecutionGate
{
    IDisposable? TryAcquire(int sourceId);
}
