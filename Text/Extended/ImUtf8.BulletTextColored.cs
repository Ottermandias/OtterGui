using Dalamud.Bindings.ImGui;
using OtterGui.Text.HelperObjects;

namespace OtterGui.Text;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

public static partial class ImUtf8
{
    /// <summary> Draw a colored, text after a bullet-point. </summary>
    /// <param name="color"> The color to use for the text, but not the bullet. </param>
    /// <param name="text"> The given text as a UTF8 string. Does not have to be null-terminated. </param>
    public static void BulletTextColored(uint color, ReadOnlySpan<byte> text)
    {
        using var g = Group();
        ImGui.Bullet();
        ImGui.SameLine();
        Text(text, color);
    }

    /// <param name="text"> The given text as a UTF16 string. </param>
    /// <inheritdoc cref="BulletTextColored(uint,ReadOnlySpan{byte})"/>
    /// <exception cref="ImUtf8FormatException" />
    public static void BulletTextColored(uint color, ReadOnlySpan<char> text)
        => BulletTextColored(color, text.Span<TextStringHandlerBuffer>());

    /// <param name="text"> The given text as a formatted string. </param>
    /// <inheritdoc cref="BulletTextColored(uint,ReadOnlySpan{char})"/>
    public static void BulletTextColored(uint color, ref Utf8StringHandler<TextStringHandlerBuffer> text)
        => BulletTextColored(color, text.Span());
}
