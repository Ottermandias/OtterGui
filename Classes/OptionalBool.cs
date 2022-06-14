using System;

namespace OtterGui.Classes;

public readonly struct OptionalBool : IEquatable<OptionalBool>, IEquatable<bool?>, IEquatable<bool>
{
    private readonly byte _value;

    public static readonly OptionalBool True  = new(true);
    public static readonly OptionalBool False = new(false);
    public static readonly OptionalBool Null  = new();

    public OptionalBool()
        => _value = byte.MaxValue;

    public OptionalBool(bool? value)
        => _value = (byte)(value == null ? byte.MaxValue : value.Value ? 1 : 0);

    public bool? Value
        => _value switch
        {
            1 => true,
            0 => false,
            _ => null,
        };

    public static implicit operator OptionalBool(bool? v)
        => new(v);

    public static implicit operator OptionalBool(bool v)
        => new(v);

    public static implicit operator bool?(OptionalBool v)
        => v.Value;

    public bool Equals(OptionalBool other)
        => _value == other._value;

    public bool Equals(bool? other)
        => _value switch
        {
            1 when other != null => other.Value,
            0 when other != null => !other.Value,
            _ when other == null => true,
            _                    => false,
        };

    public bool Equals(bool other)
        => other ? _value == 1 : _value == 0;

    public override string ToString()
        => _value switch
        {
            1 => true.ToString(),
            0 => false.ToString(),
            _ => "null",
        };
}
