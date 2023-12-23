using Dalamud.Interface.Internal;
using Dalamud.Plugin.Services;

namespace OtterGui.Classes;

public class IconStorage(ITextureProvider textureProvider, IDataManager dataManager)
{
    public bool IconExists(uint id)
        => dataManager.FileExists(textureProvider.GetIconPath(id) ?? string.Empty)
         || dataManager.FileExists(textureProvider.GetIconPath(id, ITextureProvider.IconFlags.None) ?? string.Empty);

    public IDalamudTextureWrap? this[int id]
        => textureProvider.GetIcon((uint)id);

    public IDalamudTextureWrap? this[uint id]
        => textureProvider.GetIcon(id);

    public IDalamudTextureWrap? LoadIcon(int id)
        => textureProvider.GetIcon((uint)id);

    public IDalamudTextureWrap? LoadIcon(uint id)
        => textureProvider.GetIcon(id);
}
