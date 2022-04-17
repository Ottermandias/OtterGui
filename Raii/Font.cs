using System;
using ImGuiNET;

namespace OtterGui.Raii;

// Push an arbitrary amount of fonts into an object that are all popped when it is disposed.
// If condition is false, no font is pushed.
public static partial class ImRaii
{
    public static Font PushFont(ImFontPtr font, bool condition = true)
        => condition ? new Font().Push(font) : new Font();

    public sealed class Font : IDisposable
    {
        private int _count;

        public Font()
            => _count = 0;

        public Font Push(ImFontPtr font)
        {
            ImGui.PushFont(font);
            ++_count;
            return this;
        }

        public void Pop(int num = 1)
        {
            num    =  Math.Min(num, _count);
            _count -= num;
            while (num-- > 0)
                ImGui.PopFont();
        }

        public void Dispose()
            => Pop(_count);
    }
}
