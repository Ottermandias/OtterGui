namespace OtterGui.Filesystem;

public partial class FileSystem<T>
{
    public sealed class Leaf : IWritePath
    {
        public T Value { get; }

        internal Leaf(Folder parent, string name, T value, uint identifier)
        {
            Parent = parent;
            Value  = value;
            SetName(name);
            Identifier = identifier;
        }

        public string FullName()
            => IPath.BaseFullName(this);

        public override string ToString()
            => FullName();

        public Folder    Parent        { get; internal set; }
        public string    Name          { get; private set; } = string.Empty;
        public uint      Identifier    { get; }
        public ushort    IndexInParent { get; internal set; }
        public byte      Depth         { get; internal set; }
        public PathFlags Flags         { get; private set; }

        public bool IsLocked
            => Flags.HasFlag(PathFlags.Locked);


        void IWritePath.SetParent(Folder parent)
            => Parent = parent;

        internal void SetName(string name, bool fix = true)
            => Name = fix ? name.FixName() : name;

        public void SetLocked(bool value)
            => Flags = value ? Flags | PathFlags.Locked : Flags & ~PathFlags.Locked;

        void IWritePath.SetName(string name, bool fix)
            => SetName(name, fix);

        void IWritePath.UpdateDepth()
            => Depth = unchecked((byte)(Parent.Depth + 1));

        void IWritePath.UpdateIndex(int index)
        {
            if (index < 0)
                index = Parent.Children.IndexOf(this);
            IndexInParent = (ushort)(index < 0 ? 0 : index);
        }
    }
}
