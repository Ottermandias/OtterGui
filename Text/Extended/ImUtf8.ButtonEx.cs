using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using OtterGui.Text.HelperObjects;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

// ReSharper disable MethodOverloadWithOptionalParameter

namespace OtterGui.Text;

public static partial class ImUtf8
{
    /// <summary> Draw an icon button of frame height and width. </summary>
    /// <param name="label"> The icon to draw. </param>
    /// <param name="tooltip"> A tooltip to show when hovering the button as a UTF8 string. Does not have to be null-terminated. Nothing will be displayed for an empty span. </param>
    /// <param name="size">
    /// The desired size for the button. If (0, 0), it will fit to the label.<br/>
    /// You can pass <paramref name="size"/>.x = float.MinValue to span the available width.
    /// </param>
    /// <param name="disabled"> Whether the button should be disabled. </param>
    /// <param name="textColor"> The text color inside the button. No color is pushed for 0. </param>
    /// <param name="buttonColor"> The background color inside the button. No color is pushed for 0. </param>
    /// <returns> Whether the button was clicked in this frame. </returns>
    public static bool ButtonEx(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip = default, Vector2 size = default, bool disabled = false,
        uint textColor = 0, uint buttonColor = 0)
    {
        bool ret;
        using (ImRaii.PushColor(ImGuiCol.Text, textColor, textColor != 0)
                   .Push(ImGuiCol.Button, buttonColor, buttonColor != 0))
        {
            using var _ = ImRaii.Disabled(disabled);
            ret = Button(label, size);
        }

        HoverTooltip(tooltip, ImGuiHoveredFlags.AllowWhenDisabled);
        return ret;
    }

    /// <param name="label"> The button label as a UTF16 string. </param>
    /// <inheritdoc cref="ButtonEx(ReadOnlySpan{byte},ReadOnlySpan{byte},Vector2,bool,uint,uint)"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ButtonEx(ReadOnlySpan<char> label, ReadOnlySpan<byte> tooltip = default, Vector2 size = default, bool disabled = false,
        uint textColor = 0, uint buttonColor = 0)
        => ButtonEx(label.Span<LabelStringHandlerBuffer>(), tooltip, size, disabled, textColor, buttonColor);

    /// <param name="label"> The button label as a formatted string. </param>
    /// <inheritdoc cref="ButtonEx(ReadOnlySpan{char},ReadOnlySpan{byte},Vector2,bool,uint,uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ButtonEx(ref Utf8StringHandler<LabelStringHandlerBuffer> label, ReadOnlySpan<byte> tooltip = default,
        Vector2 size = default, bool disabled = false,
        uint textColor = 0, uint buttonColor = 0)
        => ButtonEx(label.Span(), tooltip, size, disabled, textColor, buttonColor);


    /// <param name="tooltip"> A tooltip to show when hovering the button as a UTF16 string. Nothing will be displayed for an empty span. </param>
    /// <inheritdoc cref="ButtonEx(ReadOnlySpan{byte},ReadOnlySpan{byte},Vector2,bool,uint,uint)"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ButtonEx(ReadOnlySpan<byte> label, ReadOnlySpan<char> tooltip, Vector2 size = default, bool disabled = false,
        uint textColor = 0, uint buttonColor = 0)
        => ButtonEx(label, tooltip.Span<TextStringHandlerBuffer>(), size, disabled, textColor, buttonColor);

    /// <param name="tooltip"> A tooltip to show when hovering the button as a UTF16 string. Nothing will be displayed for an empty span. </param>
    /// <inheritdoc cref="ButtonEx(ReadOnlySpan{char},ReadOnlySpan{byte},Vector2,bool,uint,uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ButtonEx(ReadOnlySpan<char> label, ReadOnlySpan<char> tooltip, Vector2 size = default, bool disabled = false,
        uint textColor = 0, uint buttonColor = 0)
        => ButtonEx(label.Span<LabelStringHandlerBuffer>(), tooltip.Span<TextStringHandlerBuffer>(), size, disabled, textColor, buttonColor);

    /// <param name="tooltip"> A tooltip to show when hovering the button as a UTF16 string. Nothing will be displayed for an empty span. </param>
    /// <inheritdoc cref="ButtonEx(ref Utf8StringHandler{LabelStringHandlerBuffer},ReadOnlySpan{byte},Vector2,bool,uint,uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ButtonEx(ref Utf8StringHandler<LabelStringHandlerBuffer> label, ReadOnlySpan<char> tooltip, Vector2 size = default,
        bool disabled = false,
        uint textColor = 0, uint buttonColor = 0)
        => ButtonEx(label.Span(), tooltip.Span<TextStringHandlerBuffer>(), size, disabled, textColor, buttonColor);


    /// <param name="tooltip"> A tooltip to show when hovering the button as a formatted string. Nothing will be displayed for an empty span. </param>
    /// <inheritdoc cref="ButtonEx(ReadOnlySpan{byte},ReadOnlySpan{char},Vector2,bool,uint,uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ButtonEx(ReadOnlySpan<byte> label, ref Utf8StringHandler<TextStringHandlerBuffer> tooltip, Vector2 size = default,
        bool disabled = false,
        uint textColor = 0, uint buttonColor = 0)
        => ButtonEx(label, tooltip.Span(), size, disabled, textColor, buttonColor);

    /// <param name="tooltip"> A tooltip to show when hovering the button as a formatted string. Nothing will be displayed for an empty span. </param>
    /// <inheritdoc cref="ButtonEx(ReadOnlySpan{char},ReadOnlySpan{char},Vector2,bool,uint,uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ButtonEx(ReadOnlySpan<char> label, ref Utf8StringHandler<TextStringHandlerBuffer> tooltip, Vector2 size = default,
        bool disabled = false,
        uint textColor = 0, uint buttonColor = 0)
        => ButtonEx(label.Span<LabelStringHandlerBuffer>(), tooltip.Span(), size, disabled, textColor, buttonColor);

    /// <param name="tooltip"> A tooltip to show when hovering the button as a formatted string. Nothing will be displayed for an empty span. </param>
    /// <inheritdoc cref="ButtonEx(ref Utf8StringHandler{LabelStringHandlerBuffer},ReadOnlySpan{char},Vector2,bool,uint,uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ButtonEx(ref Utf8StringHandler<LabelStringHandlerBuffer> label, ref Utf8StringHandler<TextStringHandlerBuffer> tooltip,
        Vector2 size = default, bool disabled = false,
        uint textColor = 0, uint buttonColor = 0)
        => ButtonEx(label.Span(), tooltip.Span(), size, disabled, textColor, buttonColor);
}
