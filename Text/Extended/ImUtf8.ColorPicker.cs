using ImGuiNET;
using OtterGui.Text.HelperObjects;

namespace OtterGui.Text;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

public static partial class ImUtf8
{
    /// <summary> Make a single-button color picker with a contrasted letter centered on it. </summary>
    /// <param name="label"> The label of the color picker as a UTF8 string. HAS to be null-terminated. </param>
    /// <param name="tooltip"> A tooltip when hovering the color picker as a UTF8 string. Does not have to be null-terminated. </param>
    /// <param name="input"> The input color value. </param>
    /// <param name="setter"> The color setter. </param>
    /// <param name="letter"> The letter to superimpose on the picker as a UTF8 string. Does not have to be null-terminated. Can be empty. </param>
    /// <returns> When the color was changed this frame. </returns>
    public static bool ColorPicker(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip, Vector3 input, Action<Vector3> setter,
        ReadOnlySpan<byte> letter = default)
    {
        var ret = false;
        if (ColorEdit(label, ref input,
                ImGuiColorEditFlags.NoInputs
              | ImGuiColorEditFlags.DisplayRGB
              | ImGuiColorEditFlags.InputRGB
              | ImGuiColorEditFlags.NoTooltip
              | ImGuiColorEditFlags.HDR))
        {
            setter(input);
            ret = true;
        }

        if (letter.Length > 0 && ImGui.IsItemVisible())
        {
            var textSize  = CalcTextSize(letter);
            var center    = ImGui.GetItemRectMin() + (ImGui.GetItemRectSize() - textSize) / 2;
            var textColor = ImGuiUtil.ContrastColorBw(new Vector4(input, 0.7f));
            ImGui.GetWindowDrawList().AddText(letter, center, ImGui.ColorConvertFloat4ToU32(textColor));
        }

        HoverTooltip(tooltip);

        return ret;
    }

    /// <param name="label"> The label of the color picker as a UTF16 string. </param>
    /// <inheritdoc cref="ColorPicker(ReadOnlySpan{byte},ReadOnlySpan{byte},Vector3,Action{Vector3},ReadOnlySpan{byte})"/>
    /// <exception cref="ImUtf8FormatException" />
    public static bool ColorPicker(ReadOnlySpan<char> label, ReadOnlySpan<byte> tooltip, Vector3 input, Action<Vector3> setter,
        ReadOnlySpan<byte> letter = default)
        => ColorPicker(label.Span<LabelStringHandlerBuffer>(), tooltip, input, setter, letter);

    /// <param name="label"> The label of the color picker as a formatted string. </param>
    /// <inheritdoc cref="ColorPicker(ReadOnlySpan{char},ReadOnlySpan{byte},Vector3,Action{Vector3},ReadOnlySpan{byte})"/>
    public static bool ColorPicker(ref Utf8StringHandler<LabelStringHandlerBuffer> label, ReadOnlySpan<byte> tooltip, Vector3 input,
        Action<Vector3> setter, ReadOnlySpan<byte> letter = default)
        => ColorPicker(label.Span(), tooltip, input, setter, letter);


    /// <param name="letter"> The letter to superimpose on the picker as a UTF16 string. </param>
    /// <inheritdoc cref="ColorPicker(ReadOnlySpan{byte},ReadOnlySpan{byte},Vector3,Action{Vector3},ReadOnlySpan{byte})"/>
    /// <exception cref="ImUtf8FormatException" />
    public static bool ColorPicker(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip, Vector3 input, Action<Vector3> setter,
        ReadOnlySpan<char> letter)
        => ColorPicker(label, tooltip, input, setter, letter.Span<HintStringHandlerBuffer>());

    /// <param name="letter"> The letter to superimpose on the picker as a UTF16 string. </param>
    /// <inheritdoc cref="ColorPicker(ReadOnlySpan{char},ReadOnlySpan{byte},Vector3,Action{Vector3},ReadOnlySpan{byte})"/>
    public static bool ColorPicker(ReadOnlySpan<char> label, ReadOnlySpan<byte> tooltip, Vector3 input, Action<Vector3> setter,
        ReadOnlySpan<char> letter)
        => ColorPicker(label, tooltip, input, setter, letter.Span<HintStringHandlerBuffer>());

