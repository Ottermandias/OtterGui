using System;
using System.Collections.Generic;
using Dalamud.Interface.Internal;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using ImGuiScene;
using Lumina.Data.Files;

namespace OtterGui.Classes;

public class IconStorage
{
    private readonly ITextureProvider _textureProvider;
    private readonly IDataManager     _dataManager;

    public IconStorage(ITextureProvider textureProvider, IDataManager dataManager)
    {
        _textureProvider = textureProvider;
        _dataManager     = dataManager;
    }

    public bool IconExists(uint id)
        => _dataManager.FileExists(_textureProvider.GetIconPath(id) ?? string.Empty)
         || _dataManager.FileExists(_textureProvider.GetIconPath(id, ITextureProvider.IconFlags.None) ?? string.Empty);

    public IDalamudTextureWrap? this[int id]
        => _textureProvider.GetIcon((uint)id);

    public IDalamudTextureWrap? this[uint id]
        => _textureProvider.GetIcon(id);

    public IDalamudTextureWrap? LoadIcon(int id)
        => _textureProvider.GetIcon((uint)id);

    public IDalamudTextureWrap? LoadIcon(uint id)
        => _textureProvider.GetIcon(id);
}
