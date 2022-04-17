namespace OtterGui;

public static class Functions
{
    // Split a uint into four bytes, e.g. for RGBA colors.
    public static (byte Lowest, byte Second, byte Third, byte Highest) SplitBytes(uint value)
    {
        var byte4 = (byte)(value >> 24);
        var byte3 = (byte)(value >> 16);
        var byte2 = (byte)(value >> 8);
        var byte1 = (byte)value;
        return (byte1, byte2, byte3, byte4);
    }

    // Obtain a descriptive hex-string of a RGBA color.
    public static string ColorBytes(uint color)
    {
        var (r, g, b, a) = SplitBytes(color);
        return $"#{r:X2}{g:X2}{b:X2}{a:X2}";
    }

    // Reorder a ABGR color to RGBA.
    public static uint ReorderColor(uint seColor)
    {
        var (a, b, g, r) = SplitBytes(seColor);
        return r | ((uint)g << 8) | ((uint)b << 16) | ((uint)a << 24);
    }

    // Average two given colors.
    public static uint AverageColor(uint c1, uint c2)
    {
        var (r1, g1, b1, a1) = SplitBytes(c1);
        var (r2, g2, b2, a2) = SplitBytes(c2);
        var r = (uint)(r1 + r2) / 2;
        var g = (uint)(g1 + g2) / 2;
        var b = (uint)(b1 + b2) / 2;
        var a = (uint)(a1 + a2) / 2;
        return r | (g << 8) | (b << 16) | (a << 24);
    }
}
