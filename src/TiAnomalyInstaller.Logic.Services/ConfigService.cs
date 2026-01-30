// ⠀
// ConfigService.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 17.01.2026.
// ⠀

using System.Text.Json;
using Microsoft.Extensions.Logging;
using TiAnomalyInstaller.Logic.Services.Entities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TiAnomalyInstaller.Logic.Services;

public interface IConfigService
{
    public RemoteConfigEntity? Cached { get; }
    public Task<RemoteConfigEntity> ObtainRemoteConfigAsync(string url, bool force);
}

public class ConfigService(
    HttpClient client, 
    ILogger<ConfigService> logger
 ): IConfigService {
    public RemoteConfigEntity? Cached { get; private set; }
    
    public async Task<RemoteConfigEntity> ObtainRemoteConfigAsync(string url, bool force)
    {
        try
        {
            if (Cached != null && !force)
                return Cached;
            var content = await client.GetStringAsync(url);
            Cached = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build()
                .Deserialize<RemoteConfigEntity>(content);
            return Cached ?? throw new NullReferenceException();
        }
        catch (Exception ex)
        {
            logger.LogError("{ex}", ex);
            throw;
        }
    }
}