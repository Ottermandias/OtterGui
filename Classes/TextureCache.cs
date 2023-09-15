using Dalamud.Interface.Internal;
using Dalamud.Plugin.Services;

namespace OtterGui.Classes;

public class TextureCache
{
    public readonly IDataManager     DataManager;
    public readonly ITextureProvider TextureProvider;

    public IDalamudTextureWrap? LoadIcon(uint iconId, bool keepAlive = false)
        => TextureProvider.GetIcon(iconId, ITextureProvider.IconFlags.HiRes, null, keepAlive);

    public IDalamudTextureWrap? LoadFile(string file, bool keepAlive = false)
        => Path.IsPathRooted(file)
            ? TextureProvider.GetTextureFromFile(new FileInfo(file), keepAlive)
            : TextureProvider.GetTextureFromGame(file, keepAlive);

    public bool TryLoadIcon(uint iconId, [NotNullWhen(true)] out IDalamudTextureWrap? wrap, bool keepAlive = false)
    {
        wrap = LoadIcon(iconId, keepAlive);
        return wrap != null;
    }

    public bool TryLoadFile(string file, [NotNullWhen(true)] out IDalamudTextureWrap? wrap, bool keepAlive = false)
    {
        wrap = LoadFile(file, keepAlive);
        return wrap != null;
    }

    public bool FileExists(string file)
        => Path.IsPathRooted(file) ? File.Exists(file) : DataManager.FileExists(file);

    public bool IconExists(uint iconId)
        => DataManager.FileExists(TextureProvider.GetIconPath(iconId) ?? string.Empty)
         || DataManager.FileExists(TextureProvider.GetIconPath(iconId, ITextureProvider.IconFlags.None) ?? string.Empty);

    public TextureCache(IDataManager dataManager, ITextureProvider textureProvider)
    {
        DataManager     = dataManager;
        TextureProvider = textureProvider;
    }
}
