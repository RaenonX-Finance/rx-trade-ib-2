﻿using Rx.IB2.Enums;
using Rx.IB2.Extensions;
using Rx.IB2.Interfaces;
using Rx.IB2.Models;
using Rx.IB2.Models.Requests;
using Rx.IB2.Models.Requests.OptionVolatilityHistoryRequest;

namespace Rx.IB2.Services.IbApiSenders;

public partial class IbApiSender {
    public int? SubscribePxHistory(PxHistoryRequest request) {
        var requestId = RequestManager.GetNextRequestId(
            IbApiRequestType.History,
            request.Account,
            request.ContractId
        );

        Log.Information(
            "#{RequestId}: Requesting {DataType} price of [{ContractId}] @ {Interval} in {Duration}",
            requestId,
            request.DataType,
            request.ContractId,
            request.Interval,
            request.Duration
        );

        return RequestPxHistory(request, requestId);
    }

    public List<int?> SubscribeOptionVolatilityHistory(OptionVolatilityHistoryRequest request) {
        var requestIdForIv = RequestManager.GetNextRequestId(
            IbApiRequestType.History,
            request.Account
        );
        Log.Information(
            "#{RequestId}: Requesting volatility history of {Symbol} (IV)",
            requestIdForIv,
            request.Symbol
        );
        
        var requestIdForHv = RequestManager.GetNextRequestId(
            IbApiRequestType.History,
            request.Account
        );
        Log.Information(
            "#{RequestId}: Requesting volatility history of {Symbol} (HV)",
            requestIdForHv,
            request.Symbol
        );

        return [
            RequestPxHistory(request.AsHistoryVolatilityRequestIv(), requestIdForIv),
            RequestPxHistory(request.AsHistoryVolatilityRequestHv(), requestIdForHv)
        ];
    }

    private int? RequestPxHistory(IHistoryPxRequest request, int requestId) {
        ClientSocket.reqHistoricalData(
            requestId,
            request.Contract,
            request.IsSubscription ? "" : DateTime.Now.ToUniversalTime().ToIbApiFormat(),
            request.Duration,
            request.BarSize.ToString(),
            request.DataType.ToString().ToSnakeCase().ToUpper(),
            Convert.ToInt32(request.RthOnly),
            1, // 1 for "yyyyMMdd HH:mm:ss"; 2 for system format
            request.IsSubscription,
            null
        );

        if (request.Contract.ConId == default) {
            Log.Error("#{RequestId}: Contract ID on history Px request unavailable", requestId);
            return null;
        }

        HistoryPxRequestManager.RecordMeta(requestId, new IbApiHistoryPxRequestMeta {
            ContractId = request.Contract.ConId,
            Interval = request.BarSize.Interval,
            DataType = request.DataType,
            IsSubscription = request.IsSubscription
        });

        return requestId;
    }

    public void CancelHistory(int requestId) {
        Log.Information("#{RequestId}: Cancelling history px subscription", requestId);
        ClientSocket.cancelHistoricalData(requestId);
        RequestManager.ClearByRequestId(requestId);
    }
}