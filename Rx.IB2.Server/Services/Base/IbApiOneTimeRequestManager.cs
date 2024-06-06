using System.Collections.Concurrent;
using Rx.IB2.Extensions;
using ILogger = Serilog.ILogger;

namespace Rx.IB2.Services.Base;

public abstract class IbApiOneTimeRequestManager<T> {
    private readonly ConcurrentDictionary<int, SemaphoreSlim> _semaphores = new();

    private readonly ConcurrentDictionary<int, IList<T>> _dataHolder = new();

    protected abstract ILogger Log { get; }

    protected abstract string Name { get; }

    public virtual void AddData(int requestId, T data) {
        _dataHolder
            .GetOrSetDefaultAndReturn(requestId, () => new List<T>())
            .Add(data);
    }

    public void EnterLock(int requestId) {
        Log.Information("#{RequestId}: Entering {Name} lock", requestId, Name);
        _semaphores
            .GetOrSetDefaultAndReturn(requestId, () => new SemaphoreSlim(initialCount: 1, maxCount: 1))
            .Wait();
    }

    public bool ReleaseLock(int requestId) {
        if (!_semaphores.TryGetValue(requestId, out var semaphore)) {
            return false;
        }

        Log.Information("#{RequestId}: Releasing {Name} lock", requestId, Name);
        semaphore.Release();
        return true;
    }

    public IEnumerable<T> WaitAndGetData(int requestId) {
        Log.Information("#{RequestId}: Waiting {Name} lock", requestId, Name);
        if (!_semaphores.TryGetValue(requestId, out var semaphore)) {
            Log.Warning("#{RequestId}: no associated {Name} lock to wait", requestId, Name);
            return [];
        }

        semaphore.Wait();
        semaphore.Release();

        if (_dataHolder.TryGetValue(requestId, out var data)) {
            return data;
        }

        Log.Warning("#{RequestId}: No {Name} found", requestId, Name);
        return [];
    }

    public IList<T> GetData(int requestId) {
        if (_dataHolder.TryGetValue(requestId, out var data)) {
            return data;
        }

        throw new InvalidOperationException(
            $"Attempted to get the data of request #{requestId} but it's uninitialized"
        );
    }
}