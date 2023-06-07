using Microsoft.Extensions.Primitives;

namespace Rx.IB2.Extensions;

public static class ApiRequestExtensions {
    public static string GetFirstValueInQuery(this IQueryCollection queryCollection, string queryKey) {
        var symbolQuery = queryCollection[queryKey];

        if (symbolQuery == StringValues.Empty) {
            throw new ArgumentException($"`{queryKey}` is required in the query");
        }

        if (symbolQuery.Count <= 0) {
            throw new ArgumentException($"`{queryKey}` should have one or more key value pair in the query");
        }
        
        var symbol = symbolQuery[0];

        if (symbol is null) {
            throw new ArgumentException($"`{queryKey}` is null in the query");
        }

        return symbol;
    }
}