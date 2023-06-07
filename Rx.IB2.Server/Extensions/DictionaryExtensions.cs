namespace Rx.IB2.Extensions;

public static class DictionaryExtensionsForStruct {
    public static TValue? TryGetValueOrNull<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary, TKey key
    ) where TValue : struct {
        var found = dictionary.TryGetValue(key, out var value);

        return found ? value : null;
    }
}

public static class DictionaryExtensionsForClass {
    public static TValue? TryGetValueOrNull<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary, TKey key
    ) where TValue : class {
        var found = dictionary.TryGetValue(key, out var value);

        return found ? value : null;
    }
}

public static class DictionaryExtensions {
    public static TValue GetOrSetDefaultAndReturn<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> generateDefault
    ) {
        if (dictionary.TryGetValue(key, out var val)) {
            return val;
        }

        val = generateDefault();
        dictionary[key] = val;

        return val;
    }
}