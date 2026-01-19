// ⠀
// ConfigServiceV2.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 17.01.2026.
// ⠀

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TiAnomalyInstaller.Logic.Services.Entities.ConfigService;

namespace TiAnomalyInstaller.Logic.Services;

public interface IConfigServiceV2
{
    public RemoteConfigEntity? Cached { get; }
    public Task<RemoteConfigEntity> ObtainRemoteConfigAsync(string url, bool force);
}

public class ConfigServiceV2(
    HttpClient client, 
    ILogger<ConfigServiceV2> logger
 ): IConfigServiceV2 {
    public RemoteConfigEntity? Cached { get; private set; }
    
    public async Task<RemoteConfigEntity> ObtainRemoteConfigAsync(string url, bool force)
    {
        try
        {
            if (Cached != null && !force)
                return Cached;
            var content = await client.GetStringAsync(url);
            Cached = JsonConvert.DeserializeObject<RemoteConfigEntity>(content);
            return Cached ?? throw new NullReferenceException();
        }
        catch (Exception ex)
        {
            logger.LogError("{ex}", ex);
            throw;
        }
    }
}