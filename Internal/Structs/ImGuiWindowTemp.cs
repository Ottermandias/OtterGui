// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToAutoProperty

namespace OtterGui.Internal.Structs;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ImGuiWindowTemp
{
    private Vector2 _cursorPos;

    public Vector2 CursorPos
        => _cursorPos;
}
