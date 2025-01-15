using OtterGui.Filesystem;

namespace OtterGui;

public static class PowerSetExtensions
{
    /// <summary> Move an object in one list and move the corresponding entries in a list associated by index with the power set accordingly. </summary>
    /// <typeparam name="T1"> The type of the set items. </typeparam>
    /// <typeparam name="T2"> The type of the power set items. </typeparam>
    /// <param name="set"> The main items. </param>
    /// <param name="powerSet"> The items associated with the power set of <paramref name="set"/>. </param>
    /// <param name="setFrom"> The index of the item to move in <paramref name="set"/>. </param>
    /// <param name="setTo"> The index to move the item in <paramref name="set"/> to. </param>
    /// <returns> True if anything changed. </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool MoveWithPowerSet<T1, T2>(this IList<T1> set, List<T2> powerSet, ref int setFrom, ref int setTo)
    {
        if (!set.Move(ref setFrom, ref setTo))
            return false;

        var copy = powerSet.ToArray();
        for (var i = 0u; i < powerSet.Count; ++i)
        {
            var newIndex = Functions.MoveBit(i, setFrom, setTo);
            powerSet[(int)newIndex] = copy[(int)i];
        }

        return true;
    }

    /// <summary> Remove an object in one list and remove the corresponding entries in a list associated by index with the power set accordingly. </summary>
    /// <typeparam name="T1"> The type of the set items. </typeparam>
    /// <typeparam name="T2"> The type of the power set items. </typeparam>
    /// <param name="set"> The main items. </param>
    /// <param name="powerSet"> The items associated with the power set of <paramref name="set"/>. </param>
    /// <param name="setIndex"> The index of the item in <paramref name="set"/> to be removed. </param>
    /// <returns> True if anything changed. </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool RemoveWithPowerSet<T1, T2>(this IList<T1> set, List<T2> powerSet, int setIndex)
    {
        if (setIndex < 0 || setIndex >= set.Count)
            return false;

        set.RemoveAt(setIndex);
        var newIndex = 0;
        var flag     = 1 << setIndex;
        for (var i = 0; i < powerSet.Count; ++i)
        {
            if ((i & flag) == 0)
                powerSet[newIndex++] = powerSet[i];
        }

        Debug.Assert(newIndex == powerSet.Count / 2);
        powerSet.RemoveRange(powerSet.Count / 2, powerSet.Count / 2);
        powerSet.TrimExcess();
        return true;
    }

    /// <summary> Add an item to a list and generate new items in a list that is associated by index with its power set to the correct size. </summary>
    /// <typeparam name="T1"> The type of the set items. </typeparam>
    /// <typeparam name="T2"> The type of the power set items. </typeparam>
    /// <param name="set"> The main items. </param>
    /// <param name="powerSet"> The items associated with the power set of <paramref name="set"/>. </param>
    /// <param name="newItem"> The new item to add to the list. </param>
    /// <param name="generator"> A generator function to create new items in the associated list. </param>
    /// <param name="maxSize"> The maximum allowed size of <paramref name="set"/>. Can not be larger than 31 due to int-indexing into the power set.</param>
    /// <returns> True if the new item was added. </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool AddNewWithPowerSet<T1, T2>(this IList<T1> set, List<T2> powerSet, T1 newItem, Func<T2> generator, int maxSize = 30)
    {
        if (set.Count >= maxSize)
            return false;

        set.Add(newItem);
        var newSize = 1 << set.Count;
        powerSet.EnsureCapacity(newSize);
        powerSet.AddRange(Enumerable.Repeat(0, newSize - powerSet.Count).Select(_ => generator()));
        return true;
    }
}
