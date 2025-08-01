using Dalamud.Bindings.ImGui;
using OtterGui.Text.HelperObjects;

namespace OtterGui.Text.EndObjects;

public readonly ref struct Columns
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public unsafe Columns(int count, ReadOnlySpan<byte> id = default, bool border = true)
        => ImGui.Columns(count, id.Start(), border);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Next()
        => ImGui.NextColumn();

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public unsafe void Dispose()
        => ImGui.Columns(1, (byte*)null, true);
}
