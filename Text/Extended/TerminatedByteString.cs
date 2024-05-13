namespace OtterGui.Text;

/// <summary> A null-terminated UTF8 string. </summary>
public readonly struct TerminatedByteString
{
    private static readonly byte[]               EmptyArray = [0];
    public static readonly  TerminatedByteString Empty      = new(EmptyArray);

    public TerminatedByteString()
        => _text = EmptyArray;

    internal TerminatedByteString(byte[] text)
        => _text = text;

    private readonly byte[] _text;

    public IReadOnlyList<byte> TextWithNull
        => _text;

    public int Length
        => _text.Length - 1;

    public int Count
        => Length;

    public bool IsEmpty
        => _text.Length <= 1;

    public static implicit operator ReadOnlySpan<byte>(TerminatedByteString text)
        => text._text.AsSpan(^1);
}
