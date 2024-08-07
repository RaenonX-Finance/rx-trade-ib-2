﻿using System.Collections.Concurrent;
using Rx.IB2.Enums;

namespace Rx.IB2.Services;

public class IbApiOneTimePxRequestManager {
    private readonly ConcurrentDictionary<int, HashSet<PxTick>> _received = new();

    private readonly ConcurrentBag<int> _cancelled = [];

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
        if (!_received.GetValueOrDefault(requestId)?.IsSupersetOf(_target[requestId]) ?? true) {
            return null;
        }

        _received.Remove(requestId, out _);
        _cancelled.Add(requestId);
        return requestId;
    }

    public int? RecordReceivedSingle(int requestId, PxTick tick) {
        if (!_target.ContainsKey(requestId)) {
            return null;
        }

        // Only record requests that are not cancelled
        if (!_cancelled.Contains(requestId)) {
            _received.GetOrAdd(requestId, []).Add(tick);
        }

        return GetRequestIdIfReceivedAll(requestId);
    }

    public int? RecordReceivedMultiple(int requestId, IEnumerable<PxTick> ticks) {
        if (!_target.ContainsKey(requestId)) {
            return null;
        }

        // Only record requests that are not cancelled
        if (!_cancelled.Contains(requestId)) {
            _received.GetOrAdd(requestId, []).UnionWith(ticks);
        }

        return GetRequestIdIfReceivedAll(requestId);
    }
}