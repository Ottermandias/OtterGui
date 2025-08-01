using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using OtterGui.Text.HelperObjects;

namespace OtterGui.Text;

#pragma warning disable CS1573

// No format handlers for copied text and tooltip because they would be evaluated at the wrong time.
public static partial class ImUtf8
{
    /// <summary> Draw a selectable that copies a given text on click and displays the given tooltip. </summary>
    /// <param name="text"> The given text as a UTF8 string. HAS to be null-terminated.</param>
    /// <param name="copiedText"> The text to copy as a UTF8 string. HAS to be null-terminated. </param>
    /// <param name="tooltip"> The text to display on hover. Does not have to be null-terminated.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ReadOnlySpan<byte> text, ReadOnlySpan<byte> copiedText, ReadOnlySpan<byte> tooltip)
    {
        if (Selectable(text))
        {
            try
            {
                SetClipboardText(copiedText);
            }
            catch
            {
                // ignored
            }
        }

        HoverTooltip(tooltip);
    }

    /// <param name="text"> The given text as a UTF16 string. </param>
    /// <inheritdoc cref="CopyOnClickSelectable(ReadOnlySpan{byte},ReadOnlySpan{byte},ReadOnlySpan{byte})"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ReadOnlySpan<char> text, ReadOnlySpan<byte> copiedText, ReadOnlySpan<byte> tooltip)
        => CopyOnClickSelectable(text.Span<TextStringHandlerBuffer>(), copiedText, tooltip);

