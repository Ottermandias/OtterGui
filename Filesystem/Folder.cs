using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace OtterGui.Filesystem;

public partial class FileSystem<T>
{
    public class Folder : IPath
    {
        public Folder Parent        { get; internal set; }
        public string Name          { get; internal set; }
        public int    TotalChildren { get; internal set; } = 0;
        public int    TotalLeaves   { get; internal set; } = 0;

        internal List<IPath> _children = new();

        public Folder(Folder parent, string name)
        {
            Parent = parent;
            Name   = name.FixName();
        }

        public IEnumerable<IPath> GetSortedEnumerator(SortMode mode)
        {
            switch (mode)
            {
                case SortMode.FoldersFirst:
                    yield return this;

                    foreach (var child in _children.OfType<Folder>())
                        yield return child;
                    foreach (var child in _children.OfType<Leaf>())
                        yield return child;

                    break;
                case SortMode.Lexicographical:
                    yield return this;

                    foreach (var child in _children)
                        yield return child;

                    break;
                default: throw new InvalidEnumArgumentException();
            }
        }

        public IEnumerable<IPath> GetAllChildren(SortMode mode)
        {
            return GetSortedEnumerator(mode).SelectMany(p => p is Folder f
                ? f.GetAllChildren(mode)
                : new[]
                {
                    p,
                });
        }

        internal static Folder CreateRoot()
            => new(null!, string.Empty);

        public override string ToString()
            => this.FullName();
    }
}
