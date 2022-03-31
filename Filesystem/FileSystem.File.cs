using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OtterGui.Filesystem;

public partial class FileSystem<T>
{
    public void SaveToFile(FileInfo file, Func<T, string, (string, bool)> conversion, bool addEmptyFolders)
    {
        using var stream = File.Exists(file.FullName)
            ? File.Open(file.FullName, FileMode.Truncate)
            : File.Open(file.FullName, FileMode.CreateNew);
        using var w = new StreamWriter(stream);
        using var j = new JsonTextWriter(w);
        j.Formatting = Formatting.Indented;

        var emptyFolders = new List<string>();
        j.WriteStartObject();
        j.WritePropertyName("Data");
        j.WriteStartObject();
        if (Root._children.Count > 0)
            foreach (var path in Root.GetAllChildren(SortMode.Lexicographical))
            {
                switch (path)
                {
                    case Folder f:
                        if (addEmptyFolders && f._children.Count == 0)
                            emptyFolders.Add(f.FullName());
                        break;
                    case Leaf l:
                        var fullPath = l.FullName();
                        var (name, write) = conversion(l.Value, fullPath);
                        if (write)
                        {
                            j.WritePropertyName(name);
                            j.WriteValue(fullPath);
                        }

                        break;
                }
            }

        j.WriteEndObject();
        if (addEmptyFolders)
        {
            j.WritePropertyName("EmptyFolders");
            j.WriteStartArray();
            foreach (var emptyFolder in emptyFolders)
                j.WriteValue(emptyFolder);
            j.WriteEndArray();
        }

        j.WriteEndObject();
    }

    public static bool Load(FileInfo file, IEnumerable<T> objects, Func<T, string> func, out FileSystem<T> ret,
        IComparer<string>? comparer = null)
    {
        ret = new FileSystem<T>(comparer);
        if (!File.Exists(file.FullName))
            return true;

        var jObject      = JObject.Parse(File.ReadAllText(file.FullName));
        var data         = jObject["Data"]?.Value<Dictionary<string, string>>() ?? new Dictionary<string, string>();
        var emptyFolders = jObject["EmptyFolders"]?.Value<string[]>() ?? Array.Empty<string>();
        var changes      = false;

        foreach (var value in objects)
        {
            var name = func(value);
            if (data.TryGetValue(name, out var path))
            {
                var split = path.Split();
                var (result, folder) = ret.CreateAllFolders(split[..^1]);
                if (result is not Result.Success or Result.SuccessNothingDone)
                {
                    changes = true;
                    continue;
                }

                var leaf = new Leaf(folder, split[^1], value);
                while (ret.SetChild(folder, leaf, out var idx) == Result.ItemExists)
                {
                    leaf.Name += '_';
                    changes   =  true;
                }

                data.Remove(name);
            }
            else
            {
                var leaf = new Leaf(ret.Root, name, value);
                while (ret.SetChild(ret.Root, leaf, out var idx) == Result.ItemExists)
                {
                    leaf.Name += '_';
                    changes   =  true;
                }
            }
        }

        changes |= data.Count > 0;

        foreach (var split in emptyFolders.Concat(data.Values).Select(folder => folder.Split()))
        {
            var (result, _) = ret.CreateAllFolders(split);
            if (result is not Result.Success or Result.SuccessNothingDone)
                changes = true;
        }

        return changes;
    }
}
