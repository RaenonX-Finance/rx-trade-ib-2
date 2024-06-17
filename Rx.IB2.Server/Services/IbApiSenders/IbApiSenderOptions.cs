using System.Collections.Concurrent;
using IBApi;
using Rx.IB2.Enums;
using Rx.IB2.Extensions;
using Rx.IB2.Models.Options;
using Rx.IB2.Models.Requests;
using Rx.IB2.Models.Responses;
using Rx.IB2.Models.Utils;
using Rx.IB2.Utils;

namespace Rx.IB2.Services.IbApiSenders;

public partial class IbApiSender {
    public void RequestOptionDefinitions(OptionDefinitionRequest request) {
        Log.Information("Requesting option definitions of {Symbol}", request.Symbol);

        if (request.InUsePxRequestIds.Count > 0) {
            Log.Information(
                "To cancel market data subscription of the following requests: {@RequestIds}",
                request.InUsePxRequestIds
            );
        }

        foreach (var pxRequestId in request.InUsePxRequestIds) {
            ClientSocket.cancelMktData(pxRequestId);
        }

        if (request.InUseContractId is not null) {
            CancelRealtime(request.Account, request.InUseContractId.Value);
        }

        var options = new ToContractOptions {
            AutoConvertOptionsToStocks = true
        };
        foreach (var contractDetail in RequestContractDetails(request.Symbol.ToContract(options))) {
            var requestId = RequestManager.GetNextRequestIdNoCancel();
            OptionDefinitionsManager.RecordRequestOrigin(requestId, request.Origin);

            var contract = contractDetail.Contract;
            var contractModel = contract.ToContractModel();

            // Requesting the underlying price at the same time for various reasons, such as:
            // - Calculating GEX
            // - Options strike to use
            SubscribeRealtime(request.Account, contractDetail);

            OptionDefinitionsManager.EnterLock(requestId);

            Log.Information(
                "#{RequestId}: Requesting options param of {Symbol}",
                requestId,
                request.Symbol
            );
            ClientSocket.reqSecDefOptParams(
                requestId,
                contract.Symbol,
                contractModel.ExchangeForOptionDefinitionsQuery,
                contractModel.SecTypeForOptionDefinitionsQuery,
                contract.ConId
            );
        }
    }

    public async Task<OptionPxResponse> SubscribeOptionsPx(OptionPxRequest request) {
        Log.Information(
            "Received option Px subscription request of {Symbol} expiring {@Expiry} at {@Strikes}",
            request.Symbol,
            request.Expiry,
            request.Strikes
        );

        var realtimeRequestLiveFetchIds = new List<Task<List<int>>>();
        var contracts = new ConcurrentDictionary<OptionsContractDictKey, Contract>();

        foreach (var expiry in request.Expiry) {
            foreach (var strike in request.Strikes) {
                realtimeRequestLiveFetchIds.Add(Task.Run(() => {
                    var callContract = ContractMaker.MakeOptions(
                        request.Symbol,
                        expiry,
                        OptionRight.Call,
                        strike,
                        request.TradingClass
                    );

                    return SubscribeRealtimeFromContract(
                        request.Account,
                        callContract,
                        contract => contracts.AddOrUpdate(
                            new OptionsContractDictKey { Strike = strike, Expiry = expiry, Right = OptionRight.Call },
                            contract,
                            (_, _) => contract
                        )
                    );
                }));
                await Throttle();
                realtimeRequestLiveFetchIds.Add(Task.Run(() => {
                    var putContract = ContractMaker.MakeOptions(
                        request.Symbol,
                        expiry,
                        OptionRight.Put,
                        strike,
                        request.TradingClass
                    );

                    return SubscribeRealtimeFromContract(
                        request.Account,
                        putContract,
                        contract => contracts.AddOrUpdate(
                            new OptionsContractDictKey { Strike = strike, Expiry = expiry, Right = OptionRight.Put },
                            contract,
                            (_, _) => contract
                        )
                    );
                }));
                await Throttle();
            }
        }

        var realTimeRequestLiveIds = await Task.WhenAll(realtimeRequestLiveFetchIds);
        var realTimeRequestFrozenIds = await Task.WhenAll(
            contracts.Values
                .Select(contract => Task.Run(() => SubscribeFrozenRealtimeFromContract(request.Account, contract)))
        );

        return new OptionPxResponse {
            Origin = request.Origin,
            RealtimeRequestIds = realTimeRequestLiveIds.Concat(realTimeRequestFrozenIds).SelectMany(x => x).ToList(),
            ContractIdPairs = contracts
                .GroupBy(x => (x.Key.Expiry, x.Key.Strike))
                .Select(x => new OptionContractIdPair {
                    Expiry = x.Key.Expiry,
                    Strike = x.Key.Strike,
                    Call = x.First(item => item.Key.Right == OptionRight.Call).Value.ConId,
                    Put = x.First(item => item.Key.Right == OptionRight.Put).Value.ConId
                })
                .ToList()
        };
    }

