using Dalamud.Bindings.ImGui;
using OtterGui.Text.HelperObjects;

namespace OtterGui.Text;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

public static partial class ImUtf8
{
    /// <summary> Go to the next table column and draw text. </summary>
    /// <param name="text"> The given text as a UTF8 string. Does not have to be null-terminated. </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void DrawTableColumn(ReadOnlySpan<byte> text)
    {
        ImGui.TableNextColumn();
        Text(text);
    }

    /// <param name="text"> The given text as a UTF16 string. </param>
    /// <inheritdoc cref="DrawTableColumn(ReadOnlySpan{byte})"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void DrawTableColumn(ReadOnlySpan<char> text)
    {
        ImGui.TableNextColumn();
        Text(text);
    }

    /// <param name="text"> The given text as a formatted string. </param>
    /// <inheritdoc cref="DrawTableColumn(ReadOnlySpan{char})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void DrawTableColumn(ref Utf8StringHandler<TextStringHandlerBuffer> text)
    {
        ImGui.TableNextColumn();
        Text(ref text);
    }


    /// <summary> Go to the next table column and draw frame-aligned text. </summary>
    /// <param name="text"> The given text as a UTF8 string. Does not have to be null-terminated. </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void DrawFrameColumn(ReadOnlySpan<byte> text)
    {
        ImGui.TableNextColumn();
        TextFrameAligned(text);
    }

    /// <param name="text"> The given text as a UTF16 string. </param>
    /// <inheritdoc cref="DrawTableColumn(ReadOnlySpan{byte})"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void DrawFrameColumn(ReadOnlySpan<char> text)
    {
        ImGui.TableNextColumn();
        TextFrameAligned(text);
    }

    /// <param name="text"> The given text as a formatted string. </param>
    /// <inheritdoc cref="DrawTableColumn(ReadOnlySpan{char})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void DrawFrameColumn(ref Utf8StringHandler<TextStringHandlerBuffer> text)
    {
        ImGui.TableNextColumn();
        TextFrameAligned(ref text);
    }
}
