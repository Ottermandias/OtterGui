using OtterGui.Text.EndObjects;

namespace OtterGui.Text;

#pragma warning disable CS1573
public static partial class ImUtf8
{
    /// <summary> Push a new ClipRect to the stack and dispose of it when leaving scope. </summary>
    /// <param name="minimum"> The upper-left corner of the rectangle. </param>
    /// <param name="maximum"> The lower-right corner of the rectangle. </param>
    /// <param name="push"> Whether to actually push the ClipRect. </param>
    /// <param name="intersectWithCurrent"> Whether to intersect the new ClipRect with the current, or only use the new one. </param>
    /// <returns> A disposable struct that can push and pop additional ClipRects and disposes of them. Use with using. </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ClipRect PushClipRect(Vector2 minimum, Vector2 maximum, bool push = true, bool intersectWithCurrent = false)
        => new ClipRect().Push(minimum, maximum, push, intersectWithCurrent);

    /// <inheritdoc cref="PushClipRect"/>
    /// <param name="size"> The size of the rectangle. </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ClipRect PushClipRectSize(Vector2 minimum, Vector2 size, bool push = true, bool intersectWithCurrent = false)
        => new ClipRect().PushSize(minimum, size, push, intersectWithCurrent);
}
