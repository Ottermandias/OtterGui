using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

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

    // Return a human readable form of the size using the given format (which should be a float identifier followed by a placeholder).
    public static string HumanReadableSize(long size, string format = "{0:0.#} {1}")
    {
        var    order = 0;
        double s     = size;
        while (s >= 1024 && order < ByteAbbreviations.Length - 1)
        {
            order++;
            s /= 1024;
        }

        return string.Format(format, s, ByteAbbreviations[order]);
    }

    private static readonly string[] ByteAbbreviations =
    {
        "B",
        "KB",
        "MB",
        "GB",
        "TB",
        "PB",
        "EB",
    };

    // Compress any type to a base64 encoding of its compressed json representation, prepended with a version byte.
    // Returns an empty string on failure.
    public static unsafe string ToCompressedBase64<T>(T data, byte version)
    {
        try
        {
            var       json             = JsonConvert.SerializeObject(data, Formatting.None);
            var       bytes            = Encoding.UTF8.GetBytes(json);
            using var compressedStream = new MemoryStream();
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(new ReadOnlySpan<byte>(&version, 1));
                zipStream.Write(bytes, 0, bytes.Length);
            }

            return Convert.ToBase64String(compressedStream.ToArray());
        }
        catch
        {
            return string.Empty;
        }
    }

    // Decompress a base64 encoded string to the given type and a prepended version byte if possible.
    // On failure, data will be default and version will be byte.MaxValue.
    public static byte FromCompressedBase64<T>(string base64, out T? data)
    {
        var version = byte.MaxValue;
        try
        {
            var       bytes            = Convert.FromBase64String(base64);
            using var compressedStream = new MemoryStream(bytes);
            using var zipStream        = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var resultStream     = new MemoryStream();
            zipStream.CopyTo(resultStream);
            bytes   = resultStream.ToArray();
            version = bytes[0];
            var json = Encoding.UTF8.GetString(bytes, 1, bytes.Length - 1);
            data = JsonConvert.DeserializeObject<T>(json);
        }
        catch
        {
            data = default;
        }

        return version;
    }
}
