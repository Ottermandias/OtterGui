using ImGuiNET;
using OtterGui.Internal.Enums;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToAutoProperty
// ReSharper disable InconsistentNaming
#pragma warning disable IDE0044
#pragma warning disable CS9084

namespace OtterGui.Internal.Structs;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct ImGuiWindow
{
    // @formatter:off
    [FieldOffset(0x0000)] private byte* _name;
    [FieldOffset(0x0008)] private ImGuiId _id;
    [FieldOffset(0x000C)] private ImGuiWindowFlags _flags;
    [FieldOffset(0x0010)] private ImGuiWindowFlags _lastFlags;
    [FieldOffset(0x0048)] private Vector2 _pos;
    [FieldOffset(0x0050)] private Vector2 _size;
    [FieldOffset(0x0058)] private Vector2 _sizeFull;
    [FieldOffset(0x0060)] private Vector2 _contentSize;
    [FieldOffset(0x0068)] private Vector2 _contentSizeIdeal;
    [FieldOffset(0x0070)] private Vector2 _contentSizeExplicit;
    [FieldOffset(0x0078)] private Vector2 _windowPadding;
    [FieldOffset(0x007C)] private float _windowRounding;
    [FieldOffset(0x0080)] private float _windowBorderSize;
    [FieldOffset(0x0084)] private int _nameBufLength;
    [FieldOffset(0x0098)] private Vector2 _scroll;
    [FieldOffset(0x00A0)] private Vector2 _scrollMax;
    [FieldOffset(0x00A8)] private Vector2 _scrollTarget;
    [FieldOffset(0x00B0)] private Vector2 _scrollTargetCenterRatio;
    [FieldOffset(0x00B8)] private Vector2 _scrollTargetEdgeSnapDist;
    [FieldOffset(0x00C0)] private Vector2 _scrollBarSizes;
    [FieldOffset(0x00C8)][MarshalAs(UnmanagedType.U1)] private bool _scrollBarX;
    [FieldOffset(0x00C9)][MarshalAs(UnmanagedType.U1)] private bool _scrollBarY;
    [FieldOffset(0x00CA)][MarshalAs(UnmanagedType.U1)] private bool _active;
    [FieldOffset(0x00CB)][MarshalAs(UnmanagedType.U1)] private bool _wasActive;
    [FieldOffset(0x00CC)][MarshalAs(UnmanagedType.U1)] private bool _writeAccessed;
    [FieldOffset(0x00CD)][MarshalAs(UnmanagedType.U1)] private bool _collapsed;
    [FieldOffset(0x00CE)][MarshalAs(UnmanagedType.U1)] private bool _wantCollapseToggle;
    [FieldOffset(0x00CF)][MarshalAs(UnmanagedType.U1)] private bool _skipItems;
    [FieldOffset(0x00D0)][MarshalAs(UnmanagedType.U1)] private bool _appearing;
    [FieldOffset(0x00D1)][MarshalAs(UnmanagedType.U1)] private bool _hidden;
    [FieldOffset(0x00D2)][MarshalAs(UnmanagedType.U1)] private bool _isFallbackWindow;
    [FieldOffset(0x00D3)][MarshalAs(UnmanagedType.U1)] private bool _isExplicitChild;
    [FieldOffset(0x00D4)][MarshalAs(UnmanagedType.U1)] private bool _hasCloseButton;
    [FieldOffset(0x0110)] private ImVector<ImGuiId> _idStack;
    [FieldOffset(0x0118)] private ImGuiWindowTemp   _dc;
    // @formatter:on


    public bool SkipItems
        => _skipItems;

    public ref ImGuiWindowTemp DC
        => ref _dc;
}

#pragma warning restore IDE0044
#pragma warning restore CS9084