    /// <param name="text"> The given text as a formatted string. </param>
    /// <inheritdoc cref="CopyOnClickSelectable(ReadOnlySpan{char},ReadOnlySpan{byte},ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ref Utf8StringHandler<TextStringHandlerBuffer> text, ReadOnlySpan<byte> copiedText,
        ReadOnlySpan<byte> tooltip)
        => CopyOnClickSelectable(text.Span(), copiedText, tooltip);


    /// <param name="copiedText"> The text to copy as a UTF16 string. </param>
    /// <inheritdoc cref="CopyOnClickSelectable(ReadOnlySpan{byte},ReadOnlySpan{byte},ReadOnlySpan{byte})"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ReadOnlySpan<byte> text, ReadOnlySpan<char> copiedText, ReadOnlySpan<byte> tooltip)
    {
        if (Selectable(text))
        {
            try
            {
                SetClipboardText(copiedText);
            }
            catch
            {
                // ignored
            }
        }

        HoverTooltip(tooltip);
    }

    /// <param name="copiedText"> The text to copy as a UTF16 string. </param>
    /// <inheritdoc cref="CopyOnClickSelectable(ReadOnlySpan{char},ReadOnlySpan{byte},ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ReadOnlySpan<char> text, ReadOnlySpan<char> copiedText, ReadOnlySpan<byte> tooltip)
        => CopyOnClickSelectable(text.Span<TextStringHandlerBuffer>(), copiedText, tooltip);

    /// <param name="copiedText"> The text to copy as a UTF16 string. </param>
    /// <inheritdoc cref="CopyOnClickSelectable(ref Utf8StringHandler{TextStringHandlerBuffer},ReadOnlySpan{byte},ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ref Utf8StringHandler<TextStringHandlerBuffer> text, ReadOnlySpan<char> copiedText,
        ReadOnlySpan<byte> tooltip)
        => CopyOnClickSelectable(text.Span(), copiedText, tooltip);

    /// <param name="tooltip"> The text to display on hover. </param>
    /// <inheritdoc cref="CopyOnClickSelectable(ReadOnlySpan{byte},ReadOnlySpan{byte},ReadOnlySpan{byte})"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ReadOnlySpan<byte> text, ReadOnlySpan<byte> copiedText, ReadOnlySpan<char> tooltip)
    {
        if (Selectable(text))
        {
            try
            {
                SetClipboardText(copiedText);
            }
            catch
            {
                // ignored
            }
        }

        HoverTooltip(tooltip);
    }

    /// <param name="tooltip"> The text to display on hover. </param>
    /// <inheritdoc cref="CopyOnClickSelectable(ReadOnlySpan{char},ReadOnlySpan{byte},ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ReadOnlySpan<char> text, ReadOnlySpan<byte> copiedText, ReadOnlySpan<char> tooltip)
        => CopyOnClickSelectable(text.Span<TextStringHandlerBuffer>(), copiedText, tooltip);

    /// <inheritdoc cref="CopyOnClickSelectable(ref Utf8StringHandler{TextStringHandlerBuffer},ReadOnlySpan{byte},ReadOnlySpan{byte})"/>
    /// <param name="tooltip"> The text to display on hover. </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ref Utf8StringHandler<TextStringHandlerBuffer> text, ReadOnlySpan<byte> copiedText,
        ReadOnlySpan<char> tooltip)
        => CopyOnClickSelectable(text.Span(), copiedText, tooltip);


    /// <param name="tooltip"> The text to display on hover. </param>
    /// <inheritdoc cref="CopyOnClickSelectable(ReadOnlySpan{byte},ReadOnlySpan{char},ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ReadOnlySpan<byte> text, ReadOnlySpan<char> copiedText, ReadOnlySpan<char> tooltip)
    {
        if (Selectable(text))
        {
            try
            {
                SetClipboardText(copiedText);
            }
            catch
            {
                // ignored
            }
        }

        HoverTooltip(tooltip);
    }

    /// <param name="tooltip"> The text to display on hover. </param>
    /// <inheritdoc cref="CopyOnClickSelectable(ReadOnlySpan{char},ReadOnlySpan{char},ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ReadOnlySpan<char> text, ReadOnlySpan<char> copiedText, ReadOnlySpan<char> tooltip)
        => CopyOnClickSelectable(text.Span<TextStringHandlerBuffer>(), copiedText, tooltip);

    /// <param name="tooltip"> The text to display on hover. </param>
    /// <inheritdoc cref="CopyOnClickSelectable(ref Utf8StringHandler{TextStringHandlerBuffer},ReadOnlySpan{char},ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ref Utf8StringHandler<TextStringHandlerBuffer> text, ReadOnlySpan<char> copiedText,
        ReadOnlySpan<char> tooltip)
        => CopyOnClickSelectable(text.Span(), copiedText, tooltip);


    /// <inheritdoc cref="CopyOnClickSelectable(ReadOnlySpan{byte},ReadOnlySpan{byte},ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ReadOnlySpan<byte> text, ReadOnlySpan<byte> copiedText)
        => CopyOnClickSelectable(text, copiedText, "Click to copy to clipboard."u8);

    /// <inheritdoc cref="CopyOnClickSelectable(ReadOnlySpan{char},ReadOnlySpan{byte},ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ReadOnlySpan<char> text, ReadOnlySpan<byte> copiedText)
        => CopyOnClickSelectable(text, copiedText, "Click to copy to clipboard."u8);

    /// <inheritdoc cref="CopyOnClickSelectable(ref Utf8StringHandler{TextStringHandlerBuffer},ReadOnlySpan{byte},ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ref Utf8StringHandler<TextStringHandlerBuffer> text, ReadOnlySpan<byte> copiedText)
        => CopyOnClickSelectable(ref text, copiedText, "Click to copy to clipboard."u8);


    /// <inheritdoc cref="CopyOnClickSelectable(ReadOnlySpan{byte},ReadOnlySpan{char},ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ReadOnlySpan<byte> text, ReadOnlySpan<char> copiedText)
        => CopyOnClickSelectable(text, copiedText, "Click to copy to clipboard."u8);

    /// <inheritdoc cref="CopyOnClickSelectable(ReadOnlySpan{char},ReadOnlySpan{char},ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ReadOnlySpan<char> text, ReadOnlySpan<char> copiedText)
        => CopyOnClickSelectable(text, copiedText, "Click to copy to clipboard."u8);

    /// <inheritdoc cref="CopyOnClickSelectable(ref Utf8StringHandler{TextStringHandlerBuffer},ReadOnlySpan{char},ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ref Utf8StringHandler<TextStringHandlerBuffer> text, ReadOnlySpan<char> copiedText)
        => CopyOnClickSelectable(ref text, copiedText, "Click to copy to clipboard."u8);


    /// <inheritdoc cref="CopyOnClickSelectable(ReadOnlySpan{byte},ReadOnlySpan{byte},ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ReadOnlySpan<byte> text)
        => CopyOnClickSelectable(text, text, "Click to copy to clipboard."u8);

    /// <inheritdoc cref="CopyOnClickSelectable(ReadOnlySpan{char},ReadOnlySpan{byte},ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ReadOnlySpan<char> text)
    {
        var t = text.Span<TextStringHandlerBuffer>();
        if (Selectable(t))
        {
            try
            {
                SetClipboardText(t);
            }
            catch
            {
                // ignored
            }
        }

        HoverTooltip("Click to copy to clipboard."u8);
    }

    /// <inheritdoc cref="CopyOnClickSelectable(ref Utf8StringHandler{TextStringHandlerBuffer},ReadOnlySpan{byte},ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ref Utf8StringHandler<TextStringHandlerBuffer> text)
    {
        var t = text.Span();
        if (Selectable(t))
        {
            try
            {
                SetClipboardText(t);
            }
            catch
            {
                // ignored
            }
        }

        HoverTooltip("Click to copy to clipboard."u8);
    }

    /// <inheritdoc cref="CopyOnClickSelectable(ref Utf8StringHandler{TextStringHandlerBuffer},ReadOnlySpan{byte},ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyOnClickSelectable(ref Utf8StringHandler<TextStringHandlerBuffer> text, ImFontPtr font)
    {
        var t = text.Span();
        using (ImRaii.PushFont(font))
        {
            if (Selectable(t))
            {
                try
                {
                    SetClipboardText(t);
                }
                catch
                {
                    // ignored
                }
            }
        }

        HoverTooltip("Click to copy to clipboard."u8);
    }
}
