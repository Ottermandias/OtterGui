using System;
using System.IO;
using System.Text;
using Dalamud.Game;
using OtterGui.Log;

namespace OtterGui.Classes;

/// <summary>
/// Any file type that we want to save via SaveService.
/// </summary>
public interface ISavable<in T>
{
    /// <summary> The full file name of a given object. </summary>
    public string ToFilename(T fileNames);

    /// <summary> Write the objects data to the given stream writer. </summary>
    public void Save(StreamWriter writer);

    /// <summary> An arbitrary message printed to Debug before saving. </summary>
    public string LogName(string fileName)
        => fileName;

    public string TypeName
        => GetType().Name;
}

public class SaveServiceBase<T>
{
#if DEBUG
    private static readonly TimeSpan StandardDelay = TimeSpan.FromSeconds(2);
#else
    private static readonly TimeSpan StandardDelay = TimeSpan.FromSeconds(10);
#endif

    protected readonly Logger           Log;
    protected readonly FrameworkManager Framework;

    public readonly T FileNames;

    public Framework DalamudFramework
        => Framework.Framework;

    protected SaveServiceBase(Logger log, FrameworkManager framework, T fileNames)
    {
        Log       = log;
        Framework = framework;
        FileNames = fileNames;
    }

    /// <summary> Queue a save for the next framework tick. </summary>
    public void QueueSave(ISavable<T> value)
    {
        var file = value.ToFilename(FileNames);
        Framework.RegisterOnTick($"{value.GetType().Name} ## {file}", () => { ImmediateSave(value); });
    }

    /// <summary> Queue a delayed save with the standard delay for after the delay is over. </summary>
    public void DelaySave(ISavable<T> value)
        => DelaySave(value, StandardDelay);

    /// <summary> Queue a delayed save for after the delay is over. </summary>
    public void DelaySave(ISavable<T> value, TimeSpan delay)
    {
        var file = value.ToFilename(FileNames);
        Framework.RegisterDelayed($"{value.GetType().Name} ## {file}", () => { ImmediateSave(value); }, delay);
    }

    /// <summary> Immediately trigger a save. </summary>
    public void ImmediateSave(ISavable<T> value)
    {
        var name = value.ToFilename(FileNames);
        try
        {
            if (name.Length == 0)
                throw new Exception("Invalid object returned empty filename.");

            var secureWrite = File.Exists(name);
            var firstName   = secureWrite ? name + ".tmp" : name;
            Log.Debug($"Saving {value.TypeName} {value.LogName(name)}...");
            var file = new FileInfo(firstName);
            file.Directory?.Create();
            using (var s = file.Exists ? file.Open(FileMode.Truncate) : file.Open(FileMode.CreateNew))
            {
                using var w = new StreamWriter(s, Encoding.UTF8);
                value.Save(w);
            }

            if (secureWrite)
                File.Move(file.FullName, name, true);
        }
        catch (Exception ex)
        {
            Log.Error($"Could not save {value.GetType().Name} {value.LogName(name)}:\n{ex}");
        }
    }

    public void ImmediateDelete(ISavable<T> value)
    {
        var name = value.ToFilename(FileNames);
        try
        {
            if (name.Length == 0)
                throw new Exception("Invalid object returned empty filename.");

            if (!File.Exists(name))
                return;

            Log.Information($"Deleting {value.GetType().Name} {value.LogName(name)}...");
            File.Delete(name);
        }
        catch (Exception ex)
        {
            Log.Error($"Could not delete {value.GetType().Name} {value.LogName(name)}:\n{ex}");
        }
    }
}
