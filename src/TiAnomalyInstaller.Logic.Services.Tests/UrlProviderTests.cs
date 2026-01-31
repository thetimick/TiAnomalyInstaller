// ⠀
// UrlProviderTests.cs
// TiAnomalyInstaller.Logic.Services.Tests
// 
// Created by the_timick on 31.01.2026.
// ⠀

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TiAnomalyInstaller.AppConstants;
using TiAnomalyInstaller.Logic.Services.Providers;
using TiAnomalyInstaller.Logic.Services.Tests.Helpers;

namespace TiAnomalyInstaller.Logic.Services.Tests;

public class UrlProviderTests
{
    private readonly ILogger<UrlProvider> _logger = Substitute.For<ILogger<UrlProvider>>();
    
    [Fact]
    public async Task DirectUrl()
    {
        var client = MockHttpClient.Make();
        var provider = new UrlProvider(client, _logger);
        
        (await provider.ObtainUrl("https://google.com/file.txt")).Should()
            .BeEquivalentTo("https://google.com/file.txt");
    }

    [Fact]
    public async Task YandexUrlIsSuccess()
    {
        const string raw = "https://disk.yandex.ru/d/KxZEKMAOXPgcTA";
        const string expect = "https://google.com";
        var mockResourcesUrl = Constants.Utils.YandexDiskResourcesApi
            .Replace("<key>", raw);

        var client = MockHttpClient.Make(
            mockResourcesUrl, 
            $"{{ \"href\" : \"{expect}\" }}"
        );
        var provider = new UrlProvider(client, _logger);
        
        (await provider.ObtainUrl(raw)).Should()
            .BeEquivalentTo(expect);
    }
    
    [Fact]
    public async Task YandexUrlIsError()
    {
        const string raw = "https://disk.yandex.ru/d/KxZEKMAOXPgcTA";
        var mockResourcesUrl = Constants.Utils.YandexDiskResourcesApi
            .Replace("<key>", raw);
        
        var client = MockHttpClient.Make(
            mockResourcesUrl, 
            "{ \"href_error\" : \"https://google.com\" }"
        );
        var provider = new UrlProvider(client, _logger);
        
        (await provider.ObtainUrl(raw)).Should()
            .BeEquivalentTo(raw);
    }
    
    [Fact]
    public async Task GoogleUrl()
    {
        const string raw = "https://drive.google.com/file/d/1Cxky-WySUOS_wbyEgeFPHy2MC6KRJsAl/view?usp=sharing";
        var expect = Constants.Utils.GoogleDriveTemplateUrl
            .Replace("<file_id>", "1Cxky-WySUOS_wbyEgeFPHy2MC6KRJsAl");
        
        var client = MockHttpClient.Make();
        var provider = new UrlProvider(client, _logger);

        (await provider.ObtainUrl(raw)).Should()
            .BeEquivalentTo(expect);
    }
}