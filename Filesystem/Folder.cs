namespace OtterGui.Filesystem;

public partial class FileSystem<T>
{
    public class Folder(Folder parent, string name, uint identifier) : IWritePath
    {
        internal const byte RootDepth = byte.MaxValue;

        // The folder internally keeps track of total descendants and total leaves.
        public int TotalDescendants { get; internal set; }
        public int TotalLeaves      { get; internal set; }

        internal readonly List<IWritePath> Children = [];

        public int TotalChildren
            => Children.Count;

        public Folder    Parent        { get; internal set; } = parent;
        public string    Name          { get; private set; }  = name.FixName();
        public uint      Identifier    { get; }               = identifier;
        public ushort    IndexInParent { get; internal set; }
        public byte      Depth         { get; internal set; }
        public PathFlags Flags         { get; private set; }

        public bool IsRoot
            => Depth == RootDepth;

        public bool IsLocked
            => Flags.HasFlag(PathFlags.Locked);

        void IWritePath.SetParent(Folder parent)
            => Parent = parent;

        void IWritePath.SetName(string name, bool fix)
            => Name = fix ? name.FixName() : name;

        public void SetLocked(bool value)
            => Flags = value ? Flags | PathFlags.Locked : Flags & ~PathFlags.Locked;

        void IWritePath.UpdateDepth()
        {
            var oldDepth = Depth;
            Depth = unchecked((byte)(Parent.Depth + 1));
            if (Depth == oldDepth)
                return;

            foreach (var desc in GetWriteDescendants())
                desc.UpdateDepth();
        }

        void IWritePath.UpdateIndex(int index)
        {
            if (index < 0)
                index = Parent.Children.IndexOf(this);
            IndexInParent = (ushort)(index < 0 ? 0 : index);
        }

        public IEnumerable<Folder> GetSubFolders()
            => Children.OfType<Folder>();

        public IEnumerable<Leaf> GetLeaves()
            => Children.OfType<Leaf>();


        // Iterate through all direct children in sort order.
        public IEnumerable<IPath> GetChildren(ISortMode<T> mode)
            => mode.GetChildren(this);

        // Iterate through all Descendants in sort order, not including the folder itself.
        public IEnumerable<IPath> GetAllDescendants(ISortMode<T> mode)
        {
            return GetChildren(mode).SelectMany(p => p is Folder f
                ? f.GetAllDescendants(mode).Prepend(f)
                : Array.Empty<IPath>().Append(p));
        }

        internal IEnumerable<IWritePath> GetWriteDescendants()
        {
            return Children.SelectMany(p => p is Folder f
                ? f.GetWriteDescendants().Prepend(f)
                : Array.Empty<IWritePath>().Append(p));
        }

        public string FullName()
            => IPath.BaseFullName(this);

        public override string ToString()
            => FullName();


        // Creates the specific root element.
        // The name is set to empty due to it being fixed in the constructor.
        internal static Folder CreateRoot()
            => new(null!, "_", 0)
            {
                Name  = string.Empty,
                Depth = RootDepth,
            };
    }
}