    public async Task<OptionPxResponse> RequestOptionsPx(OptionPxRequest request) {
        Log.Information(
            "Received option Px one-time request of {Symbol} expiring {@Expiry} at {@Strikes}",
            request.Symbol,
            request.Expiry,
            request.Strikes
        );

        var contracts = new ConcurrentDictionary<OptionsContractDictKey, Contract>();

        // Requesting realtime and frozen separately to avoid keep switching in-between
        // --- Request realtime
        foreach (var expiry in request.Expiry) {
            var tasksOfExpiry = new List<Task>();

            foreach (var strike in request.Strikes) {
                tasksOfExpiry.Add(Task.Run(() => {
                    // Not parallelizing to intentionally delay the real time market data request
                    var callContract = ContractMaker.MakeOptions(
                        request.Symbol,
                        expiry,
                        OptionRight.Call,
                        strike,
                        request.TradingClass
                    );
                    contracts.AddOrUpdate(
                        new OptionsContractDictKey { Strike = strike, Expiry = expiry, Right = OptionRight.Call },
                        callContract,
                        (_, _) => callContract
                    );

                    RequestRealtimeFromContract(
                        request.Account,
                        callContract,
                        contract => contracts.AddOrUpdate(
                            new OptionsContractDictKey { Strike = strike, Expiry = expiry, Right = OptionRight.Call },
                            contract,
                            (_, _) => contract
                        ),
                        OptionPxTargetTicks
                    );
                }));
                await Throttle();
                tasksOfExpiry.Add(Task.Run(() => {
                    var putContract = ContractMaker.MakeOptions(
                        request.Symbol,
                        expiry,
                        OptionRight.Put,
                        strike,
                        request.TradingClass
                    );
                    contracts.AddOrUpdate(
                        new OptionsContractDictKey { Strike = strike, Expiry = expiry, Right = OptionRight.Put },
                        putContract,
                        (_, _) => putContract
                    );

                    RequestRealtimeFromContract(
                        request.Account,
                        putContract,
                        contract => contracts.AddOrUpdate(
                            new OptionsContractDictKey { Strike = strike, Expiry = expiry, Right = OptionRight.Put },
                            contract,
                            (_, _) => contract
                        ),
                        OptionPxTargetTicks
                    );
                }));
                await Throttle();
            }

            // Wait for all the tasks of the current `expiry` to complete before requesting another `expiry`
            await Task.WhenAll(tasksOfExpiry);
        }

        // --- Request frozen
        foreach (var contract in contracts.Values) {
            RequestFrozenRealtimeFromContract(request.Account, contract, OptionPxTargetTicks);
        }

        return new OptionPxResponse {
            Origin = request.Origin,
            // Realtime request IDs are sent to client for canceling px on unload.
            // Since this is a one-time request, no need to send request IDs for cancellations 
            RealtimeRequestIds = [],
            ContractIdPairs = contracts
                .GroupBy(x => (x.Key.Expiry, x.Key.Strike))
                .Select(x => new OptionContractIdPair {
                    Expiry = x.Key.Expiry,
                    Strike = x.Key.Strike,
                    Call = x.First(item => item.Key.Right == OptionRight.Call).Value.ConId,
                    Put = x.First(item => item.Key.Right == OptionRight.Put).Value.ConId
                })
                .ToList()
        };
    }
}