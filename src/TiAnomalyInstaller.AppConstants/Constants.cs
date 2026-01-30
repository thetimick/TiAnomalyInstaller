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
        public static class MO2
        {
            public static readonly string PlayingFileName = Path.Combine(OrganizerFolderName, "ModOrganizer.exe");
            public static readonly string ConfigFileName = Path.Combine(OrganizerFolderName, "ModOrganizer.ini");
        }
        
        public static readonly string BackgroundFileName = Path.Combine(StorageFolder, "background.image");
        public static readonly string LocalConfigFileName = Path.Combine(CurrentDirectory, Storage, "config.toml");
        public static readonly string LogFileName = Path.Combine(CurrentDirectory, Storage, "log.txt");
    }

    public static class Utils
    {
        public const string YandexDiskDomain = "disk.yandex.ru";
        public const string YandexDiskResourcesApi = "https://cloud-api.yandex.net/v1/disk/public/resources/download?public_key=<key>";
        public const string GoogleDriveDomain = "drive.google.com";
        public const string GoogleDriveTemplateUrl = "https://drive.google.com/uc?export=download&id=<file_id>";
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