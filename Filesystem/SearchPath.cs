using System.Collections.Generic;

namespace OtterGui.Filesystem;

public partial class FileSystem<T>
{
    public interface IPath
    {
        public Folder Parent { get; }
        public string Name   { get; }
    }


    // A search path just to be used for comparison between siblings.
    internal struct SearchPath : IPath
    {
        public Folder Parent
            => null!;

        public string Name { get; private init; }

        public static implicit operator SearchPath(string path)
            => new() { Name = path };
    }
}