    /// <param name="letter"> The letter to superimpose on the picker as a UTF16 string. </param>
    /// <inheritdoc cref="ColorPicker(ref Utf8StringHandler{LabelStringHandlerBuffer},ReadOnlySpan{byte},Vector3,Action{Vector3},ReadOnlySpan{byte})"/>
    public static bool ColorPicker(ref Utf8StringHandler<LabelStringHandlerBuffer> label, ReadOnlySpan<byte> tooltip, Vector3 input,
        Action<Vector3> setter, ReadOnlySpan<char> letter)
        => ColorPicker(ref label, tooltip, input, setter, letter.Span<HintStringHandlerBuffer>());


    /// <param name="letter"> The letter to superimpose on the picker as a formatted string. </param>
    /// <inheritdoc cref="ColorPicker(ReadOnlySpan{byte},ReadOnlySpan{byte},Vector3,Action{Vector3},ReadOnlySpan{byte})"/>
    /// <exception cref="ImUtf8FormatException" />
    public static bool ColorPicker(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip, Vector3 input, Action<Vector3> setter,
        ref Utf8StringHandler<HintStringHandlerBuffer> letter)
        => ColorPicker(label, tooltip, input, setter, letter.Span());

    /// <param name="letter"> The letter to superimpose on the picker as a formatted string. </param>
    /// <inheritdoc cref="ColorPicker(ReadOnlySpan{char},ReadOnlySpan{byte},Vector3,Action{Vector3},ReadOnlySpan{byte})"/>
    public static bool ColorPicker(ReadOnlySpan<char> label, ReadOnlySpan<byte> tooltip, Vector3 input, Action<Vector3> setter,
        ref Utf8StringHandler<HintStringHandlerBuffer> letter)
        => ColorPicker(label, tooltip, input, setter, letter.Span());

    /// <param name="letter"> The letter to superimpose on the picker as a formatted string. </param>
    /// <inheritdoc cref="ColorPicker(ref Utf8StringHandler{LabelStringHandlerBuffer},ReadOnlySpan{byte},Vector3,Action{Vector3},ReadOnlySpan{byte})"/>
    public static bool ColorPicker(ref Utf8StringHandler<LabelStringHandlerBuffer> label, ReadOnlySpan<byte> tooltip, Vector3 input,
        Action<Vector3> setter, ref Utf8StringHandler<HintStringHandlerBuffer> letter)
        => ColorPicker(label.Span(), tooltip, input, setter, letter.Span());


    /// <param name="tooltip"> A tooltip when hovering the color picker as a UTF16 string. </param>
    /// <inheritdoc cref="ColorPicker(ReadOnlySpan{byte},ReadOnlySpan{byte},Vector3,Action{Vector3},ReadOnlySpan{byte})"/>
    /// <exception cref="ImUtf8FormatException" />
    public static bool ColorPicker(ReadOnlySpan<byte> label, ReadOnlySpan<char> tooltip, Vector3 input, Action<Vector3> setter,
        ReadOnlySpan<byte> letter)
        => ColorPicker(label, tooltip.Span<TextStringHandlerBuffer>(), input, setter, letter);

    /// <param name="tooltip"> A tooltip when hovering the color picker as a UTF16 string. </param>
    /// <inheritdoc cref="ColorPicker(ReadOnlySpan{char},ReadOnlySpan{byte},Vector3,Action{Vector3},ReadOnlySpan{byte})"/>
    public static bool ColorPicker(ReadOnlySpan<char> label, ReadOnlySpan<char> tooltip, Vector3 input, Action<Vector3> setter,
        ReadOnlySpan<byte> letter = default)
        => ColorPicker(label, tooltip.Span<TextStringHandlerBuffer>(), input, setter, letter);

    /// <param name="tooltip"> A tooltip when hovering the color picker as a UTF16 string. </param>
    /// <inheritdoc cref="ColorPicker(ref Utf8StringHandler{LabelStringHandlerBuffer},ReadOnlySpan{byte},Vector3,Action{Vector3},ReadOnlySpan{byte})"/>
    public static bool ColorPicker(ref Utf8StringHandler<LabelStringHandlerBuffer> label, ReadOnlySpan<char> tooltip, Vector3 input,
        Action<Vector3> setter, ReadOnlySpan<byte> letter = default)
        => ColorPicker(ref label, tooltip.Span<TextStringHandlerBuffer>(), input, setter, letter);


