using OtterGui.Text.EndObjects;

namespace OtterGui.Text;

public static partial class ImUtf8
{
    public static ListClipper ListClipper(int itemsCount, float itemsHeight = -1f)
        => new(itemsCount, itemsHeight);
}
