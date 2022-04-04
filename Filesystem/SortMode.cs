namespace OtterGui.Filesystem;

public enum SortMode : byte
{
    FoldersFirst           = 0x00,
    Lexicographical        = 0x01,
    InverseFoldersFirst    = 0x02,
    InverseLexicographical = 0x03,
    FoldersLast            = 0x04,
    InverseFoldersLast     = 0x05,
}
