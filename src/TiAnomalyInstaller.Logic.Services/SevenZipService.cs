// ⠀
// SevenZipService.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 08.01.2026.
// ⠀

using SharpSevenZip;

namespace TiAnomalyInstaller.Logic.Services;

/// <summary>
/// Wrapper над SharpSevenZip
/// </summary>
public interface ISevenZipService
{
    /// <summary>
    /// Прогресс в процентах (0-100)
    /// </summary>
    public IProgress<byte>? Handler { get; set; }

    /// <summary>
    /// Распаковка архива в папку
    /// </summary>
    /// <param name="fileName">Полный путь до архива</param>
    /// <param name="folderName">Полный путь до папки</param>
    /// <param name="token"></param>
    /// <returns>Task</returns>
    public Task ToFolderAsync(string fileName, string folderName, CancellationToken token);
}

public class SevenZipService: ISevenZipService
{
    public IProgress<byte>? Handler { get; set; }
    
    public async Task ToFolderAsync(string fileName, string folderName, CancellationToken token)
    {
        using var extractor = new SharpSevenZipExtractor(fileName);
        extractor.Extracting += (_, args) => Handler?.Report(args.PercentDone);
        await Task.Factory.StartNew(() => extractor.ExtractArchive(folderName), token);
        Handler = null;
    }
}