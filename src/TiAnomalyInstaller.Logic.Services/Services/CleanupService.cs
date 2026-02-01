// ⠀
// CleanupService.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 01.02.2026.
// ⠀

using TiAnomalyInstaller.AppConstants;

namespace TiAnomalyInstaller.Logic.Services.Services;

public interface ICleanupService
{
    void RemoveGame();
    void RemoveArchives();
}

public class CleanupService(IStorageService storage): ICleanupService
{
    public void RemoveGame()
    {
        if (Directory.Exists(Constants.VanillaFolderName))
            Directory.Delete(Constants.VanillaFolderName, true);
        if (Directory.Exists(Constants.OrganizerFolderName))
            Directory.Delete(Constants.OrganizerFolderName, true);
        
        storage.Remove(StorageServiceKey.Version);
    }
    
    public void RemoveArchives()
    {
        if (Directory.Exists(Constants.StorageDownloadFolder))
            Directory.Delete(Constants.StorageDownloadFolder, true);
    }
}