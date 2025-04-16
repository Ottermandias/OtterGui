namespace OtterGui.Extensions;

public static class StableInsertionSortExtension
{
    /// <summary> Sort <paramref name="list"/> by <paramref name="selector"/> while keeping the current order for equal objects. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void StableSort<T, TKey>(this IList<T> list, Func<T, TKey> selector)
    {
        var tmpList = new List<T>(list.Count);
        tmpList.AddRange(list.OrderBy(selector));
        for (var i = 0; i < tmpList.Count; ++i)
            list[i] = tmpList[i];
    }

    /// <summary> Sort <paramref name="list"/> by <paramref name="comparer"/> while keeping the current order for equal objects. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void StableSort<T>(this IList<T> list, Comparison<T> comparer)
    {
        var tmpList = new List<(T, int)>(list.Count);
        tmpList.AddRange(list.WithIndex());
        tmpList.Sort((a, b) =>
        {
            var ret = comparer(a.Item1, b.Item1);
            return ret != 0 ? ret : a.Item2.CompareTo(b.Item2);
        });
        for (var i = 0; i < tmpList.Count; ++i)
            list[i] = tmpList[i].Item1;
    }
}
