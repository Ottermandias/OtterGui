using Dalamud.Bindings.ImGui;

namespace OtterGui.Text.Widget.Editors;

public sealed class ColorEditor : IEditor<float>, IEditor<Vector3>, IEditor<Vector4>
{
    public static readonly ColorEditor StandardDynamicRange = new(false);
    public static readonly ColorEditor HighDynamicRange     = new(true);

    private readonly bool _hdr;

    private ColorEditor(bool hdr)
    {
        _hdr = hdr;
    }

    public static ColorEditor Get(bool hdr)
        => hdr ? HighDynamicRange : StandardDynamicRange;

    public bool Draw(Span<float> values, bool disabled)
    {
        switch (values.Length)
        {
            case 3:
                return Draw(MemoryMarshal.Cast<float, Vector3>(values), disabled);
            case 4:
                return Draw(MemoryMarshal.Cast<float, Vector4>(values), disabled);
            default: return Editors.DefaultFloat.Draw(values, disabled);
        }
    }

    public bool Draw(Span<Vector3> values, bool disabled)
    {
        if (values.Length != 1)
            return Editors.DefaultFloat.Draw(MemoryMarshal.Cast<Vector3, float>(values), disabled);

        ref var value = ref values[0];
        var previous = value;
        if (!ImUtf8.ColorEdit("###color"u8, ref value, ImGuiColorEditFlags.Float | (_hdr ? ImGuiColorEditFlags.HDR : 0)))
            return false;
        if (disabled)
        {
            value = previous;
            return false;
        }
        if (!_hdr)
            value = Vector3.Clamp(value, Vector3.Zero, Vector3.One);
        return true;
    }

    public bool Draw(Span<Vector4> values, bool disabled)
    {
        if (values.Length != 1)
            return Editors.DefaultFloat.Draw(MemoryMarshal.Cast<Vector4, float>(values), disabled);

        ref var value = ref values[0];
        var previous = value;
        if (!ImUtf8.ColorEdit("###color"u8, ref value,
                ImGuiColorEditFlags.Float | ImGuiColorEditFlags.AlphaPreviewHalf | (_hdr ? ImGuiColorEditFlags.HDR : 0)))
            return false;
        if (disabled)
        {
            value = previous;
            return false;
        }
        if (!_hdr)
            value = Vector4.Clamp(value, Vector4.Zero, Vector4.One);
        return true;
    }

    public IEditor<TStored> Reinterpreting<TStored>() where TStored : unmanaged
    {
        if (typeof(TStored) == typeof(float) || typeof(TStored) == typeof(Vector3) || typeof(TStored) == typeof(Vector4))
            return (IEditor<TStored>)(object)this;

        return new ReinterpretingEditor<TStored, float>(this);
    }
}
