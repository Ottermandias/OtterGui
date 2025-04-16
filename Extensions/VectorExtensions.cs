namespace OtterGui.Extensions;

public static class VectorExtensions
{
    /// <summary> Return a new vector with both values in <paramref name="x"/> rounded up. </summary>
    public static Vector2 Ceiling(this Vector2 x)
        => new(MathF.Ceiling(x.X), MathF.Ceiling(x.Y));

    /// <summary> Return a new vector with both values in <paramref name="x"/> rounded down. </summary>
    public static Vector2 Floor(this Vector2 x)
        => new(MathF.Floor(x.X), MathF.Floor(x.Y));

    /// <summary> Return a new vector with both values in <paramref name="x"/> rounded. </summary>
    public static Vector2 Round(this Vector2 x)
        => new(MathF.Round(x.X), MathF.Round(x.Y));
}
