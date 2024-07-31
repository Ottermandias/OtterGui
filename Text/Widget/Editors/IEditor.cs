using ImGuiNET;

namespace OtterGui.Text.Widget.Editors;

/// <summary>
/// Represents a pre-configured component that allows to display and/or edit a value, or an array of contiguous values, stored in a region of memory.
/// </summary>
/// <typeparam name="T"> The type of the values stored in the memory region. </typeparam>
public interface IEditor<T> where T : unmanaged
{
    /// <summary> Draws the pre-configured component. </summary>
    /// <param name="values"> The memory region to display and/or edit. </param>
    /// <param name="disabled"> Whether to draw the component in read-only mode. </param>
    /// <returns> Whether the contents of the memory region changed in this frame. </returns>
    /// <remarks>
    /// ID handling is outside the scope of this function - use <see cref="ImUtf8.PushId(ReadOnlySpan{byte})"/> (or other overloads).<para/>
    /// Label drawing is also out of scope - use <see cref="ImUtf8.SameLineInner"/> and <see cref="ImUtf8.Text(ReadOnlySpan{byte})"/>, or whatever you want.<para/>
    /// This function honors <see cref="ImGui.SetNextItemWidth(float)"/>.
    /// </remarks>
    bool Draw(Span<T> values, bool disabled);

    /// <summary>
    /// Presents this editor as an editor for another stored type. Analogous to <see cref="MemoryMarshal.Cast{TFrom, TTo}(Span{TFrom})"/>.
    /// </summary>
    /// <typeparam name="TStored"> The wanted stored type. </typeparam>
    /// <returns> This editor, presented as an editor for the wanted type. </returns>
    /// <remarks>
    /// Examples of use cases:
    /// <list type="bullet">
    /// <item>Preparing an editor of a high-level type for use over <see cref="byte"/> buffers.</item>
    /// <item>Turning an editor of a container type, such as <see cref="Vector3"/>, into an editor of its underlying type, such as <see cref="float"/>, and conversely.</item>
    /// </list>
    /// </remarks>
    /// <seealso cref="MemoryMarshal.Cast{TFrom, TTo}(Span{TFrom})"/>
    IEditor<TStored> Reinterpreting<TStored>() where TStored : unmanaged
        => typeof(TStored) == typeof(T) ? (IEditor<TStored>)this : new ReinterpretingEditor<TStored, T>(this);

    /// <summary>
    /// Creates an editor for another underlying representation of the values.
    /// </summary>
    /// <typeparam name="TStored"> The type of the underlying representation of the values. </typeparam>
    /// <param name="convert"> A conversion from the values stored in memory to the values that shall be displayed. </param>
    /// <param name="convertBack"> A conversion from the values entered by the user to the values to store in memory. </param>
    /// <returns> The wrapped editor. </returns>
    /// <remarks>
    /// Examples of use cases:
    /// <list type="bullet">
    /// <item>Applying a factor and/or a bias to values, for example to present a degree editor for an angle stored in radians.</item>
    /// <item>Presenting an integer editor for values that are stored as floating-point numbers but where only integer values make sense.</item>
    /// </list>
    /// </remarks>
    sealed IEditor<TStored> Converting<TStored>(Func<TStored, T> convert, Func<T, TStored> convertBack) where TStored : unmanaged
        => new ConvertingEditor<TStored, T>(this, convert, convertBack);
}
