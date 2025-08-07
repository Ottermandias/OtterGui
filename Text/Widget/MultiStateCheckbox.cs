using Dalamud.Bindings.ImGui;
using OtterGui.Text.HelperObjects;
using OtterGuiInternal;
using OtterGuiInternal.Structs;
using OtterGuiInternal.Utility;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

namespace OtterGui.Text.Widget;

/// <summary>
/// Draw a checkbox that toggles forward or backward between different states.
/// </summary>
public abstract class MultiStateCheckbox<T>
{
    /// <summary> Render the symbol corresponding to <paramref name="value"/> starting at <paramref name="position"/> and with <paramref name="size"/> as box size. </summary>
    protected abstract void RenderSymbol(T value, Vector2 position, float size);

    /// <summary> Increment the value. </summary>
    protected abstract T NextValue(T value);

    /// <summary> Decrement the value. </summary>
    protected abstract T PreviousValue(T value);

    /// <summary> Draw the multi state checkbox. </summary>
    /// <param name="label"> The label for the checkbox as a UTF8 string. HAS to be null-terminated. </param>
    /// <param name="value"> The input/output value. </param>
    /// <returns> True when <paramref name="value"/> changed in this frame. </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Draw(ReadOnlySpan<byte> label, ref T value)
        => Draw(label, value, out value);

    /// <param name="label"> The label for the checkbox as a UTF16 string. </param>
    /// <inheritdoc cref="Draw(ReadOnlySpan{byte},ref T)"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Draw(ReadOnlySpan<char> label, ref T value)
        => Draw(label.Span<LabelStringHandlerBuffer>(), value, out value);

    /// <param name="label"> The label for the checkbox as a formatted string. </param>
    /// <inheritdoc cref="Draw(ReadOnlySpan{char},ref T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Draw(ref Utf8StringHandler<LabelStringHandlerBuffer> label, ref T value)
        => Draw(label.Span(), ref value);

    /// <summary> Draw the multi state checkbox. </summary>
    /// <param name="label"> The label for the checkbox as a UTF8 string. HAS to be null-terminated. </param>
    /// <param name="currentValue"> The input value. </param>
    /// <param name="newValue"> The output value. </param>
    /// <returns> True when this was toggled this frame and a new value is returned. </returns>
    public bool Draw(ReadOnlySpan<byte> label, T currentValue, out T newValue)
    {
        newValue = currentValue;
        var window = ImGuiInternal.GetCurrentWindow();
        if (window.SkipItems)
            return false;

        var (visibleEnd, labelSize, id) = StringHelpers.ComputeSizeAndId(label);
        // Calculate the bounding box of the checkbox including the label.
        var squareSize  = ImGui.GetFrameHeight();
        var style       = ImGui.GetStyle();
        var screenPos   = window.DC.CursorPos;
        var itemSize    = new Vector2(squareSize + (labelSize.X > 0 ? style.ItemInnerSpacing.X + labelSize.X : 0), squareSize);
        var boundingBox = new ImRect(screenPos, screenPos + itemSize);

        // Add the item to internals. Skip it if it is clipped.
        ImGuiInternal.ItemSize(itemSize, style.FramePadding.Y);
        if (!ImGuiInternal.ItemAdd(boundingBox, id))
            return false;

        // Handle user interaction.
        var returnValue = ImGuiInternal.ButtonBehavior(boundingBox, id, out var hovered, out var held);
        var rightClick  = ImGui.IsItemClicked(ImGuiMouseButton.Right);
        if (rightClick)
            newValue = PreviousValue(currentValue);
        else if (returnValue)
            newValue = NextValue(currentValue);

        // Draw the checkbox.
        var checkBoundingBox = new ImRect(screenPos, screenPos + new Vector2(squareSize));
        ImGuiInternal.RenderNavHighlight(boundingBox, id);
        ImGuiInternal.RenderFrame(checkBoundingBox.Min, checkBoundingBox.Max, ColorHelpers.GetFrameBg(hovered, held), true,
            style.FrameRounding);

        // Draw the desired symbol into the checkbox.
        var paddingSize = Math.Max(1, (int)(squareSize / 6));
        RenderSymbol(currentValue, screenPos + new Vector2(paddingSize), squareSize - paddingSize * 2);

        // Add the label if there is one visible.
        if (labelSize.X > 0)
        {
            var labelPos = new Vector2(checkBoundingBox.Max.X + style.ItemInnerSpacing.X, checkBoundingBox.Min.Y + style.FramePadding.Y);
            StringHelpers.AddText(ImGui.GetWindowDrawList(), labelPos, ImGui.GetColorU32(ImGuiCol.Text), label[..visibleEnd], false);
        }

        return returnValue || rightClick;
    }

    /// <param name="label"> The label for the checkbox as a UTF16 string. </param>
    /// <inheritdoc cref="Draw(ReadOnlySpan{byte},T, out T)"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Draw(ReadOnlySpan<char> label, T currentValue, out T newValue)
        => Draw(label.Span<LabelStringHandlerBuffer>(), currentValue, out newValue);

    /// <param name="label"> The label for the checkbox as a formatted string. </param>
    /// <inheritdoc cref="Draw(ReadOnlySpan{char},T, out T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Draw(ref Utf8StringHandler<LabelStringHandlerBuffer> label, T value, out T newValue)
        => Draw(label.Span(), value, out newValue);
}
