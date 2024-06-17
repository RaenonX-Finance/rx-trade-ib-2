using System.Collections.Concurrent;
using Rx.IB2.Enums;

namespace Rx.IB2.Services;

public class IbApiOneTimePxRequestManager {
    private readonly ConcurrentDictionary<int, HashSet<PxTick>> _received = new();

    private readonly ConcurrentDictionary<int, HashSet<PxTick>> _target = new();

    public void RecordTarget(int requestId, IEnumerable<PxTick> targetTicks) {
        var targetTicksSet = targetTicks.ToHashSet();

        _target.AddOrUpdate(
            requestId,
            targetTicksSet,
            (_, _) => targetTicksSet
        );
    }

    public ICollection<int> ActiveRequests => _received.Keys;

    private int? GetRequestIdIfReceivedAll(int requestId) {
        if (!_received[requestId].IsSupersetOf(_target[requestId])) {
            return null;
        }

        _received.Remove(requestId, out _);
        return requestId;
    }

    public int? RecordReceivedSingle(int requestId, PxTick tick) {
        if (!_target.ContainsKey(requestId)) {
            return null;
        }

        _received.GetOrAdd(requestId, []).Add(tick);

        return GetRequestIdIfReceivedAll(requestId);
    }

    public int? RecordReceivedMultiple(int requestId, IEnumerable<PxTick> ticks) {
        if (!_target.ContainsKey(requestId)) {
            return null;
        }

        _received.GetOrAdd(requestId, []).UnionWith(ticks);

        return GetRequestIdIfReceivedAll(requestId);
    }
}