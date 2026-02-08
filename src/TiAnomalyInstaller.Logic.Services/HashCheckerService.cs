// ⠀
// HashCheckerService.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 07.01.2026.
// ⠀

using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace TiAnomalyInstaller.Logic.Services;

public interface IHashCheckerService
{
    public Task<string?> ComputeFileHashAsync(string fileName, CancellationToken token = default);
    public Task<string?> ComputeStreamHashAsync(Stream stream, CancellationToken token = default);
    public Task<bool> OnFileAsync(string fileName, string checksum, CancellationToken token = default);
    public Task<bool> OnStreamAsync(Stream stream, string checksum, CancellationToken token = default);
}

public partial class HashCheckerService(ILogger<HashCheckerService> logger): IHashCheckerService
{
    public IProgress<double>? Handler { get; set; }

    public Task<string?> ComputeFileHashAsync(string fileName, CancellationToken token)
    {
        return GetFileHashAsync(fileName, token);
    }
    
    public Task<string?> ComputeStreamHashAsync(Stream stream, CancellationToken token)
    {
        return GetStreamHashAsync(stream, token);
    }

    public async Task<bool> OnFileAsync(string fileName, string checksum, CancellationToken token)
    {
        if (await GetFileHashAsync(fileName, token)  is not { } hash)
        {
            LogInfo($"Failed to calculate hash for file '{fileName}'.");
            return false;
        }

        if (string.Equals(hash, checksum, StringComparison.OrdinalIgnoreCase)) 
            return true;
        
        LogInfo(
            $"Checksum mismatch for file '{fileName}'. " +
            $"Actual: {hash}, Expected: {checksum}."
        );
        
        return false;
    }

    public async Task<bool> OnStreamAsync(Stream stream, string checksum, CancellationToken token = default)
    {
        if (await GetStreamHashAsync(stream, token) is { } hash)
            return string.Equals(hash, checksum, StringComparison.CurrentCultureIgnoreCase);
        return false;
    }
}

// Private Methods

public partial class HashCheckerService
{
    private static async Task<string?> GetFileHashAsync(string fileName, CancellationToken token)
    {
        if (!File.Exists(fileName))
            return null;
        await using var stream = File.OpenRead(fileName);
        var bytes = await MD5.HashDataAsync(stream, token);
        return Convert.ToHexString(bytes).ToUpper();
    }

    private static async Task<string?> GetStreamHashAsync(Stream stream, CancellationToken token)
    {
        var bytes = await MD5.HashDataAsync(stream, token);
        return Convert.ToHexString(bytes).ToUpper();
    }
}

// Private Methods

public partial class HashCheckerService
{
    [LoggerMessage(LogLevel.Information, "{msg}")]
    private partial void LogInfo(string msg);
}