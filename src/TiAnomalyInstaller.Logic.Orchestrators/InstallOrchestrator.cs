// ⠀
// InstallOrchestrator.cs
// TiAnomalyInstaller.Logic.Orchestrators
// 
// Created by the_timick on 10.01.2026.
// ⠀

using System.Globalization;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TiAnomalyInstaller.AppConstants;
using TiAnomalyInstaller.AppConstants.Localization;
using TiAnomalyInstaller.Logic.Orchestrators.Components;
using TiAnomalyInstaller.Logic.Orchestrators.Entities;
using TiAnomalyInstaller.Logic.Services;
using TiAnomalyInstaller.Logic.Services.Entities;
using TiAnomalyInstaller.Logic.Services.Services;
using TiAnomalyInstaller.Logic.Services.Services.SevenZip;
using Version = SemanticVersioning.Version;

namespace TiAnomalyInstaller.Logic.Orchestrators;

public interface IInstallOrchestrator : IOrchestrator<InstallEventArgs>;

public partial class InstallOrchestrator(
    IStorageService storageService,
    IConfigService configService,
    IHashCheckerService hashCheckerService,
    ISevenZipService sevenZipService,
    IOrganizerService organizerService,
    ITransferService transferService,
    ILogger<InstallOrchestrator> logger,
    IServiceProvider provider
): IInstallOrchestrator
{
    public event EventHandler<InstallEventArgs>? Handler;
    
    public async Task StartAsync(CancellationToken token = default)
    {
        // Достаем параметры
        var url = storageService.GetString(StorageServiceKey.ProfileUrl) ?? "";
        var config = await configService.ObtainRemoteConfigAsync(url, false);

        // Первоначальные проверки
        Initial(config);

        // Загружаем архивы и распаковываем

        var archives = GetArchives(config);
        if (archives.Count == 0)
        {
            logger.LogInformation("Archives is empty");
            return;
        }
        
        await Parallel.ForEachAsync(
            archives, 
            new ParallelOptions {
                MaxDegreeOfParallelism = 4, 
                CancellationToken = token
            },
            async (archive, cts) => {
                await DownloadArchiveAsync(archive, cts);
                await UnpackArchiveAsync(archive, cts);
                Handler?.Invoke(this, new InstallEventArgs {
                    Type = InstallEventArgs.InstallType.Complete,
                    Identifier = archive.Checksum.Value,
                    IsCompleted = true
                });
            }
        );
        
        // Объединяем директории
        
        Handler?.Invoke(this, new InstallEventArgs {
            Type = InstallEventArgs.InstallType.Merge,
            Identifier = "0",
            Title = Strings.mw_progress_title_finalization,
            IsIndeterminate = true
        });
        
        await MergeAllContentAsync(archives, token);
        
        // Правим конфиг MO2
        await organizerService.ConfigureAsync(config.Metadata.Profile, token);
        
        // Правим версию в локальном конфиге
        storageService.Set(StorageServiceKey.Version, config.Metadata.LatestVersion);
    }
}

// Private Methods

public partial class InstallOrchestrator
{
    private List<RemoteConfigEntity.ArchiveItemEntity> GetArchives(RemoteConfigEntity config)
    {
        var rawCurrentVersion = storageService.GetString(StorageServiceKey.Version);
        var rawLatestVersion = config.Archives.Version;
        
        if (!Version.TryParse(rawLatestVersion, out var latestVersion))
        {
            logger.LogInformation("Some error in the config");
            return [];
        }

        if (!Version.TryParse(rawCurrentVersion, out var currentVersion))
        {
            logger.LogInformation("Clean installation");
            return config.Archives.Install.Concat(config.Archives.Patch).ToList();
        }

        if (currentVersion == latestVersion)
        {
            logger.LogInformation("Versions are equal — exiting, but... how did we even get here?");
            return [];
        }
        
        var install = config.Archives.Install
            .Where(entity => {
                if (!Version.TryParse(entity.Version, out var version))
                    return false;
                return version > currentVersion;
            })
            .ToList();
        
        var patch = config.Archives.Patch
            .Where(entity => {
                if (!Version.TryParse(entity.Patch?.FromVersion, out var fromVersion))
                    return false;
                return fromVersion >= currentVersion;
            })
            .ToList();
        
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Install Archives count is {installCount} / Patch Archives count is {patchCount}", install.Count, patch.Count);
        
        return install
            .Concat(patch)
            .ToList();
    } 
    
