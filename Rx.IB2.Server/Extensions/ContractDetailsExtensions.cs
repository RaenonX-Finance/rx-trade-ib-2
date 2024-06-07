using IBApi;
using Rx.IB2.Models.Utils;

namespace Rx.IB2.Extensions;

public static class ContractDetailsExtensions {
    public static DateInterval[] ToTradingDateIntervals(this ContractDetails contractDetails) {
        // `TimeZoneId` and/or `TradingHours` could be null, at least after the Friday market close on options
        if (contractDetails.TimeZoneId is null || contractDetails.TradingHours is null) {
            return [];
        }
        
        return DateInterval.FromIbApiMessage(contractDetails.TimeZoneId, contractDetails.TradingHours);
    }
}