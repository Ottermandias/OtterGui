// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToAutoProperty

namespace OtterGui.Internal.Structs;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ImGuiWindowTemp
{
    private Vector2 _cursorPos;
    private Vector2 _cursorPosPrevLine;
    private Vector2 _cursorStartPos;
    private Vector2 _cursorMaxPos;
    private Vector2 _idealMaxPos;
    private Vector2 _currLineSize;
    private Vector2 _prevLineSize;
    private float _currLineTextBaseOffset;

    public Vector2 CursorPos
        => _cursorPos;

    public float CurrLineTextBaseOffset
        => _currLineTextBaseOffset;
}
