using ImGuiNET;

namespace OtterGui.Text.Widget.Editors;

public sealed class DragEditor<T>(T? minimum, T? maximum, float speed, float relativeSpeed, Editors.FormatBuffer format, ImGuiSliderFlags flags) : IEditor<T>
    where T : unmanaged, INumber<T>
{
    private readonly Editors.FormatBuffer _format = format;

    public static DragEditor<T> CreateInteger(T? minimum, T? maximum, float speed, float relativeSpeed, scoped ReadOnlySpan<byte> unit, ImGuiSliderFlags flags)
        => new(minimum, maximum, speed, relativeSpeed, Editors.GenerateIntegerFormat<T>(false, unit), flags);

    public static DragEditor<T> CreateInteger(T? minimum, T? maximum, float speed, float relativeSpeed, scoped ReadOnlySpan<char> unit, ImGuiSliderFlags flags)
        => new(minimum, maximum, speed, relativeSpeed, Editors.GenerateIntegerFormat<T>(false, unit), flags);

    public static DragEditor<T> CreateFloat(T? minimum, T? maximum, float speed, float relativeSpeed, byte precision, scoped ReadOnlySpan<byte> unit, ImGuiSliderFlags flags)
        => new(minimum, maximum, speed, relativeSpeed, Editors.GenerateFloatFormat<T>(precision, unit), flags);

    public static DragEditor<T> CreateFloat(T? minimum, T? maximum, float speed, float relativeSpeed, byte precision, scoped ReadOnlySpan<char> unit, ImGuiSliderFlags flags)
        => new(minimum, maximum, speed, relativeSpeed, Editors.GenerateFloatFormat<T>(precision, unit), flags);

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
                ImUtf8.DragScalar(helper.Id, ref value, _format, value, value, 0.0f, flags);
            }
            else
            {
                if (ImUtf8.DragScalar(helper.Id, ref values[valueIdx], _format, minimum, maximum, CalculateSpeed(values[valueIdx]), flags))
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

    public float CalculateSpeed(T value)
        => Math.Max(speed, Math.Abs(float.CreateSaturating(value)) * relativeSpeed);
}
