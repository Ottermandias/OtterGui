using Dalamud.Bindings.ImGui;

namespace OtterGui.Text.EndObjects;

public readonly ref struct Columns
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public unsafe Columns(int count, ReadOnlySpan<byte> id = default, bool border = true)
        => ImGui.Columns(count, id, border);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Next()
        => ImGui.NextColumn();

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Dispose()
        => ImGui.Columns(1, default, true);
}
