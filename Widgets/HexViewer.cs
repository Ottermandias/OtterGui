using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace OtterGui.Widgets;

public static partial class Widget
{
    private static readonly CompositeFormat ByteHexFormat = CompositeFormat.Parse(" {0:X2}");

    public static void DrawHexViewer(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0)
            return;

        var font = UiBuilder.MonoFont;
        using var _ = ImRaii.PushFont(font);
        var emWidth = font.GetCharAdvance('m');

        var addressDigitCount = 8 - (BitOperations.LeadingZeroCount((uint)data.Length - 1) >> 2);
        var addressFormat = CompositeFormat.Parse($"{{0:X{addressDigitCount}}}:");
        var charsPerRow = (int)MathF.Floor(ImGui.GetContentRegionAvail().X / emWidth);
        var bytesPerRow = (charsPerRow - addressDigitCount - 1) / 3;
        bytesPerRow = 1 << BitOperations.Log2((uint)bytesPerRow);

        var builder = new StringBuilder();
        for (var rowAddress = 0; rowAddress < data.Length; rowAddress += bytesPerRow)
        {
            builder.Clear();

            builder.AppendFormat(null, addressFormat, rowAddress);
            for (var i = rowAddress; i < data.Length && i < rowAddress + bytesPerRow; i++)
                builder.AppendFormat(null, ByteHexFormat, data[i]);

            ImGui.TextUnformatted(builder.ToString());
        }
    }
}
