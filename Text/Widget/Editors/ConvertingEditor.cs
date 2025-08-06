using OtterGuiInternal.Utility;

namespace OtterGui.Text.Widget.Editors;

internal sealed class ConvertingEditor<TStored, TEditable>(IEditor<TEditable> inner, Func<TStored, TEditable> convert, Func<TEditable, TStored> convertBack) : IEditor<TStored>
    where TStored : unmanaged where TEditable : unmanaged
{
    public unsafe bool Draw(Span<TStored> values, bool disabled)
    {
        // Allocation strategy borrowed from Dalamud.Bindings.ImGui string conversion logic.
        Span<TEditable> converted = values.Length <= StringHelpers.MaxStackAlloc / sizeof(TEditable)
            ? stackalloc TEditable[values.Length]
            : new TEditable[values.Length];

        for (var i = 0; i < values.Length; ++i)
            converted[i] = convert(values[i]);

        if (!inner.Draw(converted, disabled))
            return false;

        for (var i = 0; i < values.Length; ++i)
            values[i] = convertBack(converted[i]);

        return true;
    }
}
