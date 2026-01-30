// ⠀
// HostedService.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 07.01.2026.
// ⠀

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TiAnomalyInstaller.AppConstants;
using TiAnomalyInstaller.Logic.Services.Entities;

namespace TiAnomalyInstaller.Logic.Services;

public class HostedService(
    IStorageService storageService,
    IConfigService configService,
    IHashCheckerService hashCheckerService,
    IInMemoryStorageService inMemoryStorageService,
    HttpClient client,
    ILogger<HostedService> logger
): IHostedService {
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var url = storageService.GetString(StorageServiceKey.ProfileUrl) ?? string.Empty;
            var remote = await configService.ObtainRemoteConfigAsync(url, false);
            await PreloadCustomBackgroundImageIfNeededAsync(remote);
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError("{ex}", ex);
            inMemoryStorageService.SetValue(InMemoryStorageKey.ConfigError, ex);
        }
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        storageService.Save();
        return Task.CompletedTask;
    }
    
    // Private Methods

    private async Task PreloadCustomBackgroundImageIfNeededAsync(RemoteConfigEntity config)
    {
        try
        {
            var fileName = Constants.Files.BackgroundFileName;
            
            // Если нет URL - используем зашитую картинку
            if (config.Visual.BackgroundImage is not { } url)
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
                return;
            }
            
            // Если файла нет - загружаем
            if (!File.Exists(fileName))
            {
                await File.WriteAllBytesAsync(
                    fileName,
                    await client.GetByteArrayAsync(url)
                );
                return;
            }
            
            // Если файл есть - сверяем хеш
            
            var bytes = await client.GetByteArrayAsync(url);
            
            // Совпадает
            await using var stream = new MemoryStream(bytes);
            if (await hashCheckerService.ComputeStreamHashAsync(stream) is { } hash && await hashCheckerService.OnFileAsync(fileName, hash)) 
                return;
            
            // Не совпадает
            File.Delete(fileName);
            await File.WriteAllBytesAsync(fileName, bytes);
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError("{ex}", ex);
        }
    }
}