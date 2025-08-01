using Dalamud.Bindings.ImGui;

namespace OtterGui.Text.EndObjects;

/// <summary> A clipper utility that cleans up after itself. </summary>
public ref struct ListClipper
{
    /// <summary> The clipper itself. </summary>
    public ImGuiListClipperPtr ClipperPtr;

    public bool Disposed { get; private set; }

    /// <summary> The index of the first displayed element. </summary>
    public readonly int DisplayStart
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ClipperPtr.DisplayStart;
    }

    /// <summary> The index of the last displayed element. </summary>
    public readonly int DisplayEnd
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ClipperPtr.DisplayEnd;
    }

    /// <summary> The number of displayed items. </summary>
    public readonly int ItemsCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ClipperPtr.ItemsCount;
    }

    /// <summary> The total height of the items. </summary>
    public readonly float ItemsHeight
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ClipperPtr.ItemsHeight;
    }

    /// <summary> The height offset of the skipped height. </summary>
    public readonly float StartPosY
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ClipperPtr.StartPosY;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal unsafe ListClipper(int itemsCount, float itemsHeight)
    {
        ClipperPtr = ImGui.ImGuiListClipper();
        ClipperPtr.Begin(itemsCount, itemsHeight);
    }

    /// <summary> Force the current display range using a pair of indices. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void ForceDisplayRangeByIndices(int itemMin, int itemMax)
        => ClipperPtr.ForceDisplayRangeByIndices(itemMin, itemMax);

    /// <summary> Execute a step in the clipper. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe bool Step()
        => ClipperPtr.Step();

    /// <summary> Dispose of the list clipper. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void Dispose()
    {
        if (Disposed)
            return;

        ClipperPtr.End();
        ClipperPtr.Destroy();
        Disposed = true;
    }
}
