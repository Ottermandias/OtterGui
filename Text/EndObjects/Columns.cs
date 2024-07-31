using ImGuiNET;
using OtterGui.Text.HelperObjects;

namespace OtterGui.Text.EndObjects;

public readonly ref struct Columns
{
    public unsafe Columns(int count, ReadOnlySpan<byte> id = default, bool border = true)
    {
        ImGuiNative.igColumns(count, id.Start(), border.Byte());
    }

    public readonly void Next()
    {
        ImGuiNative.igNextColumn();
    }

    public readonly unsafe void Dispose()
    {
        ImGuiNative.igColumns(1, null, 1);
    }
}
