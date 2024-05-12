using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using OtterGui.Text.HelperObjects;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

// ReSharper disable MethodOverloadWithOptionalParameter

namespace OtterGui.Text;

public static partial class ImUtf8
{
    /// <summary> Draw an icon button of frame height and width. </summary>
    /// <param name="icon"> The icon to draw. </param>
    /// <param name="tooltip"> A tooltip to show when hovering the button as a UTF8 string. Does not have to be null-terminated. Nothing will be displayed for an empty span. </param>
    /// <param name="disabled"> Whether the button should be disabled. </param>
    /// <param name="textColor"> The text color inside the button. No color is pushed for 0. </param>
    /// <param name="buttonColor"> The background color inside the button. No color is pushed for 0. </param>
    /// <returns> Whether the button was clicked in this frame. </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IconButton(FontAwesomeIcon icon, ReadOnlySpan<byte> tooltip, bool disabled = false, uint textColor = 0,
        uint buttonColor = 0)
    {
        var  size = new Vector2(FrameHeight);
        bool ret;
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            using var color = ImRaii.PushColor(ImGuiCol.Text, textColor, textColor != 0)
                .Push(ImGuiCol.Button, buttonColor, buttonColor != 0);
            using var _ = ImRaii.Disabled(disabled);
            ret = Button(icon.Bytes().Span, size);
        }

        HoverTooltip(tooltip, ImGuiHoveredFlags.AllowWhenDisabled);
        return ret;
    }

    /// <param name="tooltip"> A tooltip to show when hovering the button as a UTF16 string. Nothing will be displayed for an empty span. </param>
    /// <inheritdoc cref="IconButton(FontAwesomeIcon,ReadOnlySpan{byte},bool,uint,uint)"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IconButton(FontAwesomeIcon icon, ReadOnlySpan<char> tooltip, bool disabled = false, uint textColor = 0,
        uint buttonColor = 0)
        => IconButton(icon, tooltip.Span<TextStringHandlerBuffer>(), disabled, textColor, buttonColor);

    /// <param name="tooltip"> A tooltip to show when hovering the button as a formatted string. Nothing will be displayed for an empty span. </param>
    /// <inheritdoc cref="IconButton(FontAwesomeIcon,ReadOnlySpan{char},bool,uint,uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IconButton(FontAwesomeIcon icon, ref Utf8StringHandler<TextStringHandlerBuffer> tooltip, bool disabled = false,
        uint textColor = 0, uint buttonColor = 0)
        => IconButton(icon, tooltip.Span(), disabled, textColor, buttonColor);


    /// <inheritdoc cref="IconButton(FontAwesomeIcon,ReadOnlySpan{byte},bool,uint,uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IconButton(FontAwesomeIcon icon, ReadOnlySpan<byte> tooltip)
    {
        var  size = new Vector2(FrameHeight);
        bool ret;
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            ret = Button(icon.Bytes().Span, size);
        }

        HoverTooltip(tooltip, ImGuiHoveredFlags.AllowWhenDisabled);
        return ret;
    }

    /// <param name="tooltip"> A tooltip to show when hovering the button as a formatted string. Nothing will be displayed for an empty span. </param>
    /// <inheritdoc cref="IconButton(FontAwesomeIcon,ReadOnlySpan{char},bool,uint,uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IconButton(FontAwesomeIcon icon, ReadOnlySpan<char> tooltip)
        => IconButton(icon, tooltip.Span<TextStringHandlerBuffer>());

    /// <param name="tooltip"> A tooltip to show when hovering the button as a UTF16 string. Nothing will be displayed for an empty span. </param>
    /// <inheritdoc cref="IconButton(FontAwesomeIcon,ref Utf8StringHandler{TextStringHandlerBuffer},bool,uint,uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IconButton(FontAwesomeIcon icon, ref Utf8StringHandler<TextStringHandlerBuffer> tooltip)
        => IconButton(icon, tooltip.Span());


    /// <inheritdoc cref="IconButton(FontAwesomeIcon,ReadOnlySpan{byte},bool,uint,uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IconButton(FontAwesomeIcon icon)
    {
        var size = new Vector2(FrameHeight);
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            return Button(icon.Bytes().Span, size);
        }
    }

    /// <inheritdoc cref="IconButton(FontAwesomeIcon,ReadOnlySpan{byte},bool,uint,uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IconButton(FontAwesomeIcon icon, bool disabled)
    {
        using var dis = ImRaii.Disabled(disabled);
        return IconButton(icon);
    }

    /// <inheritdoc cref="IconButton(FontAwesomeIcon,ReadOnlySpan{byte},bool,uint,uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IconButton(FontAwesomeIcon icon, bool disabled, uint textColor = 0, uint buttonColor = 0)
    {
        using var color = ImRaii.PushColor(ImGuiCol.Text, textColor, textColor != 0)
            .Push(ImGuiCol.Button, buttonColor, buttonColor != 0);
        return IconButton(icon, disabled);
    }
}

internal static class FontAwesomeExtensions
{
    internal unsafe struct IconBuffer
    {
        private ulong _buffer;

        public IconBuffer(FontAwesomeIcon icon)
        {
            var   iconChar = icon.ToIconChar();
            ulong tmp      = 0;
            Encoding.UTF8.GetBytes(&iconChar, 1, (byte*)&tmp, 8);
            _buffer = tmp;
        }

        public ReadOnlySpan<byte> Span
        {
            get
            {
                fixed (ulong* ptr = &_buffer)
                {
                    return new Span<byte>(ptr, 8);
                }
            }
        }
    }

    internal static IconBuffer Bytes(this FontAwesomeIcon icon)
        => new(icon);
}
