using OtterGui.Log;

namespace OtterGui.Classes;

public static partial class Backup
{
    public const int MaxNumBackups = 10;

    /// <summary>
    /// Create a backup named by ISO 8601 of the current time.
    /// </summary>
    public static void CreatePermanentBackup(Logger logger, DirectoryInfo dir, IReadOnlyCollection<FileInfo> files, string name)
        => CreateBackupInternal(logger, dir, files, name);

    /// <summary>
    /// Create a backup named by ISO 8601 of the current time.
    /// </summary>
    /// <remarks>
    /// If the newest previously existing backup equals the current state of files, do not create a new backup.
    /// If the maximum number of backups is exceeded afterwards, delete the oldest backup.
    /// </remarks>
    public static void CreateAutomaticBackup(Logger logger, DirectoryInfo dir, IReadOnlyCollection<FileInfo> files)
        => CreateBackupInternal(logger, dir, files, null);

    private static void CreateBackupInternal(Logger logger, DirectoryInfo dir, IReadOnlyCollection<FileInfo> files, string? name)
    {
        try
        {
            var configDirectory = dir.Parent!.FullName;
            var directory       = CreateBackupDirectory(dir);
            if (name == null)
            {
                var (newestFile, oldestFile, numFiles) = CheckExistingBackups(directory);
                var newBackupName = Path.Combine(directory.FullName, $"{DateTime.Now:yyyyMMddHHmmss}.zip");
                if (newestFile == null || CheckNewestBackup(logger, newestFile, configDirectory, files.Count))
                {
                    CreateBackupFile(files, newBackupName, configDirectory);
                    if (numFiles > MaxNumBackups)
                        oldestFile!.Delete();
                }
            }
            else
            {
                var fileName = $"{name}.zip";
                if (FormatRegex().IsMatch(fileName))
                    fileName = $"{name}-.zip";
                var newBackupName = Path.Combine(directory.FullName, fileName);
                CreateBackupFile(files, newBackupName, configDirectory);
            }
        }
        catch (Exception e)
        {
            logger.Error($"Could not create backups:\n{e}");
        }
    }

    // Obtain the backup directory. Create it if it does not exist.
    private static DirectoryInfo CreateBackupDirectory(DirectoryInfo dir)
    {
        var path   = Path.Combine(dir.Parent!.Parent!.FullName, "backups", dir.Name);
        var newDir = new DirectoryInfo(path);
        if (!newDir.Exists)
            newDir = Directory.CreateDirectory(newDir.FullName);

        return newDir;
    }

    // Check the already existing backups.
    // Only keep MaxNumBackups at once, and delete the oldest if the number would be exceeded.
    // Return the newest backup.
    private static (FileInfo? Newest, FileInfo? Oldest, int Count) CheckExistingBackups(DirectoryInfo backupDirectory)
    {
        var       count  = 0;
        FileInfo? newest = null;
        FileInfo? oldest = null;

        foreach (var file in backupDirectory.EnumerateFiles("*.zip").Where(f => FormatRegex().IsMatch(f.Name)))
        {
            ++count;
            var time = file.CreationTimeUtc;
            if ((oldest?.CreationTimeUtc ?? DateTime.MaxValue) > time)
                oldest = file;

            if ((newest?.CreationTimeUtc ?? DateTime.MinValue) < time)
                newest = file;
        }

        return (newest, oldest, count);
    }

    [GeneratedRegex(@"^\d{14}\.zip$", RegexOptions.ExplicitCapture | RegexOptions.NonBacktracking)]
    private static partial Regex FormatRegex();

    // Compare the newest backup against the currently existing files.
    // If there are any differences, return true, and if they are completely identical, return false.
    private static bool CheckNewestBackup(Logger logger, FileInfo newestFile, string configDirectory, int fileCount)
    {
        try
        {
            using var oldFileStream = File.Open(newestFile.FullName, FileMode.Open);
            using var oldZip        = new ZipArchive(oldFileStream, ZipArchiveMode.Read);
            // Number of stored files is different.
            if (fileCount != oldZip.Entries.Count)
                return true;

            // Since number of files is identical,
            // the backups are identical if every file in the old backup
            // still exists and is identical.
            foreach (var entry in oldZip.Entries)
            {
                var file = Path.Combine(configDirectory, entry.FullName);
                if (!File.Exists(file))
                    return true;

                using var currentData = File.OpenRead(file);
                using var oldData     = entry.Open();

                if (!Equals(currentData, oldData))
                    return true;
            }
        }
        catch (Exception e)
        {
            logger.Warning($"Could not read the newest backup file {newestFile.FullName}:\n{e}");
            return true;
        }

        return false;
    }

    // Create the actual backup, storing all the files relative to the given configDirectory in the zip.
    private static void CreateBackupFile(IEnumerable<FileInfo> files, string fileName, string configDirectory)
    {
        using var fileStream = File.Open(fileName, FileMode.Create);
        using var zip        = new ZipArchive(fileStream, ZipArchiveMode.Create);
        foreach (var file in files.Where(f => File.Exists(f.FullName)))
            zip.CreateEntryFromFile(file.FullName, Path.GetRelativePath(configDirectory, file.FullName), CompressionLevel.Optimal);
    }

    // Compare two streams per byte and return if they are equal.
    private static bool Equals(Stream lhs, Stream rhs)
    {
        while (true)
        {
            var current = lhs.ReadByte();
            var old     = rhs.ReadByte();
            if (current != old)
                return false;

            if (current == -1)
                return true;
        }
    }
}
