using Newtonsoft.Json;
using TiAnomalyInstaller.Logic.Configuration.Entities;

namespace TiAnomalyInstaller.Logic.Configuration;

public interface IConfigurationService
{
    public LocalConfigEntity GetLocalConfig(string fileName);
    public Task<RemoteConfigEntity> ObtainRemoteConfigAsync(string url);
}

public class ConfigurationService: IConfigurationService
{
    public LocalConfigEntity GetLocalConfig(string fileName)
    {
        var rawContent = File.ReadAllText(
            Path.Combine(
                Environment.CurrentDirectory, 
                fileName
            )
        );
        
        if (JsonConvert.DeserializeObject<LocalConfigEntity>(rawContent) is { } entity)
            return entity;

        throw new NullReferenceException();
    }
    
    public async Task<RemoteConfigEntity> ObtainRemoteConfigAsync(string url)
    {
        var client = new HttpClient();
        var rawContent = await client.GetStringAsync(url);
        
        if (JsonConvert.DeserializeObject<RemoteConfigEntity>(rawContent) is { } entity)
            return entity;
        
        throw new NullReferenceException();
    }
}