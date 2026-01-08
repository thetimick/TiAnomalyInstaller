using Newtonsoft.Json;
using TiAnomalyInstaller.AppConstants;
using TiAnomalyInstaller.Logic.Configuration.Entities;

namespace TiAnomalyInstaller.Logic.Configuration;

public interface IConfigurationService
{
    public (LocalConfigEntity local, RemoteConfigEntity remote)? Cached { get; }
    public Task PrepareAsync(string fileName);
}

public class ConfigurationService: IConfigurationService
{
    public (LocalConfigEntity local, RemoteConfigEntity remote)? Cached { get; private set; }

    public async Task PrepareAsync(string fileName)
    {
        var localConfig = GetLocalConfig(fileName);
        var remoteConfig = await ObtainRemoteConfigAsync(localConfig.ConfigUrl);
        Cached = (localConfig, remoteConfig);
    }

    private static LocalConfigEntity GetLocalConfig(string fileName)
    {
        var rawContent = File.ReadAllText(
            Path.Combine(
                Constants.CurrentDirectory, 
                fileName
            )
        );
        
        if (JsonConvert.DeserializeObject<LocalConfigEntity>(rawContent) is { } entity)
            return entity;

        throw new NullReferenceException();
    }
    
    private static async Task<RemoteConfigEntity> ObtainRemoteConfigAsync(string url)
    {
        var client = new HttpClient();
        var rawContent = await client.GetStringAsync(url);
        
        if (JsonConvert.DeserializeObject<RemoteConfigEntity>(rawContent) is { } entity)
            return entity;
        
        throw new NullReferenceException();
    }
}