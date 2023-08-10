using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using Dalamud.Game;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using ImGuiScene;
using Lumina.Data.Files;
using OtterGui.Log;

namespace OtterGui.Classes;

public readonly struct TextureHandle
{
    private readonly TextureCache _cache;
    public readonly  string       Identifier;

    internal TextureHandle(TextureCache cache, string identifier)
    {
        _cache     = cache;
        Identifier = identifier;
    }

    public (nint Texture, Vector2 Dimensions) Value
        => _cache.GetTexture(Identifier, out var wrap) ? (wrap.ImGuiHandle, new Vector2(wrap.Width, wrap.Height)) : (nint.Zero, Vector2.Zero);

    /// <remarks>Prefer to use <see cref="Value"/> to have fewer lookups and locks.</remarks>
    public int Width
        => _cache.GetTexture(Identifier, out var wrap) ? wrap.Width : 0;

    /// <remarks>Prefer to use <see cref="Value"/> to have fewer lookups and locks.</remarks>
    public int Height
        => _cache.GetTexture(Identifier, out var wrap) ? wrap.Height : 0;

    /// <remarks>Prefer to use <see cref="Value"/> to have fewer lookups and locks.</remarks>
    public nint Texture
        => _cache.GetTexture(Identifier, out var wrap) ? wrap.ImGuiHandle : nint.Zero;
}

public class TextureCache : IDisposable
{
    private class Data
    {
        public TextureWrap Wrap = null!;
        public DateTime    LastAccess;
        public string      Identifier = null!;
    }

    public TimeSpan KeepAliveTime   { get; set; } = TimeSpan.FromSeconds(2);
    public int      CleanupPerFrame { get; set; } = 10;
    public Logger?  Logger          { get; set; } = null;

    private readonly Framework        _framework;
    private readonly UiBuilder        _uiBuilder;
    private readonly IDataManager     _dataManager;
    private readonly ITextureProvider _textureProvider;

    public TextureHandle LoadFile(string filename, bool keepAlive = false)
    {
        var ret = new TextureHandle(this, filename);
        if (keepAlive)
            GetTexture(ret.Identifier, out _, true);
        return ret;
    }

    public TextureHandle? LoadIcon(uint iconId, bool keepAlive = false)
        => TryLoadIcon(iconId, out var ret, keepAlive) ? ret : null;

    public bool TryLoadIcon(uint iconId, out TextureHandle ret, bool keepAlive = false)
    {
        var path = HqPath(iconId);
        if (_dataManager.FileExists(path))
        {
            ret = LoadFile(path, keepAlive);
            return true;
        }

        path = NormalPath(iconId);
        if (_dataManager.FileExists(path))
        {
            ret = LoadFile(path, keepAlive);
            return true;
        }

        ret = new TextureHandle(this, string.Empty);
        return false;
    }

    private static string HqPath(uint id)
        => $"ui/icon/{id / 1000 * 1000:000000}/{id:000000}_hr1.tex";

    private static string NormalPath(uint id)
        => $"ui/icon/{id / 1000 * 1000:000000}/{id:000000}.tex";

    public TextureCache(Framework framework, UiBuilder uiBuilder, IDataManager dataManager, ITextureProvider textureProvider)
    {
        _framework       = framework;
        _uiBuilder       = uiBuilder;
        _dataManager     = dataManager;
        _textureProvider = textureProvider;

        _framework.Update += OnUpdate;
    }

    private readonly List<Data>               _data     = new();
    private readonly Dictionary<string, Data> _textures = new();

    private int _currentCleanupIndex;

    internal bool GetTexture(string identifier, [NotNullWhen(true)] out TextureWrap? wrap)
        => GetTexture(identifier, out wrap, false);

    private bool GetTexture(string identifier, [NotNullWhen(true)] out TextureWrap? wrap, bool keepAlive)
    {
        Data? data;
        bool  success;
        lock (_textures)
        {
            success = _textures.TryGetValue(identifier, out data);
        }

        if (success)
        {
            data!.LastAccess = keepAlive ? DateTime.MaxValue : DateTime.UtcNow;
            wrap             = data.Wrap;
            Logger?.Excessive($"[TextureCache] Obtained loaded file {identifier}.");
        }
        else
        {
            if (Path.IsPathRooted(identifier))
            {
                if (!File.Exists(identifier))
                {
                    Logger?.Warning($"[TextureCache] Requested file {identifier} does not exist on drive.");
                    wrap = null;
                    return false;
                }

                try
                {
                    wrap = _uiBuilder.LoadImage(identifier);
                    Logger?.Verbose($"[TextureCache] Loaded new image {identifier} from drive.");
                }
                catch
                {
                    try
                    {
                        var tex = _dataManager.GameData.GetFileFromDisk<TexFile>(identifier);
                        wrap = _textureProvider.GetTexture(tex);
                        Logger?.Verbose($"[TextureCache] Loaded new TexFile {identifier} from drive.");
                    }
                    catch
                    {
                        Logger?.Warning($"[TextureCache] Requested file {identifier} does not have a loadable type.");
                        wrap = null;
                        return false;
                    }
                }
            }
            else
            {
                try
                {
                    var tex = _dataManager.GameData.GetFile<TexFile>(identifier);
                    wrap = tex == null ? null : _textureProvider.GetTexture(tex);
                    Logger?.Verbose($"[TextureCache] Loaded new TexFile {identifier} from game files.");
                }
                catch
                {
                    Logger?.Warning($"[TextureCache] Requested file {identifier} does not exist in game files.");
                    wrap = null;
                    return false;
                }
            }

            if (wrap == null)
                return false;

            data = new Data()
            {
                Identifier = identifier,
                LastAccess = keepAlive ? DateTime.MaxValue : DateTime.UtcNow,
                Wrap       = wrap!,
            };
            lock (_data)
            lock (_textures)
            {
                _textures.TryAdd(data.Identifier, data);
                if (!keepAlive)
                    _data.Add(data);
            }
        }

        return true;
    }

    public void Dispose()
    {
        _framework.Update -= OnUpdate;
        lock (_data)
        lock (_textures)
        {
            foreach (var (_, data) in _textures)
                data.Wrap.Dispose();
            _data.Clear();
            _textures.Clear();
        }
    }

    private void OnUpdate(Framework _)
        => Cleanup();

    private void Cleanup()
    {
        var timing = DateTime.UtcNow - KeepAliveTime;
        lock (_data)
        {
            var max = Math.Min(_data.Count, CleanupPerFrame);
            for (var i = 0; i < max; ++i)
            {
                var data = _data[_currentCleanupIndex];
                if (data.LastAccess >= timing)
                {
                    _currentCleanupIndex = _currentCleanupIndex == _data.Count - 1 ? 0 : _currentCleanupIndex + 1;
                    continue;
                }

                lock (_textures)
                {
                    _textures.Remove(data.Identifier);
                }

                _data.RemoveAt(_currentCleanupIndex);
                data.Wrap.Dispose();
                if (_currentCleanupIndex == _data.Count)
                    _currentCleanupIndex = 0;
                Logger?.Verbose($"[TextureCache] Destroyed loaded texture {data.Identifier} last accessed at {data.LastAccess:T}.");
            }
        }
    }
}