    /// <param name="tooltip"> A tooltip when hovering the color picker as a UTF16 string. </param>
    /// <inheritdoc cref="ColorPicker(ReadOnlySpan{byte},ReadOnlySpan{byte},Vector3,Action{Vector3},ReadOnlySpan{char})"/>
    public static bool ColorPicker(ReadOnlySpan<byte> label, ReadOnlySpan<char> tooltip, Vector3 input, Action<Vector3> setter,
        ReadOnlySpan<char> letter)
        => ColorPicker(label, tooltip.Span<TextStringHandlerBuffer>(), input, setter, letter);

    /// <param name="tooltip"> A tooltip when hovering the color picker as a UTF16 string. </param>
    /// <inheritdoc cref="ColorPicker(ReadOnlySpan{char},ReadOnlySpan{byte},Vector3,Action{Vector3},ReadOnlySpan{char})"/>
    public static bool ColorPicker(ReadOnlySpan<char> label, ReadOnlySpan<char> tooltip, Vector3 input, Action<Vector3> setter,
        ReadOnlySpan<char> letter)
        => ColorPicker(label, tooltip.Span<TextStringHandlerBuffer>(), input, setter, letter);

    /// <param name="tooltip"> A tooltip when hovering the color picker as a UTF16 string. </param>
    /// <inheritdoc cref="ColorPicker(ref Utf8StringHandler{LabelStringHandlerBuffer},ReadOnlySpan{byte},Vector3,Action{Vector3},ReadOnlySpan{char})"/>
    public static bool ColorPicker(ref Utf8StringHandler<LabelStringHandlerBuffer> label, ReadOnlySpan<char> tooltip, Vector3 input,
        Action<Vector3> setter, ReadOnlySpan<char> letter)
        => ColorPicker(ref label, tooltip.Span<TextStringHandlerBuffer>(), input, setter, letter);


    /// <param name="tooltip"> A tooltip when hovering the color picker as a UTF16 string. </param>
    /// <inheritdoc cref="ColorPicker(ReadOnlySpan{byte},ReadOnlySpan{byte},Vector3,Action{Vector3},ref Utf8StringHandler{HintStringHandlerBuffer})"/>
    public static bool ColorPicker(ReadOnlySpan<byte> label, ReadOnlySpan<char> tooltip, Vector3 input, Action<Vector3> setter,
        ref Utf8StringHandler<HintStringHandlerBuffer> letter)
        => ColorPicker(label, tooltip.Span<TextStringHandlerBuffer>(), input, setter, ref letter);

    /// <param name="tooltip"> A tooltip when hovering the color picker as a UTF16 string. </param>
    /// <inheritdoc cref="ColorPicker(ReadOnlySpan{char},ReadOnlySpan{byte},Vector3,Action{Vector3},ref Utf8StringHandler{HintStringHandlerBuffer})"/>
    public static bool ColorPicker(ReadOnlySpan<char> label, ReadOnlySpan<char> tooltip, Vector3 input, Action<Vector3> setter,
        ref Utf8StringHandler<HintStringHandlerBuffer> letter)
        => ColorPicker(label, tooltip.Span<TextStringHandlerBuffer>(), input, setter, ref letter);

    /// <param name="tooltip"> A tooltip when hovering the color picker as a UTF16 string. </param>
    /// <inheritdoc cref="ColorPicker(ref Utf8StringHandler{LabelStringHandlerBuffer},ReadOnlySpan{byte},Vector3,Action{Vector3},ref Utf8StringHandler{HintStringHandlerBuffer})"/>
    public static bool ColorPicker(ref Utf8StringHandler<LabelStringHandlerBuffer> label, ReadOnlySpan<char> tooltip, Vector3 input,
        Action<Vector3> setter, ref Utf8StringHandler<HintStringHandlerBuffer> letter)
        => ColorPicker(ref label, tooltip.Span<TextStringHandlerBuffer>(), input, setter, ref letter);
}
