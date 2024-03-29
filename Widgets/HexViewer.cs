using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace OtterGui.Widgets;

public static partial class Widget
{
    private static ReadOnlySpan<byte> HexBytes
        => "0123456789ABCDEF"u8;

    public static unsafe void DrawHexViewer(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0)
            return;

        var font = UiBuilder.MonoFont;
        using var _ = ImRaii.PushFont(font);
        var emWidth = font.GetCharAdvance('m');

        var addressDigitCount = 8 - (BitOperations.LeadingZeroCount((uint)data.Length - 1) >> 2);
        var addressFormat = CompositeFormat.Parse($"{{0:X{addressDigitCount}}}:");
        var charsPerRow = (int)MathF.Floor(ImGui.GetContentRegionAvail().X / emWidth);
        var bytesPerRow = (charsPerRow - addressDigitCount - 2) / 4;
        bytesPerRow = 1 << BitOperations.Log2((uint)bytesPerRow);
        var capacity = addressDigitCount + 2 + 4 * bytesPerRow;

        var buffer = stackalloc byte[capacity];
        for (var rowAddress = 0; rowAddress < data.Length; rowAddress += bytesPerRow)
        {
            var bufferI = 0;
            for (var i = addressDigitCount; i-- > 0;)
                buffer[bufferI++] = HexBytes[(rowAddress >> (i << 2)) & 0xF];
            buffer[bufferI++] = (byte)':';

            for (var i = rowAddress; i < data.Length && i < rowAddress + bytesPerRow; i++)
            {
                buffer[bufferI++] = (byte)' ';
                var @byte = data[i];
                buffer[bufferI++] = HexBytes[@byte >> 4];
                buffer[bufferI++] = HexBytes[@byte & 0xF];
            }

            for (var i = data.Length; i < rowAddress + bytesPerRow; i++)
            {
                buffer[bufferI++] = (byte)' ';
                buffer[bufferI++] = (byte)' ';
                buffer[bufferI++] = (byte)' ';
            }

            buffer[bufferI++] = (byte)' ';

            for (var i = rowAddress; i < data.Length && i < rowAddress + bytesPerRow; i++)
            {
                var @byte = data[i];
                buffer[bufferI++] = @byte is >= 32 and < 127 ? @byte : (byte)'.';
            }

            ImGuiNative.igTextUnformatted(buffer, buffer + bufferI);
        }
    }
}
