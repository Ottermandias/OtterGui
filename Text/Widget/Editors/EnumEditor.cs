using Dalamud.Bindings.ImGui;
using OtterGui.Raii;
using OtterGui.Text.HelperObjects;

namespace OtterGui.Text.Widget.Editors;

public sealed class EnumEditor<T>(IReadOnlyList<(ReadOnlyMemory<byte> Label, T Value, ReadOnlyMemory<byte> Description)> domain) : IEditor<T>
    where T : unmanaged, IUtf8SpanFormattable, IEqualityOperators<T, T, bool>
{
    public bool Draw(Span<T> values, bool disabled)
    {
        var helper = Editors.PrepareMultiComponent(values.Length);
        var ret    = false;

        for (var valueIdx = 0; valueIdx < values.Length; ++valueIdx)
        {
            using var id = ImRaii.PushId(valueIdx);
            helper.SetupComponent(valueIdx);

            var currentValue = values[valueIdx];
            var labelLength  = 0;
            var valueFound   = false;
            foreach (var v in domain)
            {
                if (v.Value == currentValue)
                {
                    v.Label.Span.CopyInto<TextStringHandlerBuffer>();
                    labelLength = v.Label.Length;
                    valueFound  = true;
                }
            }
            if (!valueFound)
            {
                var writer = new SpanTextWriter(TextStringHandlerBuffer.Span);
                writer.Append(currentValue, default, CultureInfo.CurrentCulture);
                writer.EnsureNullTerminated();
                labelLength = writer.Position;
            }
            ret = disabled
                ? ImUtf8.InputText(default(ReadOnlySpan<byte>), TextStringHandlerBuffer.Span[..labelLength], out TerminatedByteString _, flags: ImGuiInputTextFlags.ReadOnly)
                : DrawCombo(TextStringHandlerBuffer.Span[..labelLength], ref values[valueIdx]);
        }

        return ret;
    }

    private bool DrawCombo(ReadOnlySpan<byte> preview, ref T currentValue)
    {
        using var c = ImUtf8.Combo(default(ReadOnlySpan<byte>), preview);
        if (!c)
            return false;

        var ret = false;
        foreach (var (valueLabel, value, valueDescription) in domain)
        {
            if (ImUtf8.Selectable(valueLabel.Span, value == currentValue))
            {
                currentValue = value;
                ret          = true;
            }

            if (valueDescription.Length > 0)
                ImUtf8.SelectableHelpMarker(valueDescription.Span);
        }

        return ret;
    }
}
