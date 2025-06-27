namespace OtterGui.Filesystem;

public enum SortMode
{
    FoldersFirst,
    Lexicographical,
    InverseFoldersFirst,
    InverseLexicographical,
    FoldersLast,
    InverseFoldersLast,
    InternalOrder,
    InverseInternalOrder,
}

public interface ISortMode<T> where T : class
{
    ReadOnlySpan<byte> Name        { get; }
    ReadOnlySpan<byte> Description { get; }

    IEnumerable<FileSystem<T>.IPath> GetChildren(FileSystem<T>.Folder folder);

    public static readonly ISortMode<T> FoldersFirst           = new FoldersFirstT();
    public static readonly ISortMode<T> Lexicographical        = new LexicographicalT();
    public static readonly ISortMode<T> InverseFoldersFirst    = new InverseFoldersFirstT();
    public static readonly ISortMode<T> InverseLexicographical = new InverseLexicographicalT();
    public static readonly ISortMode<T> FoldersLast            = new FoldersLastT();
    public static readonly ISortMode<T> InverseFoldersLast     = new InverseFoldersLastT();
    public static readonly ISortMode<T> InternalOrder          = new InternalOrderT();
    public static readonly ISortMode<T> InverseInternalOrder   = new InverseInternalOrderT();

    private struct FoldersFirstT : ISortMode<T>
    {
        public ReadOnlySpan<byte> Name
            => "Folders First"u8;

        public ReadOnlySpan<byte> Description
            => "In each folder, sort all subfolders lexicographically, then sort all leaves lexicographically."u8;

        public IEnumerable<FileSystem<T>.IPath> GetChildren(FileSystem<T>.Folder folder)
            => folder.GetSubFolders().Cast<FileSystem<T>.IPath>().Concat(folder.GetLeaves());
    }

    private struct LexicographicalT : ISortMode<T>
    {
        public ReadOnlySpan<byte> Name
            => "Lexicographical"u8;

        public ReadOnlySpan<byte> Description
            => "In each folder, sort all children lexicographically."u8;

        public IEnumerable<FileSystem<T>.IPath> GetChildren(FileSystem<T>.Folder folder)
            => folder.Children;
    }

    private struct InverseFoldersFirstT : ISortMode<T>
    {
        public ReadOnlySpan<byte> Name
            => "Folders First (Inverted)"u8;

        public ReadOnlySpan<byte> Description
            => "In each folder, sort all subfolders in inverse lexicographical order, then sort all leaves in inverse lexicographical order."u8;

        public IEnumerable<FileSystem<T>.IPath> GetChildren(FileSystem<T>.Folder folder)
            => folder.GetSubFolders().Cast<FileSystem<T>.IPath>().Reverse().Concat(folder.GetLeaves().Reverse());
    }

    public struct InverseLexicographicalT : ISortMode<T>
    {
        public ReadOnlySpan<byte> Name
            => "Lexicographical (Inverted)"u8;

        public ReadOnlySpan<byte> Description
            => "In each folder, sort all children in inverse lexicographical order."u8;

        public IEnumerable<FileSystem<T>.IPath> GetChildren(FileSystem<T>.Folder folder)
            => folder.Children.Cast<FileSystem<T>.IPath>().Reverse();
    }

    public struct FoldersLastT : ISortMode<T>
    {
        public ReadOnlySpan<byte> Name
            => "Folders Last"u8;

        public ReadOnlySpan<byte> Description
            => "In each folder, sort all leaves lexicographically, then sort all subfolders lexicographically."u8;

        public IEnumerable<FileSystem<T>.IPath> GetChildren(FileSystem<T>.Folder folder)
            => folder.GetLeaves().Cast<FileSystem<T>.IPath>().Concat(folder.GetSubFolders());
    }

    public struct InverseFoldersLastT : ISortMode<T>
    {
        public ReadOnlySpan<byte> Name
            => "Folders Last (Inverted)"u8;

        public ReadOnlySpan<byte> Description
            => "In each folder, sort all leaves in inverse lexicographical order, then sort all subfolders in inverse lexicographical order."u8;

        public IEnumerable<FileSystem<T>.IPath> GetChildren(FileSystem<T>.Folder folder)
            => folder.GetLeaves().Cast<FileSystem<T>.IPath>().Reverse().Concat(folder.GetSubFolders().Reverse());
    }

    public struct InternalOrderT : ISortMode<T>
    {
        public ReadOnlySpan<byte> Name
            => "Internal Order"u8;

        public ReadOnlySpan<byte> Description
            => "In each folder, sort all children in order of their identifiers (i.e. in order of their creation in the filesystem)."u8;

        public IEnumerable<FileSystem<T>.IPath> GetChildren(FileSystem<T>.Folder folder)
            => folder.Children.OrderBy(c => c.Identifier);
    }

    public struct InverseInternalOrderT : ISortMode<T>
    {
        public ReadOnlySpan<byte> Name
            => "Internal Order (Inverted)"u8;

        public ReadOnlySpan<byte> Description
            => "In each folder, sort all children in inverse order of their identifiers (i.e. in inverse order of their creation in the filesystem)."u8;

        public IEnumerable<FileSystem<T>.IPath> GetChildren(FileSystem<T>.Folder folder)
            => folder.Children.OrderByDescending(c => c.Identifier);
    }
}
