// ⠀
// MockHttpClient.cs
// TiAnomalyInstaller.Logic.Services.Tests
// 
// Created by the_timick on 01.02.2026.
// ⠀

using RichardSzalay.MockHttp;

namespace TiAnomalyInstaller.Logic.Services.Tests.Helpers;

public static class MockHttpClient
{
    public static HttpClient Make()
    {
        return new MockHttpMessageHandler().ToHttpClient();
    }
    
    public static HttpClient Make(string when, string json)
    {
        var handler = new MockHttpMessageHandler();
        handler.When(when).Respond("application/json", json);
        return handler.ToHttpClient();
    }
}