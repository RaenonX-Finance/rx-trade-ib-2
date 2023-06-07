using System.Collections.Concurrent;
using IBApi;
using Rx.IB2.Extensions;
using Rx.IB2.Models;
using Rx.IB2.Services.Base;
using ILogger = Serilog.ILogger;

namespace Rx.IB2.Services;

public class IbApiHistoryPxRequestManager : IbApiOneTimeRequestManager<PxDataBarModel> {
    protected override ILogger Log => Serilog.Log.ForContext(typeof(IbApiHistoryPxRequestManager));

    protected override string Name => "one time history Px";

    private readonly ConcurrentDictionary<int, IbApiHistoryPxRequestMeta> _meta = new();

    public void AddBar(int requestId, Bar bar) => AddData(requestId, bar.ToPxDataBarModel());

    public IbApiHistoryPxRequestMeta? GetMeta(int requestId) {
        return _meta.TryGetValue(requestId, out var meta) ? meta : null;
    }

    public void RecordMeta(int requestId, IbApiHistoryPxRequestMeta meta) {
        _meta[requestId] = meta;
    }

    public bool IsSubscription(int requestId) {
        return GetMeta(requestId)?.IsSubscription ?? false;
    }
}