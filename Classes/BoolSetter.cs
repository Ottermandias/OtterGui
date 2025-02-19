namespace OtterGui.Classes;

public readonly ref struct BoolSetter
{
    private readonly ref bool _value;

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public BoolSetter(ref bool value, bool setTo = true)
    {
        _value = ref value;
        _value = setTo;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        _value = false;
    }
}
