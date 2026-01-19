// ⠀
// StorageServiceUnitTests.cs
// TiAnomalyInstaller.Logic.Services.Tests
// 
// Created by the_timick on 17.01.2026.
//

using Microsoft.Extensions.Logging.Abstractions;

namespace TiAnomalyInstaller.Logic.Services.Tests;

public class StorageServiceUnitTests
{
    private static string TempFolderName => Path.GetTempPath();
    private static string TempFileName => Path.Combine(TempFolderName, "storage.json");
    
    private static StorageService MakeStorageService() => new(
        TempFileName, 
        NullLogger<StorageService>.Instance
    );
    
    // ────────────────────────────────────────────────
    // Tests
    // ────────────────────────────────────────────────
    
    [Fact]
    public void FunctionalityTest()
    {
        const string expected = "http://localhost:8080";
        var storage = MakeStorageService();
        storage.Set(StorageServiceKey.ProfileUrl, expected);
        storage.Save();
        storage.Clear();
        storage.Reload();
        var value = storage.GetString(StorageServiceKey.ProfileUrl);
        Assert.Equal(expected, value);
    }
}