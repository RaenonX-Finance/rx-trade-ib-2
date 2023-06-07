namespace Rx.IB2.Extensions;

public static class CollectionExtensions {
    public static TCollection AddAndReturn<TCollection, TItem>(
        this TCollection @base, TItem item
    ) where TCollection : ICollection<TItem> {
        @base.Add(item);

        return @base;
    }
    public static TCollection UnionAndReturn<TCollection, TItem>(
        this TCollection @base, IEnumerable<TItem> item
    ) where TCollection : HashSet<TItem> {
        @base.UnionWith(item);

        return @base;
    }
}