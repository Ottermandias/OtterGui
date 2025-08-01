using Dalamud.Bindings.ImGui;

namespace OtterGui.Text.EndObjects;

public ref struct ClipRect
{
    private int _count;

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        while (_count-- > 0)
            ImGui.PopClipRect();
    }

    public ClipRect Push(Vector2 minimum, Vector2 maximum, bool push = true, bool intersectWithCurrent = false)
    {
        if (push)
        {
            ImGui.PushClipRect(minimum, maximum, intersectWithCurrent);
            ++_count;
        }

        return this;
    }

    public ClipRect PushSize(Vector2 minimum, Vector2 size, bool push = true, bool intersectWithCurrent = false)
    {
        if (push)
        {
            ImGui.PushClipRect(minimum, minimum + size, intersectWithCurrent);
            ++_count;
        }

        return this;
    }

    public ClipRect Pop(int count = 1)
    {
        count = Math.Min(count, _count);
        for (var i = 0; i < count; ++i)
            ImGui.PopClipRect();
        _count -= count;
        return this;
    }
}
