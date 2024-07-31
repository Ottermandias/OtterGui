using ImGuiNET;
using OtterGui.Text.HelperObjects;

namespace OtterGui.Text.EndObjects;

public readonly ref struct Columns
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public unsafe Columns(int count, ReadOnlySpan<byte> id = default, bool border = true)
        => ImGuiNative.igColumns(count, id.Start(), border.Byte());

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Next()
        => ImGuiNative.igNextColumn();

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public unsafe void Dispose()
        => ImGuiNative.igColumns(1, null, 1);
}
