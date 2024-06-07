using IBApi;
using Rx.IB2.Models.Utils;

namespace Rx.IB2.Extensions;

public static class ContractDetailsExtensions {
    public static DateInterval[] ToTradingDateIntervals(this ContractDetails contractDetails) {
        return DateInterval.FromIbApiMessage(contractDetails.TimeZoneId, contractDetails.TradingHours);
    }
}