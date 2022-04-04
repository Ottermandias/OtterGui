using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace OtterGui.Filesystem;

public partial class FileSystem<T>
{
    public class Folder : IPath
    {
        public Folder Parent { get; internal set; }
        public string Name   { get; internal set; }

        // The folder internally keeps track of total descendants and total leaves.
        public int TotalDescendants { get; internal set; } = 0;
        public int TotalLeaves   { get; internal set; } = 0;

        internal List<IPath> Children = new();

        public Folder(Folder parent, string name)
        {
            Parent = parent;
            Name   = name.FixName();
        }

        // Iterate through all direct children in sort order.
        public IEnumerable<IPath> GetChildren(SortMode mode)
        {
            switch (mode)
            {
                case SortMode.FoldersFirst:
                    foreach (var child in Children.OfType<Folder>())
                        yield return child;
                    foreach (var child in Children.OfType<Leaf>())
                        yield return child;

                    break;
                case SortMode.FoldersLast:
                    foreach (var child in Children.OfType<Leaf>())
                        yield return child;
                    foreach (var child in Children.OfType<Folder>())
                        yield return child;

                    break;
                case SortMode.Lexicographical:
                    foreach (var child in Children)
                        yield return child;

                    break;
                case SortMode.InverseFoldersFirst:
                    foreach (var child in Children.OfType<Folder>().Reverse())
                        yield return child;
                    foreach (var child in Children.OfType<Leaf>().Reverse())
                        yield return child;

                    break;
                case SortMode.InverseLexicographical:
                    foreach (var child in ((IReadOnlyList<IPath>)Children).Reverse())
                        yield return child;

                    break;
                case SortMode.InverseFoldersLast:
                    foreach (var child in Children.OfType<Leaf>().Reverse())
                        yield return child;
                    foreach (var child in Children.OfType<Folder>().Reverse())
                        yield return child;


                    break;
                default: throw new InvalidEnumArgumentException();
            }
        }

        // Iterate through all Descendants in sort order, not including the folder itself.
        public IEnumerable<IPath> GetAllDescendants(SortMode mode)
        {
            return GetChildren(mode).SelectMany(p => p is Folder f
                ? f.GetAllDescendants(mode).Prepend(f)
                : Array.Empty<IPath>().Append(p));
        }

        public override string ToString()
            => this.FullName();


        // Creates the specific root element.
        // The name is set to empty due to it being fixed in the constructor.
        internal static Folder CreateRoot()
            => new(null!, "_")
            {
                Name = string.Empty,
            };
    }
}
