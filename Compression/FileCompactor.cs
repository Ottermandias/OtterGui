using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using OtterGui.Log;

namespace OtterGui.Compression;

public class FileCompactor : IDisposable
{
    public readonly bool CanCompact;

    /// <summary> Whether to use file system compression at all. </summary>
    public bool Enabled
    {
        get => _enabled;
        set
        {
            _logger.Information(
                $"File System Compression was {(value ? "enabled" : "disabled")}{(CanCompact ? string.Empty : " but was not available")}.");
            _enabled = CanCompact && value;
        }
    }

    private          bool                    _enabled;
    private readonly Dictionary<string, int> _clusterSizes = new(8, StringComparer.Ordinal);
    private readonly Logger                  _logger;

    private Task?                    _massCompact;
    private CancellationTokenSource? _cancellation;

    /// <summary> Check if a mass compact operation is currently running. </summary>
    public bool MassCompactRunning
        => _massCompact is { IsCompleted: false };

    /// <summary> The file currently being compacted, if any. </summary>
    public FileInfo? CurrentFile { get; private set; }

    /// <summary> The index of the file currently being compacted for Progress. </summary>
    public int CurrentIndex { get; private set; }

    /// <summary> The total number of files in the current mass compact operation. </summary>
    public int TotalFiles { get; private set; }

    public FileCompactor(Logger logger)
    {
        _logger    = logger;
        CanCompact = !Dalamud.Utility.Util.IsLinux();
    }

    public void Dispose()
        => CancelMassCompact();

    /// <summary> Cancel the current mass compact operation if one is running. </summary>
    public void CancelMassCompact()
        => _cancellation?.Cancel();

    /// <summary> Start a new mass compact operation on a set of files. </summary>
    /// <param name="files"> The set of files we want to compact. </param>
    /// <param name="algorithm"> The compression algorithm to use. Use None to decompress files instead. </param>
    /// <returns> If the task could successfully be started. </returns>
    public bool StartMassCompact(IEnumerable<FileInfo> files, CompressionAlgorithm algorithm)
    {
        if (MassCompactRunning)
        {
            _logger.Error("Triggered Mass Compact of files while it was already running.");
            return false;
        }

        _cancellation = new CancellationTokenSource();
        var token = _cancellation.Token;
        _massCompact = Task.Run(() =>
        {
            TotalFiles   = 1;
            CurrentIndex = 0;
            CurrentFile  = null;
            var list = files.ToList();
            TotalFiles = list.Count;
            _logger.Information(
                $"Starting Mass {(algorithm is CompressionAlgorithm.None ? "Decompact" : $"Compact with {algorithm}")} for {TotalFiles} files.");
            for (; CurrentIndex < list.Count; ++CurrentIndex)
            {
                if (token.IsCancellationRequested)
                    return;

                CurrentFile = list[CurrentIndex];
                if (algorithm is CompressionAlgorithm.None)
                    Interop.DecompactFile(CurrentFile.FullName);
                else
                    CompactFile(CurrentFile.FullName, algorithm);
            }
        }, token);
        return true;
    }

    /// <summary> Get the actual file size of a file on disk. </summary>
    /// <param name="filePath"> The path to the file. </param>
    /// <returns> The actual file size considering compression and clustering. </returns>
    public long GetFileSizeOnDisk(string filePath)
    {
        if (!CanCompact)
            return new FileInfo(filePath).Length;

        var clusterSize = GetClusterSize(filePath);
        if (clusterSize == -1)
            return new FileInfo(filePath).Length;

        var size = Interop.GetCompressedFileSize(filePath);
        return Interop.RoundToCluster(size, clusterSize);
    }

    /// <summary> Write bytes and conditionally compact the file afterwards. </summary>
    /// <param name="filePath"> The file to write to. </param>
    /// <param name="decompressedFile"> The data to write. </param>
    public void WriteAllBytes(string filePath, byte[] decompressedFile)
    {
        File.WriteAllBytes(filePath, decompressedFile);

        if (Enabled)
            CompactFile(filePath);
    }

    /// <summary> Asynchronously write bytes and conditionally compact the file afterwards. </summary>
    /// <param name="filePath"> The file to write to. </param>
    /// <param name="decompressedFile"> The data to write. </param>
    /// <param name="token"> A cancellation token. </param>
    public async Task WriteAllBytesAsync(string filePath, byte[] decompressedFile, CancellationToken token = default)
    {
        await File.WriteAllBytesAsync(filePath, decompressedFile, token);

        if (Enabled && !token.IsCancellationRequested)
            CompactFile(filePath);
    }

    /// <summary> Get the cluster size for a drive root. </summary>
    private int GetClusterSize(string filePath)
    {
        if (!File.Exists(filePath))
            return -1;

        var root = Path.GetPathRoot(filePath)?.ToLowerInvariant() ?? string.Empty;
        if (root.Length == 0)
            return -1;

        if (!_clusterSizes.TryGetValue(root, out var size))
        {
            var result = Interop.GetDiskFreeSpaceW(root, out var sectorsPerCluster, out var bytesPerSector, out _, out _);
            if (result == 0)
                return -1;

            size = (int)(sectorsPerCluster * bytesPerSector);
            _logger.Verbose($"Cluster size for root {root} is {size}.");
            _clusterSizes.Add(root, size);
        }

        return size;
    }

    /// <summary> Try to compact a file with a given algorithm. </summary>
    private bool CompactFile(string filePath, CompressionAlgorithm algorithm = CompressionAlgorithm.Xpress8K)
    {
        try
        {
            var oldSize     = new FileInfo(filePath).Length;
            var clusterSize = GetClusterSize(filePath);

            if (oldSize < Math.Max(clusterSize, 8 * 1024))
            {
                _logger.Debug($"File {filePath} is smaller than cluster size ({clusterSize}), it will not be compacted.");
                return false;
            }


            if (Interop.IsCompactedFile(filePath, algorithm))
            {
                _logger.Debug($"File {filePath} is already compacted with {algorithm}.");
                return true;
            }


            Interop.CompactFile(filePath, algorithm);
            _logger.Debug($"Compacted {filePath} from {oldSize} bytes to {new Lazy<long>(() => GetFileSizeOnDisk(filePath))} bytes.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Unexpected problem when compacting file {filePath}:\n{ex}");
            return false;
        }
    }
}
