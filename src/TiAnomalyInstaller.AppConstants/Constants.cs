// ⠀
// Constants.cs
// TiAnomalyInstaller.AppConstants
// 
// Created by the_timick on 06.01.2026.
// ⠀

namespace TiAnomalyInstaller.AppConstants;

// ReSharper disable InconsistentNaming

public static class Constants
{
    public static class Files
    {
        public static class Hash
        {
            public enum ChecksumsType
            {
                Archives,
                Game
            }
            
            public enum FileType 
            {
                Archive,
                Text
            }
            
            public const string ArchiveChecksums7zFileName = "ArchiveChecksums.7z";
            public const string ArchiveChecksumsFileName = "ArchiveChecksums.txt";
            public const string GameChecksums7zFileName = "GameChecksums.7z";
            public const string GameChecksumsFileName = "GameChecksums.txt";

            public static string GetPath(ChecksumsType checksums, FileType file)
            {
                return checksums switch {
                    ChecksumsType.Archives => file switch {
                        FileType.Archive => Path.Combine(StorageDownloadFolder, ArchiveChecksums7zFileName),
                        FileType.Text => Path.Combine(StorageDownloadFolder, ArchiveChecksumsFileName),
                        _ => throw new ArgumentOutOfRangeException(nameof(file), file, null)
                    },
                    ChecksumsType.Game => file switch {
                        FileType.Archive => Path.Combine(StorageDownloadFolder, GameChecksums7zFileName),
                        FileType.Text => Path.Combine(StorageDownloadFolder, GameChecksumsFileName),
                        _ => throw new ArgumentOutOfRangeException(nameof(file), file, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(checksums), checksums, null)
                };
            }
        }
        
        public static readonly string LocalConfigFileName = Path.Combine(CurrentDirectory, Storage, "config.toml");
        public static readonly string LogFileName = Path.Combine(CurrentDirectory, Storage, "log.txt");
    }
    
    public static class MO2
    {
        public static readonly string PlayingFileName = Path.Combine(OrganizerFolderName, "ModOrganizer.exe");
        public static readonly string ConfigFileName = Path.Combine(OrganizerFolderName, "ModOrganizer.ini");
    }

    public static class Utils
    {
        public const string YandexDiskDomain = "disk.yandex.ru";
        public const string YandexDiskResourcesApi = "https://cloud-api.yandex.net/v1/disk/public/resources/download?public_key=<key>";
    }
    
    public const string Vanilla = "Vanilla";
    public const string Organizer = "Organizer";
    public const string Storage = "Storage";
    
    public static readonly string CurrentDirectory = Environment.CurrentDirectory;
    public static readonly string VanillaFolderName = Path.Combine(CurrentDirectory, Vanilla);
    public static readonly string OrganizerFolderName = Path.Combine(CurrentDirectory, Organizer);
    public static readonly string StorageFolder = Path.Combine(CurrentDirectory, Storage);
    public static readonly string StorageDownloadFolder = Path.Combine(StorageFolder, "download");
} 