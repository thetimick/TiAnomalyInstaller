// ⠀
// HashCheckerService.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 07.01.2026.
// ⠀

using System.Security.Cryptography;
using Downloader;
using TiAnomalyInstaller.AppConstants;
using TiAnomalyInstaller.Logic.Services.Entities;

namespace TiAnomalyInstaller.Logic.Services;

public interface IHashCheckerService
{
    public IProgress<double>? Handler { get; set; }
    
    public Task<bool> OnFileAsync(
        string fileName, 
        string checksum, 
        CancellationToken token
    );
    
    public Task<HashCheckerReportEntity> OnFolderAsync(
        string url, 
        string folderName,
        Constants.Files.Hash.ChecksumsType type,
        CancellationToken token
    );

    public Task<Dictionary<string, string>> LoadHashFromUrlAsync(
        string url, 
        string folderName,
        Constants.Files.Hash.ChecksumsType type,
        CancellationToken token
    );
}

public partial class HashCheckerService(ISevenZipService zipService): IHashCheckerService
{
    public IProgress<double>? Handler { get; set; }

    public async Task<bool> OnFileAsync(string fileName, string checksum, CancellationToken token)
    {
        if (await GetFileHashAsync(fileName, token) is { } hash)
            return string.Equals(hash, checksum, StringComparison.CurrentCultureIgnoreCase);
        return false;
    }
    
    public async Task<HashCheckerReportEntity> OnFolderAsync(
        string url, 
        string folderName,
        Constants.Files.Hash.ChecksumsType type,
        CancellationToken token
    ) {
        var checksums = await LoadHashFromUrlAsync(url, folderName, type, token);
        var report = new HashCheckerReportEntity([], [], []);
        var current = 0;
        var total = checksums.Count;
        
        await Parallel.ForEachAsync(checksums, token, async (pair, _) => {
            if (await GetFileHashAsync(pair.Key, token) is not { } hash)
                report.NotFound.Add(pair.Key);
            else if (string.Equals(hash.Trim(), pair.Value.Trim(), StringComparison.CurrentCultureIgnoreCase))
                report.Complete.Add(pair.Key);
            else
                report.Error.Add(new HashCheckerReportEntity.ErrorEntity(pair.Key, hash, pair.Value));
            
            Interlocked.Increment(ref current);
            Handler?.Report(total is 0 ? 0 : (double)current * 100 / total);
        });
        
        return report;
    }

    public async Task<Dictionary<string, string>> LoadHashFromUrlAsync(
        string url, 
        string folderName,
        Constants.Files.Hash.ChecksumsType type, 
        CancellationToken token
    ) {
        var archiveFileName = Constants.Files.Hash.GetPath(type, Constants.Files.Hash.FileType.Archive);
        var textFileName = Constants.Files.Hash.GetPath(type, Constants.Files.Hash.FileType.Text);

        await new DownloadService().DownloadFileTaskAsync(url, archiveFileName, token);
        await zipService.ToFolderAsync(archiveFileName, Constants.StorageDownloadFolder, token);
        
        var dictionary = (await File.ReadAllLinesAsync(textFileName, token))
            .Where(s => !s.StartsWith('#'))
            .Select(s =>
            {
                var split = s.Split('*');
                return new KeyValuePair<string, string>(
                    Path.Combine(folderName, split[1].Trim().Replace('/', '\\')),
                    split[0].Trim()
                );
            })
            .ToDictionary();

        File.Delete(archiveFileName);
        File.Delete(textFileName);
        
        return dictionary;
    }
}

// Private Methods

public partial class HashCheckerService
{
    private static async Task<string?> GetFileHashAsync(string fileName, CancellationToken token)
    {
        if (!File.Exists(fileName))
            return null;
        using var md5 = MD5.Create();
        await using var stream = File.OpenRead(fileName);
        var hash = await md5.ComputeHashAsync(stream, token);
        return Convert.ToHexString(hash).ToUpper();
    }
}