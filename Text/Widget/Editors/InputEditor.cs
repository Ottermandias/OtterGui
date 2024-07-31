using ImGuiNET;

namespace OtterGui.Text.Widget.Editors;

public sealed class InputEditor<T>(T? minimum, T? maximum, T step, T stepFast, Editors.FormatBuffer format, ImGuiInputTextFlags flags) : IEditor<T>
    where T : unmanaged, INumber<T>
{
    private readonly Editors.FormatBuffer _format = format;

    public static InputEditor<T> CreateInteger(T? minimum, T? maximum, T step, T stepFast, bool hex, scoped ReadOnlySpan<byte> unit, ImGuiInputTextFlags flags)
        => new(minimum, maximum, step, stepFast, Editors.GenerateIntegerFormat<T>(hex, unit), flags | (hex ? ImGuiInputTextFlags.CharsHexadecimal : 0));

    public static InputEditor<T> CreateInteger(T? minimum, T? maximum, T step, T stepFast, bool hex, scoped ReadOnlySpan<char> unit, ImGuiInputTextFlags flags)
        => new(minimum, maximum, step, stepFast, Editors.GenerateIntegerFormat<T>(hex, unit), flags | (hex ? ImGuiInputTextFlags.CharsHexadecimal : 0));

    public static InputEditor<T> CreateFloat(T? minimum, T? maximum, T step, T stepFast, byte precision, scoped ReadOnlySpan<byte> unit, ImGuiInputTextFlags flags)
        => new(minimum, maximum, step, stepFast, Editors.GenerateFloatFormat<T>(precision, unit), flags);

    public static InputEditor<T> CreateFloat(T? minimum, T? maximum, T step, T stepFast, byte precision, scoped ReadOnlySpan<char> unit, ImGuiInputTextFlags flags)
        => new(minimum, maximum, step, stepFast, Editors.GenerateFloatFormat<T>(precision, unit), flags);

    public unsafe bool Draw(Span<T> values, bool disabled)
    {
        var helper = Editors.PrepareMultiComponent(values.Length);
        var ret    = false;

        for (var valueIdx = 0; valueIdx < values.Length; ++valueIdx)
        {
            helper.SetupComponent(valueIdx);

            if (disabled)
            {
                var value = values[valueIdx];
                ImUtf8.InputScalar(helper.Id, ref value, _format, step, stepFast, flags | ImGuiInputTextFlags.ReadOnly);
            }
            else
            {
                if (ImUtf8.InputScalar(helper.Id, ref values[valueIdx], _format, step, stepFast, flags))
                {
                    if (minimum.HasValue && values[valueIdx] < minimum.Value)
                        values[valueIdx] = minimum.Value;
                    else if (maximum.HasValue && values[valueIdx] > maximum.Value)
                        values[valueIdx] = maximum.Value;
                    ret = true;
                }
            }
        }

        return ret;
    }
}