    private async Task DownloadArchiveAsync(RemoteConfigEntity.ArchiveItemEntity archive, CancellationToken token)
    {
        var fileName = Path.Combine(Constants.StorageDownloadFolder, archive.FileName);
        
        if (await InternalHashCheckAsync())
            return;

        var service = provider.GetRequiredService<IDownloaderService>();
        service.Handler = new Progress<DownloaderProgressEntity>(InternalHandler);
        await service.DownloadFileAsync(archive.Url, fileName, token);
        
        return;
        
        async Task<bool> InternalHashCheckAsync()
        {
            if (!File.Exists(fileName)) 
                return false;
            
            Handler?.Invoke(this, new InstallEventArgs {
                Type = InstallEventArgs.InstallType.Hash,
                Identifier = archive.Checksum.Value,
                Title = string.Format(Strings.mw_progress_title_hash, Path.GetFileName(fileName)), 
                IsIndeterminate = true
            });
            
            // Хеш совпадает, файл загружать не нужно
            if (await hashCheckerService.OnFileAsync(fileName, archive.Checksum.Value, token))
                return true;
                
            File.Delete(fileName);

            return false;
        }
        
        void InternalHandler(DownloaderProgressEntity entity)
        {
            if (entity.Result.IsCompleted)
                return;
            
            var received = new ByteSize(entity.ReceivedBytesSize).ToString();
            var total = new ByteSize(entity.TotalBytesToReceive).ToString();
            var speed = new ByteSize(entity.AverageBytesPerSecondSpeed).ToString();
            var eta = (int)Math.Ceiling((entity.TotalBytesToReceive - entity.ReceivedBytesSize) / entity.AverageBytesPerSecondSpeed);
            var etaAsString = TimeSpan.FromSeconds(eta).Humanize(precision: 1, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day, culture: CultureInfo.CurrentCulture);
            
            Handler?.Invoke(this, new InstallEventArgs {
                Type = InstallEventArgs.InstallType.Download,
                Identifier = archive.Checksum.Value,
                Title = string.Format(Strings.mw_progress_title_download, archive.FileName, $"{entity.ProgressPercentage:F}", received, total, speed, etaAsString),
                Value = entity.ProgressPercentage
            });
        }
    }
    
    private async Task UnpackArchiveAsync(RemoteConfigEntity.ArchiveItemEntity archive, CancellationToken token)
    {
        var fileName = Path.Combine(Constants.StorageDownloadFolder, archive.FileName);
        var directory = Path.Combine(Constants.StorageDownloadFolder, Path.GetFileNameWithoutExtension(archive.FileName));
        
        if (!File.Exists(fileName))
            throw new FileNotFoundException(fileName);
        
        await sevenZipService.ToFolderAsync(
            fileName, 
            directory,
            new Progress<byte>(value => {
                Handler?.Invoke(this, new InstallEventArgs {
                    Type = InstallEventArgs.InstallType.Unpack,
                    Identifier = archive.Checksum.Value,
                    Title = string.Format(Strings.mw_progress_title_unpack, archive.FileName, value),
                    Value = value
                });
            }), 
            token
        );
    }

    private async Task MergeAllContentAsync(List<RemoteConfigEntity.ArchiveItemEntity> archives, CancellationToken token)
    {
        foreach (var archive in archives)
        {
            var sourceDirectory = Path.Combine(Constants.StorageDownloadFolder, Path.GetFileNameWithoutExtension(archive.FileName));
            if (!Directory.Exists(sourceDirectory)) 
                continue;
            var destDirectory = Path.Combine(Constants.CurrentDirectory, archive.ExtractToFolder);
            await transferService.MoveDirectory(sourceDirectory, destDirectory, token);
        }
    }
}

// Private Methods (Static)

public partial class InstallOrchestrator
{
    private static void Initial(RemoteConfigEntity config)
    {
        if (FreeSpaceIsNotAvailable(config))
            throw new FreeSpaceIsNotAvailableException();
    }
    
    private static bool FreeSpaceIsNotAvailable(RemoteConfigEntity config)
    {
        var disk = new DriveInfo(Constants.CurrentDirectory);
        return disk.AvailableFreeSpace <= config.Size.DownloadBytes + config.Size.InstallBytes;
    }
}

// Private Methods (Logger)

public partial class InstallOrchestrator
{
    [LoggerMessage(LogLevel.Information, "{msg}")]
    private partial void LogInfo(string msg);
    
    [LoggerMessage(LogLevel.Error)]
    private partial void LogError(Exception ex);
}

// Extensions

public partial class InstallOrchestrator
{
    public class FreeSpaceIsNotAvailableException(
        string message = "Not enough free disk space."
    ) : Exception(message);
}