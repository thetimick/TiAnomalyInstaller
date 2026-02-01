// ⠀
// ConfigServiceTests.cs
// TiAnomalyInstaller.Logic.Services.Tests
// 
// Created by the_timick on 01.02.2026.
// ⠀

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RichardSzalay.MockHttp;
using TiAnomalyInstaller.Logic.Services.Entities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TiAnomalyInstaller.Logic.Services.Tests;

public class ConfigServiceTests
{
    private readonly ILogger<ConfigService> _logger = Substitute.For<ILogger<ConfigService>>();
    private readonly RemoteConfigEntity _mockRemoteConfigEntity = new() {
        SchemaVersion = "1.0",
        Metadata = new RemoteConfigEntity.MetadataEntity {
            Title = "TEST"
        },
        Visual = new RemoteConfigEntity.VisualEntity() {
            BackgroundImage = null
        },
        Size = new RemoteConfigEntity.SizeInfoEntity(),
        Archives = new RemoteConfigEntity.ArchiveEntity()
    };

    [Fact]
    public async Task Obtain()
    {
        const string url = "https://google.com";
        var yaml = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build()
            .Serialize(_mockRemoteConfigEntity);
        
        var handler = new MockHttpMessageHandler();
        var request = handler.When(url).Respond("application/json", yaml);
        var client = handler.ToHttpClient();
        var service = new ConfigService(client, _logger);
        
        (await service.ObtainRemoteConfigAsync(url, false)).Should()
            .BeEquivalentTo(_mockRemoteConfigEntity);
        (await service.ObtainRemoteConfigAsync(url, false)).Should()
            .BeEquivalentTo(_mockRemoteConfigEntity);
        (await service.ObtainRemoteConfigAsync(url, true)).Should()
            .BeEquivalentTo(_mockRemoteConfigEntity);
        
        handler.GetMatchCount(request).Should()
            .Be(2);
    }
}