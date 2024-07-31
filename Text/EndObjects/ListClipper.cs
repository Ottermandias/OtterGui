using ImGuiNET;

namespace OtterGui.Text.EndObjects;

public ref struct ListClipper
{
    public readonly ImGuiListClipperPtr ClipperPtr;
    public          bool                Disposed;

    public readonly ref int DisplayStart
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref ClipperPtr.DisplayStart;
    }

    public readonly ref int DisplayEnd
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref ClipperPtr.DisplayEnd;
    }

    public readonly ref int ItemsCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref ClipperPtr.ItemsCount;
    }

    public readonly ref float ItemsHeight
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref ClipperPtr.ItemsHeight;
    }

    public readonly ref float StartPosY
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref ClipperPtr.StartPosY;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal unsafe ListClipper(int itemsCount, float itemsHeight)
    {
        ClipperPtr = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
        ClipperPtr.Begin(itemsCount, itemsHeight);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ForceDisplayRangeByIndices(int itemMin, int itemMax)
        => ClipperPtr.ForceDisplayRangeByIndices(itemMin, itemMax);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Step()
        => ClipperPtr.Step();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (Disposed)
            return;

        ClipperPtr.End();
        ClipperPtr.Destroy();
        Disposed = true;
    }
}
