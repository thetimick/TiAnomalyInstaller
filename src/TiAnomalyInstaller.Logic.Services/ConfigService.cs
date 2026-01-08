// ⠀
// ConfigService.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 07.01.2026.
// ⠀

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TiAnomalyInstaller.AppConstants;
using TiAnomalyInstaller.Logic.Services.Entities.ConfigService;
using Tomlyn;

namespace TiAnomalyInstaller.Logic.Services;

public interface IConfigService
{
    public LocalConfigEntity? LocalCached { get; }
    public RemoteConfigEntity? RemoteCached { get; }
    
    public LocalConfigEntity GetLocalConfig();
    public Task<RemoteConfigEntity> ObtainRemoteConfig();

    public void SaveLocalConfig();
}

public class ConfigService(
    HttpClient client, 
    ILogger<ConfigService> logger
): IConfigService {
    public LocalConfigEntity? LocalCached { get; private set; }
    public RemoteConfigEntity? RemoteCached { get; private set; }

    public LocalConfigEntity GetLocalConfig()
    {
        if (LocalCached != null)
            return LocalCached;
        var content = File.ReadAllText(Constants.Files.LocalConfigFileName);
        LocalCached = Toml.ToModel<LocalConfigEntity>(content);
        
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("LocalConfig loaded!");
        return LocalCached;
    }
    
    public async Task<RemoteConfigEntity> ObtainRemoteConfig()
    {
        if (RemoteCached != null)
            return RemoteCached;
        var local = GetLocalConfig();
        var content = await client.GetStringAsync(local.Url);
        RemoteCached = JsonConvert.DeserializeObject<RemoteConfigEntity>(content);
        
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("RemoteConfig loaded!");
        return RemoteCached ?? throw new NullReferenceException();
    }

    public void SaveLocalConfig()
    {
        if (LocalCached is { } config) 
            File.WriteAllText(Constants.Files.LocalConfigFileName, Toml.FromModel(config));
    }
}