using System;

namespace OtterGui.Filesystem;

public enum SortMode : byte
{
    FoldersFirst           = 0x00,
    Lexicographical        = 0x01,
    InverseFoldersFirst    = 0x02,
    InverseLexicographical = 0x03,
    FoldersLast            = 0x04,
    InverseFoldersLast     = 0x05,
    InternalOrder          = 0x06,
    InternalOrderInverse   = 0x07,
}

public static class SortModeExtensions
{
    public static (string Name, string Description) Data(this SortMode value)
        => value switch
        {
            // @formatter:off
            SortMode.FoldersFirst           => ("Folders First",              "In each folder, sort all subfolders lexicographically, then sort all leaves lexicographically."),
            SortMode.Lexicographical        => ("Lexicographical",            "In each folder, sort all children lexicographically."),
            SortMode.InverseFoldersFirst    => ("Folders First (Inverted)",   "In each folder, sort all subfolders in inverse lexicographical order, then sort all leaves in inverse lexicographical order."),
            SortMode.InverseLexicographical => ("Lexicographical (Inverted)", "In each folder, sort all children in inverse lexicographical order."),
            SortMode.FoldersLast            => ("Folders Last",               "In each folder, sort all leaves lexicographically, then sort all subfolders lexicographically."),
            SortMode.InverseFoldersLast     => ("Folders Last (Inverted)",    "In each folder, sort all leaves in inverse lexicographical order, then sort all subfolders in inverse lexicographical order."),
            SortMode.InternalOrder          => ("Internal Order",             "In each folder, sort all children in order of their identifiers (i.e. in order of their creation in the filesystem)."),
            SortMode.InternalOrderInverse   => ("Internal Order (Inverted)",  "In each folder, sort all children in inverse order of their identifiers (i.e. in inverse order of their creation in the filesystem)."),
            _                               => ("Invalid",                    "Not a valid sort mode."),
            // @formatter:on
        };
}
