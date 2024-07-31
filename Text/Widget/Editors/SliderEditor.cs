using ImGuiNET;

namespace OtterGui.Text.Widget.Editors;

public sealed class SliderEditor<T>(T minimum, T maximum, Editors.FormatBuffer format, ImGuiSliderFlags flags) : IEditor<T>
    where T : unmanaged, INumber<T>
{
    private readonly Editors.FormatBuffer _format = format;

    public static SliderEditor<T> CreateInteger(T minimum, T maximum, scoped ReadOnlySpan<byte> unit, ImGuiSliderFlags flags)
        => new(minimum, maximum, Editors.GenerateIntegerFormat<T>(false, unit), flags);

    public static SliderEditor<T> CreateInteger(T minimum, T maximum, scoped ReadOnlySpan<char> unit, ImGuiSliderFlags flags)
        => new(minimum, maximum, Editors.GenerateIntegerFormat<T>(false, unit), flags);

    public static SliderEditor<T> CreateFloat(T minimum, T maximum, byte precision, scoped ReadOnlySpan<byte> unit, ImGuiSliderFlags flags)
        => new(minimum, maximum, Editors.GenerateFloatFormat<T>(precision, unit), flags);

    public static SliderEditor<T> CreateFloat(T minimum, T maximum, byte precision, scoped ReadOnlySpan<char> unit, ImGuiSliderFlags flags)
        => new(minimum, maximum, Editors.GenerateFloatFormat<T>(precision, unit), flags);

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
                ImUtf8.Slider(helper.Id, ref value, _format, minimum, maximum, flags | ImGuiSliderFlags.AlwaysClamp);
            }
            else
            {
                if (ImUtf8.Slider(helper.Id, ref values[valueIdx], _format, minimum, maximum, flags | ImGuiSliderFlags.AlwaysClamp))
                {
                    if (values[valueIdx] < minimum)
                        values[valueIdx] = minimum;
                    else if (values[valueIdx] > maximum)
                        values[valueIdx] = maximum;
                    ret = true;
                }
            }
        }

        return ret;
    }
}
