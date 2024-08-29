using ImGuiNET;
using OtterGui.Text.HelperObjects;
using OtterGuiInternal.Utility;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

namespace OtterGui.Text.Widget;

/// <summary> A two-state Checkbox that displays either a checkmark for True values or an X for False values, with no empty state. </summary>
public sealed class TriStateCheckbox : MultiStateCheckbox<bool?>
{
    /// <summary> A static instance to draw more easily. </summary>
    public static readonly TriStateCheckbox Instance = new();

    /// <inheritdoc/>
    protected override void RenderSymbol(bool? value, Vector2 position, float size)
    {
        switch (value)
        {
            case null:
                SymbolHelpers.RenderDot(ImGui.GetWindowDrawList(), position, ImGui.GetColorU32(ImGuiCol.CheckMark), size);
                break;
            case true:
                SymbolHelpers.RenderCheckmark(ImGui.GetWindowDrawList(), position, ImGui.GetColorU32(ImGuiCol.CheckMark), size);
                break;
            case false:
                SymbolHelpers.RenderCross(ImGui.GetWindowDrawList(), position, ImGui.GetColorU32(ImGuiCol.CheckMark), size);
                break;
        }
    }

    /// <summary> Draw the tri-state checkbox. </summary>
    /// <param name="label"> The label for the checkbox as a UTF8 string. HAS to be null-terminated. </param>
    /// <param name="value"> The input/output value. </param>
    /// <param name="onFlag"> The flag representing the 'Only Enabled' state. </param>
    /// <param name="offFlag"> The flag representing the 'Only Disabled' state. </param>
    /// <returns> True when <paramref name="value"/> changed in this frame. </returns>
    /// <remarks> Neither flag being on is treated the same as both being on. </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Draw<T>(ReadOnlySpan<byte> label, ref T value, T onFlag, T offFlag) where T : unmanaged, Enum
    {
        bool? converted = value.HasFlag(onFlag)
            ? value.HasFlag(offFlag)
                ? null
                : true
            : value.HasFlag(offFlag)
                ? false
                : null;
        if (!Draw(label, ref converted))
            return false;

        MergeEnum(ref value, onFlag, offFlag, converted);
        return true;
    }

    /// <param name="label"> The label for the checkbox as a UTF16 string. </param>
    /// <inheritdoc cref="Draw{T}(ReadOnlySpan{byte}, ref T, T, T)"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Draw<T>(ReadOnlySpan<char> label, ref T value, T onFlag, T offFlag) where T : unmanaged, Enum
        => Draw(label.Span<LabelStringHandlerBuffer>(), ref value, onFlag, offFlag);

    /// <param name="label"> The label for the checkbox as a formatted string. </param>
    /// <inheritdoc cref="Draw{T}(ReadOnlySpan{char}, ref T, T, T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Draw<T>(ref Utf8StringHandler<LabelStringHandlerBuffer> label, ref T value, T onFlag, T offFlag) where T : unmanaged, Enum
        => Draw(label.Span(), ref value, onFlag, offFlag);

    /// <inheritdoc/>
    protected override bool? NextValue(bool? value)
        => value switch
        {
            null  => true,
            true  => false,
            false => null,
        };

    /// <inheritdoc/>
    protected override bool? PreviousValue(bool? value)
        => value switch
        {
            null  => false,
            true  => null,
            false => true,
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static unsafe void MergeEnum<T>(ref T ret, T onFlag, T offFlag, bool? value) where T : unmanaged, Enum
    {
        var (uVal, uOn, uOff) = sizeof(T) switch
        {
            1 => (*(byte*)Unsafe.AsPointer(ref ret), *(byte*)&onFlag, *(byte*)&offFlag),
            2 => (*(ushort*)Unsafe.AsPointer(ref ret), *(ushort*)&onFlag, *(ushort*)&offFlag),
            4 => (*(uint*)Unsafe.AsPointer(ref ret), *(uint*)&onFlag, *(uint*)&offFlag),
            _ => throw new ArgumentException($"Enum type {typeof(T)} has size {sizeof(T)} > 4, which is not supported for flag checkboxes."),
        };

        uVal = value switch
        {
            null  => uVal | uOn | uOff,
            true  => (uVal | uOn) & ~uOff,
            false => (uVal | uOff) & ~uOn,
        };

        ret = *(T*)&uVal;
    }
}
