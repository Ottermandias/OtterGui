namespace OtterGui.Classes;

/// <summary> A dictionary with ReadWrite locks on actions that also exposes its own lock. </summary>
public class ReadWriteDictionary<TKey, TValue>() : ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion), IDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _dict = [];

    public new void Dispose()
    {
        Dispose(true);
        base.Dispose();
    }

    protected virtual void Dispose(bool _)
    { }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        EnterReadLock();
        try
        {
            foreach (var kvp in _dict)
                yield return kvp;
        }
        finally
        {
            ExitReadLock();
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        using var @lock = new WriteLock(this);
        _dict.Add(item.Key, item.Value);
    }

    public void Clear()
    {
        using var @lock = new WriteLock(this);
        _dict.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        using var @lock = new ReadLock(this);
        return _dict.ContainsKey(item.Key);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        using var @lock = new ReadLock(this);
        ((ICollection<KeyValuePair<TKey, TValue>>)_dict).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        using var @lock = new WriteLock(this);
        return _dict.Remove(item.Key);
    }

    public int Count
    {
        get
        {
            using var @lock = new ReadLock(this);
            return _dict.Count;
        }
    }

    public bool IsReadOnly
        => false;

    public void Add(TKey key, TValue value)
    {
        using var @lock = new WriteLock(this);
        _dict.Add(key, value);
    }

    public bool ContainsKey(TKey key)
    {
        using var @lock = new ReadLock(this);
        return _dict.ContainsKey(key);
    }

    public bool Remove(TKey key)
    {
        using var @lock = new WriteLock(this);
        return _dict.Remove(key);
    }

    public bool Remove(TKey key, [NotNullWhen(true)] out TValue? value)
    {
        using var @lock = new WriteLock(this);
        return _dict.Remove(key, out value!);
    }

    public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
    {
        using var @lock = new ReadLock(this);
        return _dict.TryGetValue(key, out value!);
    }

    public TValue this[TKey key]
    {
        get
        {
            using var @lock = new ReadLock(this);
            return _dict[key];
        }
        set
        {
            using var @lock = new ReadLock(this);
            _dict[key] = value;
        }
    }

    public ICollection<TKey> Keys
    {
        get
        {
            using var @lock = new ReadLock(this);
            return _dict.Keys;
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            using var @lock = new ReadLock(this);
            return _dict.Values;
        }
    }


    private readonly ref struct WriteLock
    {
        private readonly ReaderWriterLockSlim _lock;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public WriteLock(ReaderWriterLockSlim @lock)
        {
            _lock = @lock;
            _lock.EnterWriteLock();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Dispose()
            => _lock.ExitWriteLock();
    }

    private readonly ref struct ReadLock
    {
        private readonly ReaderWriterLockSlim _lock;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public ReadLock(ReaderWriterLockSlim @lock)
        {
            _lock = @lock;
            _lock.EnterReadLock();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Dispose()
            => _lock.ExitReadLock();
    }
}
