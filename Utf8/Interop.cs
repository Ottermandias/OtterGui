using Dalamud.Memory;

namespace OtterGui.Utf8;

public static unsafe partial class Interop
{
    /// <summary> Query information about <paramref name="address"/> from the OS. </summary>
    /// <returns> True on success. </returns>
    public static bool VirtualQuery(nint address, out MemoryBasicInformation info)
    {
        fixed (MemoryBasicInformation* ptr = &info)
        {
            return VirtualQuery(address, ptr, sizeof(MemoryBasicInformation)) is not 0;
        }
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial nint VirtualQuery(nint lpAddress, MemoryBasicInformation* lpBuffer, int dwLength);

    [StructLayout(LayoutKind.Sequential)]
    public struct MemoryBasicInformation
    {
        public nint             BaseAddress;
        public nint             AllocationBase;
        public MemoryProtection AllocationProtect;
        public ushort           PartitionId;
        public nint             RegionSize;
        public MemoryState      State;
        public MemoryProtection Protect;
        public MemoryType       Type;
    }

    [Flags]
    public enum MemoryState : uint
    {
        Commit  = 0x1000,
        Reserve = 0x2000,
        Free    = 0x10000,
    }

    [Flags]
    public enum MemoryType : uint
    {
        Private = 0x20000,
        Mapped  = 0x40000,
        Image   = 0x1000000,
    }
}
