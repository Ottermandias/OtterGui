using Dalamud.Interface;
using ImGuiNET;
using OtterGui.Log;

namespace OtterGui.Services;

public class ImGuiCacheService : IDisposable, IUiService
{
    private int _keepAliveFrames = 5;

    /// <summary> The number of frames every cache should be kept alive when not queried. Has to be >= 1. </summary>
    public int KeepAliveFrames
    {
        get => _keepAliveFrames;
        set
        {
            if (value == _keepAliveFrames)
                return;

            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);

            var diff = value - _keepAliveFrames;
            _keepAliveFrames = diff;
            foreach (var cache in _caches.Values)
                cache.Frame += diff;
        }
    }

    private readonly IUiBuilder _uiBuilder;
    private readonly Logger     _log;

    private int _cacheCounter = -1;

    private readonly SortedList<CacheId, CacheData> _caches = [];

    public ImGuiCacheService(IUiBuilder uiBuilder, Logger log)
    {
        _uiBuilder      =  uiBuilder;
        _log            =  log;
        _uiBuilder.Draw += OnDraw;
    }

    /// <summary> Obtain an ID to store and query a specific cache under. </summary>
    /// <returns> A unique ID. </returns>
    public CacheId GetNewId()
        => new(Interlocked.Increment(ref _cacheCounter));

    /// <summary> Obtain a stored cache if it exists without keeping it alive longer. </summary>
    /// <typeparam name="T"> The type has to match the stored cache. </typeparam>
    /// <param name="id"> The queried unique ID. </param>
    /// <param name="cache"> The currently stored cache if it exists and has a matching type or null. </param>
    /// <returns> True on success. </returns>
    public bool TryGetCache<T>(CacheId id, [NotNullWhen(true)] out T? cache)
    {
        if (_caches.TryGetValue(id, out var c) && c.Cache is T ch)
        {
            cache = ch;
            return true;
        }

        cache = default;
        return false;
    }

    /// <summary> Generate a stored cache for the given ID. </summary>
    /// <typeparam name="T"> The type of the cache to generate. </typeparam>
    /// <param name="id"> The unique ID for the cache. Should be obtained via <seealso cref="GetNewId"/>. </param>
    /// <param name="generator"> A generator to build the cache that is only invoked if the cache does not currently exist. </param>
    /// <returns> The existing or generated cache, or null in case of generation errors. </returns>
    /// <remarks> Querying a cache with this function resets its keep-alive timer. </remarks>
    public T? Cache<T>(CacheId id, Func<(T Cache, string Name)> generator)
    {
        if (_caches.TryGetValue(id, out var c))
        {
            if (c.Cache is T cache)
            {
                c.Frame = ImGui.GetFrameCount() + KeepAliveFrames;
                return cache;
            }

            _log.Debug($"[ImGuiCacheService] [{c.Name}] Queried at {id} with type {typeof(T)} instead of {c.Cache?.GetType()}.");
            if (c.CreateCache(_log, generator, id, KeepAliveFrames))
                return (T)c.Cache!;

            _caches.Remove(id);
            return default;
        }

        c = new CacheData();
        if (c.CreateCache(_log, generator, id, KeepAliveFrames))
        {
            _caches.Add(id, c);
            return (T)c.Cache!;
        }

        return default;
    }

    /// <summary> Invalidate an existing cache. </summary>
    /// <param name="id"> The unique ID to invalidate. </param>
    /// <returns> True if the cache was successfully removed and, if necessary, disposed. </returns>
    public bool InvalidateCache(CacheId id)
    {
        if (!_caches.Remove(id, out var cache))
            return false;

        cache.SaveDispose(_log, id);
        _log.Verbose($"[ImGuiCacheService] [{cache.Name}] Manually invalidated at {id}.");
        return true;
    }


    public void Dispose()
    {
        _uiBuilder.Draw -= OnDraw;
        foreach (var (id, cache) in _caches)
            cache.SaveDispose(_log, id);
        _caches.Clear();
    }

    private void OnDraw()
    {
        var frame = ImGui.GetFrameCount();
        for (var i = 0; i < _caches.Count; ++i)
        {
            var cache = _caches.Values[i];
            if (cache.Frame > frame)
                continue;

            var id = _caches.Keys[i];
            _caches.RemoveAt(i--);
            if (cache.SaveDispose(_log, id))
                _log.Verbose($"[ImGuiCacheService] [{cache.Name}] Automatically invalidated at {id}.");
        }
    }

    private class CacheData
    {
        public string  Name = "";
        public object? Cache;
        public int     Frame;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool CreateCache<T>(Logger log, Func<(T Cache, string Name)> generator, CacheId id, int keepAliveFrames)
        {
            SaveDispose(log, id);
            try
            {
                var (newCache, name) = generator();
                Cache                = newCache!;
                Name                 = name;
                Frame                = ImGui.GetFrameCount() + keepAliveFrames;
                log.Verbose($"[ImGuiCacheService] [{Name}] Generated new cache at {id}.");
                return true;
            }
            catch (Exception ex)
            {
                Cache = null!;
                log.Error($"[ImGuiCacheService] [{Name}] Error generating new cache at {id}:\n{ex}");
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool SaveDispose(Logger log, CacheId id)
        {
            try
            {
                (Cache as IDisposable)?.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"[ImGuiCacheService] [{Name}] Failed to invalidate at {id}:\n{ex}");
                return false;
            }
        }
    }

    public readonly struct CacheId(int id) : IEquatable<CacheId>, IComparable<CacheId>
    {
        private readonly int _id = id;

        public override string ToString()
            => _id.ToString();

        public bool Equals(CacheId other)
            => _id == other._id;

        public override bool Equals(object? obj)
            => obj is CacheId other && Equals(other);

        public override int GetHashCode()
            => _id;

        public static bool operator ==(CacheId left, CacheId right)
            => left.Equals(right);

        public static bool operator !=(CacheId left, CacheId right)
            => !left.Equals(right);

        public int CompareTo(CacheId other)
            => _id.CompareTo(other._id);
    }
}
