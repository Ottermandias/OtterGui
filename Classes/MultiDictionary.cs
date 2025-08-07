namespace OtterGui.Classes;

public class MultiDictionary<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : notnull
{
    private readonly Dictionary<TKey, List<TValue>> _dict = [];

    public MultiDictionary()
    { }

    public MultiDictionary(int count)
        => _dict = new Dictionary<TKey, List<TValue>>(count);

    public MultiDictionary(IEnumerable<KeyValuePair<TKey, TValue>> values)
    {
        foreach (var kvp in values)
            TryAdd(kvp.Key, kvp.Value);
    }

    public MultiDictionary(IReadOnlyDictionary<TKey, TValue> dict)
        => _dict = dict.ToDictionary(k => k.Key, v => new List<TValue> { v.Value });

    public IEnumerable<KeyValuePair<TKey, IReadOnlyList<TValue>>> Grouped
    {
        get
        {
            foreach (var (key, list) in _dict)
                yield return new KeyValuePair<TKey, IReadOnlyList<TValue>>(key, list);
        }
    }

    public bool TryGetValue(in TKey key, out IReadOnlyList<TValue> values)
    {
        if (_dict.TryGetValue(key, out var list))
        {
            values = list;
            return true;
        }

        values = [];
        return false;
    }

    public bool TryAdd(in TKey key, in TValue value)
    {
        ++ValueCount;
        if (_dict.TryGetValue(key, out var list))
        {
            list.Add(value);
            return true;
        }

        list = [value];
        _dict.Add(key, list);
        return true;
    }

    public bool Remove(in TKey key, out List<TValue>? values)
    {
        if (_dict.Remove(key, out values))
        {
            ValueCount -= values.Count;
            return true;
        }

        return false;
    }

    public bool RemoveValue(in TKey key, TValue value)
    {
        if (!_dict.TryGetValue(key, out var list) || !list.Remove(value))
            return false;

        --ValueCount;
        return true;
    }

    public bool ContainsKey(in TKey key)
        => _dict.ContainsKey(key);

    public bool ContainsValue(TValue value)
        => _dict.Values.Any(l => l.Contains(value));

    public int KeyCount
        => _dict.Count;

    public int ValueCount { get; private set; }

    public int Count
        => ValueCount;

    public IReadOnlyCollection<TKey> Keys
        => new CollectionAdapter<TKey>(_dict.Keys, _dict.Count);

    public IReadOnlyCollection<TValue> Values
        => new CollectionAdapter<TValue>(_dict.Values.SelectMany(l => l), ValueCount);

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (var (key, list) in _dict)
        {
            foreach (var value in list)
                yield return new KeyValuePair<TKey, TValue>(key, value);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}

public class UniquePairMultiDictionary<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : notnull
{
    private readonly Dictionary<TKey, HashSet<TValue>> _dict = [];

    public UniquePairMultiDictionary()
    { }

    public UniquePairMultiDictionary(int count)
        => _dict = new Dictionary<TKey, HashSet<TValue>>(count);

    public UniquePairMultiDictionary(IEnumerable<KeyValuePair<TKey, TValue>> values)
    {
        foreach (var kvp in values)
            TryAdd(kvp.Key, kvp.Value);
    }

    public UniquePairMultiDictionary(IReadOnlyDictionary<TKey, TValue> dict)
        => _dict = dict.ToDictionary(k => k.Key, v => new HashSet<TValue> { v.Value });

    public IEnumerable<KeyValuePair<TKey, IReadOnlySet<TValue>>> Grouped
    {
        get
        {
            foreach (var (key, set) in _dict)
                yield return new KeyValuePair<TKey, IReadOnlySet<TValue>>(key, set);
        }
    }

    public bool TryGetValue(in TKey key, [NotNullWhen(true)] out IReadOnlySet<TValue>? values)
    {
        if (_dict.TryGetValue(key, out var set))
        {
            values = set;
            return true;
        }

        values = null;
        return false;
    }

    public bool TryAdd(in TKey key, in TValue value)
    {
        if (_dict.TryGetValue(key, out var list))
        {
            if (!list.Add(value))
                return false;

            ++ValueCount;
            return true;
        }

        list = [value];
        ++ValueCount;
        _dict.Add(key, list);
        return true;
    }

    public int TryAdd(in TKey key, IEnumerable<TValue> values)
    {
        var added = 0;
        if (_dict.TryGetValue(key, out var set))
        {
            foreach (var value in values)
            {
                if (set.Add(value))
                    ++added;
            }

            return added;
        }

        set        =  [..values];
        ValueCount += set.Count;
        _dict.Add(key, set);
        return set.Count;
    }

    public bool Remove(in TKey key, out HashSet<TValue>? values)
    {
        if (_dict.Remove(key, out values))
        {
            ValueCount -= values.Count;
            return true;
        }

        return false;
    }

    public bool RemoveValue(in TKey key, TValue value)
    {
        if (!_dict.TryGetValue(key, out var set) || !set.Remove(value))
            return false;

        --ValueCount;
        return true;
    }

    public bool ContainsKey(in TKey key)
        => _dict.ContainsKey(key);

    public bool ContainsValue(TValue value)
        => _dict.Values.Any(l => l.Contains(value));

    public int KeyCount
        => _dict.Count;

    public int ValueCount { get; private set; }

    public int Count
        => ValueCount;

    public IReadOnlyCollection<TKey> Keys
        => new CollectionAdapter<TKey>(_dict.Keys, _dict.Count);

    public IReadOnlyCollection<TValue> Values
        => new CollectionAdapter<TValue>(_dict.Values.SelectMany(l => l), ValueCount);

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (var (key, list) in _dict)
        {
            foreach (var value in list)
                yield return new KeyValuePair<TKey, TValue>(key, value);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}

public static class MultiDictionaryExtensions
{
    public static MultiDictionary<TKey, TValue> ToMultiDictionary<T, TKey, TValue>(this IEnumerable<T> data, Func<T, TKey> keySelector,
        Func<T, TValue> valueSelector) where TKey : notnull
    {
        var ret = new MultiDictionary<TKey, TValue>();
        foreach (var obj in data)
        {
            var key   = keySelector(obj);
            var value = valueSelector(obj);
            ret.TryAdd(key, value);
        }

        return ret;
    }

    public static UniquePairMultiDictionary<TKey, TValue> ToUniquePairMultiDictionary<T, TKey, TValue>(this IEnumerable<T> data,
        Func<T, TKey> keySelector, Func<T, TValue> valueSelector) where TKey : notnull
    {
        var ret = new UniquePairMultiDictionary<TKey, TValue>();
        foreach (var obj in data)
        {
            var key   = keySelector(obj);
            var value = valueSelector(obj);
            ret.TryAdd(key, value);
        }

        return ret;
    }
}

internal readonly struct CollectionAdapter<T>(IEnumerable<T> enumerable, int count) : IReadOnlyCollection<T>
{
    public IEnumerator<T> GetEnumerator()
        => enumerable.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public int Count
        => count;
}
