using ImGuiNET;
using OtterGui.Text.HelperObjects;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

namespace OtterGui.Text;

public static unsafe partial class ImUtf8
{
    /// <summary> Prepare a header for a table cell. </summary>
    /// <param name="label"> The header label as a UTF8 string. HAS to be null-terminated. </param>
    /// <param name="widthOrWeight"> The initial fixed width in pixels or weight in parts of total weight, depending on mode. </param>
    /// <param name="userId"> Unknown. </param>
    /// <remarks> This does not draw a header, but using <seealso cref="ImGui.TableHeadersRow"/> will create a row with the given labels. </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TableSetupColumn(ReadOnlySpan<byte> label, ImGuiTableColumnFlags flags = ImGuiTableColumnFlags.None,
        float widthOrWeight = 0, uint userId = 0)
        => ImGuiNative.igTableSetupColumn(label.Start(), flags, widthOrWeight, userId);

    /// <param name="label"> The header label as a UTF16 string. </param>
    /// <inheritdoc cref="TableSetupColumn(ReadOnlySpan{byte},ImGuiTableColumnFlags,float,uint)"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TableSetupColumn(ReadOnlySpan<char> label, ImGuiTableColumnFlags flags = ImGuiTableColumnFlags.None,
        float widthOrWeight = 0, uint userId = 0)
        => TableSetupColumn(label.Span<LabelStringHandlerBuffer>(), flags, widthOrWeight, userId);

    /// <param name="label"> The header label as a formatted string. </param>
    /// <inheritdoc cref="TableSetupColumn(ReadOnlySpan{char},ImGuiTableColumnFlags,float,uint))"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TableSetupColumn(ref Utf8StringHandler<LabelStringHandlerBuffer> label,
        ImGuiTableColumnFlags flags = ImGuiTableColumnFlags.None, float widthOrWeight = 0, uint userId = 0)
        => TableSetupColumn(label.Span(), flags, widthOrWeight, userId);
}
