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
using TiAnomalyInstaller.Logic.Services.Entities.ConfigService;

namespace TiAnomalyInstaller.Logic.Orchestrators;

public interface IInstallOrchestrator : IOrchestrator<InstallEventArgs>;

public partial class InstallOrchestrator(
    IConfigService configService,
    IHashCheckerService hashCheckerService,
    IOrganizerService organizerService,
    IMoveService moveService,
    ILogger<InstallOrchestrator> logger,
    IServiceProvider provider
): IInstallOrchestrator {
    public EventHandler<InstallEventArgs>? Handler { get; set; }
    
    public async Task StartAsync(CancellationToken token = default)
    {
        // Загружаем конфиг
        var config = await configService.ObtainRemoteConfigAsync();

        // Первоначальные проверки
        Initial(config);

        // Загружаем архивы и распаковываем архивы

        await Parallel.ForEachAsync(
            config.Archives, 
            new ParallelOptions {
                MaxDegreeOfParallelism = 4, 
                CancellationToken = token
            },
            async (archive, cts) => {
                await DownloadArchiveAsync(archive, cts);
                await UnpackArchiveAsync(archive, cts);
                Handler?.Invoke(this, new InstallEventArgs {
                    Identifier = archive.Hash,
                    IsCompleted = true
                });
            }
        );
        
        // Объединяем директории
        
        Handler?.Invoke(this, new InstallEventArgs {
            Identifier = "0",
            Title = "Финализация...",
            IsIndeterminate = true
        });
        
        await MergeAllContentAsync(config, token);
        
        // Правим конфиг MO2
        await organizerService.ConfigureAsync(config.Profile, token);
        
        // Правим версию в локальном конфиге
        configService.LocalCached?.Version = config.Version;
    }
}

// Private Methods

public partial class InstallOrchestrator
{
    private async Task DownloadArchiveAsync(RemoteConfigEntity.ArchiveEntity archive, CancellationToken token)
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
                Identifier = archive.Hash,
                Title = string.Format(Strings.mw_progress_title_hash, Path.GetFileName(fileName)), 
                IsIndeterminate = true
            });
            
            // Хеш совпадает, файл загружать не нужно
            if (await hashCheckerService.OnFileAsync(fileName, archive.Hash, token))
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
                Identifier = archive.Hash,
                Title = string.Format(Strings.mw_progress_title_download, archive.FileName, $"{entity.ProgressPercentage:F}", received, total, speed, etaAsString),
                Value = entity.ProgressPercentage
            });
        }
    }
    
    private async Task UnpackArchiveAsync(RemoteConfigEntity.ArchiveEntity archive, CancellationToken token)
    {
        var fileName = Path.Combine(Constants.StorageDownloadFolder, archive.FileName);
        var directory = Path.Combine(Constants.StorageDownloadFolder, Path.GetFileNameWithoutExtension(archive.FileName));

        if (!File.Exists(fileName))
            throw new FileNotFoundException(fileName);
        
        var service = provider.GetRequiredService<ISevenZipService>();
        service.Handler = new Progress<byte>(InternalHandler);
        await service.ToFolderAsync(fileName, directory, token);
        
        return;

        void InternalHandler(byte progress)
        {
            Handler?.Invoke(this, new InstallEventArgs {
                Identifier = archive.Hash,
                Title = string.Format(Strings.mw_progress_title_unpack, archive.FileName, progress),
                Value = progress
            });
        }
    }

    private async Task MergeAllContentAsync(RemoteConfigEntity config, CancellationToken token)
    {
        foreach (var archive in config.Archives)
        {
            var sourceDirectory = Path.Combine(Constants.StorageDownloadFolder, Path.GetFileNameWithoutExtension(archive.FileName));
            if (!Directory.Exists(sourceDirectory)) 
                continue;
            
            var destDirectory = archive.Type switch {
                RemoteConfigEntity.ArchiveEntity.ArchiveType.Vanilla => Constants.VanillaFolderName,
                RemoteConfigEntity.ArchiveEntity.ArchiveType.Organizer => Constants.OrganizerFolderName,
                _ => string.Empty
            };

            await moveService.MoveDirectory(sourceDirectory, destDirectory, token);
        }
    }
}

// Private Methods (Static)

public partial class InstallOrchestrator
{
    private static void Initial(RemoteConfigEntity config)
    {
        if (ArchiveListIsEmpty(config))
            throw new ArchiveListIsEmptyException();
        if (FreeSpaceIsNotAvailable(config))
            throw new FreeSpaceIsNotAvailableException();
    }
    
    private static bool ArchiveListIsEmpty(RemoteConfigEntity config)
    {
        return config.Archives.Count == 0;
    } 
    
    private static bool FreeSpaceIsNotAvailable(RemoteConfigEntity config)
    {
        var disk = new DriveInfo(Constants.CurrentDirectory);
        return disk.AvailableFreeSpace <= config.Size.SizeForInstall;
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
    public class ArchiveListIsEmptyException(
        string message = "Archive list is empty."
    ) : Exception(message);

    public class FreeSpaceIsNotAvailableException(
        string message = "Not enough free disk space."
    ) : Exception(message);
}