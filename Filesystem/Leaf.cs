namespace OtterGui.Filesystem;

public partial class FileSystem<T>
{
    public class Leaf : IPath
    {
        public Folder Parent { get; internal set; }
        public string Name   { get; internal set; }
        public T      Value  { get; }

        internal Leaf(Folder parent, string name, T value)
        {
            Parent = parent;
            Name   = name.FixName();
            Value  = value;
        }

        public override string ToString()
            => this.FullName();
    }
}
