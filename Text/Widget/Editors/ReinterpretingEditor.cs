namespace OtterGui.Text.Widget.Editors;

internal sealed class ReinterpretingEditor<TStored, TEditable>(IEditor<TEditable> inner) : IEditor<TStored>
    where TStored : unmanaged where TEditable : unmanaged
{
    private readonly IEditor<TEditable> _inner = inner;

    public bool Draw(Span<TStored> values, bool disabled)
        => _inner.Draw(MemoryMarshal.Cast<TStored, TEditable>(values), disabled);

    public IEditor<TStoredNew> Reinterpreting<TStoredNew>() where TStoredNew : unmanaged
        => _inner.Reinterpreting<TStoredNew>();
}
