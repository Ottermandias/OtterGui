namespace OtterGui.Filesystem;

public partial class FileSystem<T>
{
    // Compare paths only by their name, using the submitted string comparer.
    private readonly struct NameComparer(IComparer<string> baseComparer) : IComparer<IPath>
    {
        public int Compare(IPath? x, IPath? y)
        {
            if (ReferenceEquals(x, y))
                return 0;
            if (y is null)
                return 1;
            if (x is null)
                return -1;

            return baseComparer.Compare(x.Name, y.Name);
        }
    }
}
