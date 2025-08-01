using Dalamud.Bindings.ImGui;
using OtterGui.Text.HelperObjects;

namespace OtterGui.Text;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

public static partial class ImUtf8
{
    /// <summary> Draw the same text multiple times at the cursor position to simulate a shadowed text. </summary>
    /// <param name="text"> The given text as a UTF8 string. Does not need to be null-terminated. </param>
    /// <param name="foregroundColor"> The center text color. </param>
    /// <param name="shadowColor"> The shadow color. </param>
    /// <param name="shadowWidth"> The width of the shadow in pixels. </param>
    public static void TextShadowed(ReadOnlySpan<byte> text, uint foregroundColor, uint shadowColor, byte shadowWidth = 1)
    {
        var x = ImGui.GetCursorPosX();
        var y = ImGui.GetCursorPosY();

        for (var i = -shadowWidth; i <= shadowWidth; i++)
        {
            for (var j = -shadowWidth; j <= shadowWidth; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                ImGui.SetCursorPos(new Vector2(x + i, y + j));
                Text(text, shadowColor);
            }
        }

        ImGui.SetCursorPos(new Vector2(x, y));
        Text(text, foregroundColor);
    }

    /// <param name="text"> The given text as a UTF16 string. </param>
    /// <inheritdoc cref="TextShadowed(ReadOnlySpan{byte}, uint, uint, byte)"/>
    /// <exception cref="ImUtf8FormatException" />
    public static void TextShadowed(ReadOnlySpan<char> text, uint foregroundColor, uint shadowColor, byte shadowWidth = 1)
        => TextShadowed(text.Span<TextStringHandlerBuffer>(), foregroundColor, shadowColor, shadowWidth);

    /// <param name="text"> The given text as a formatted string. </param>
    /// <inheritdoc cref="TextShadowed(ReadOnlySpan{char}, uint, uint, byte)"/>
    public static void TextShadowed(ref Utf8StringHandler<TextStringHandlerBuffer> text, uint foregroundColor, uint shadowColor,
        byte shadowWidth = 1)
        => TextShadowed(text.Span(), foregroundColor, shadowColor, shadowWidth);


    /// <summary> Draw the same text multiple times at the given position in a draw list to simulate a shadowed text. </summary>
    /// <param name="drawList"> The draw list. </param>
    /// <param name="position"> The position to draw the text at. </param>
    /// <param name="text"> The text to draw. </param>
    /// <param name="foregroundColor"> The center text color. </param>
    /// <param name="shadowColor"> The shadow color. </param>
    /// <param name="shadowWidth"> The width of the shadow in pixels. </param>
    public static void TextShadowed(ImDrawListPtr drawList, Vector2 position, ReadOnlySpan<byte> text, uint foregroundColor, uint shadowColor,
        byte shadowWidth = 1)
    {
        for (var i = -shadowWidth; i <= shadowWidth; i++)
        {
            for (var j = -shadowWidth; j <= shadowWidth; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                drawList.AddText(text, position + new Vector2(i, j), shadowColor);
            }
        }

        drawList.AddText(text, position, foregroundColor);
    }

    /// <param name="text"> The given text as a UTF16 string. </param>
    /// <inheritdoc cref="TextShadowed(ReadOnlySpan{byte}, uint, uint, byte)"/>
    /// <exception cref="ImUtf8FormatException" />
    public static void TextShadowed(ImDrawListPtr drawList, Vector2 position, ReadOnlySpan<char> text, uint foregroundColor, uint shadowColor,
        byte shadowWidth = 1)
        => TextShadowed(drawList, position, text.Span<TextStringHandlerBuffer>(), foregroundColor, shadowColor, shadowWidth);

    /// <param name="text"> The given text as a formatted string. </param>
    /// <inheritdoc cref="TextShadowed(ReadOnlySpan{char}, uint, uint, byte)"/>
    public static void TextShadowed(ImDrawListPtr drawList, Vector2 position, ref Utf8StringHandler<TextStringHandlerBuffer> text,
        uint foregroundColor, uint shadowColor, byte shadowWidth = 1)
        => TextShadowed(drawList, position, text.Span(), foregroundColor, shadowColor, shadowWidth);
}
