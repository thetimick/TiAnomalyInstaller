// ⠀
// SevenZipService.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 08.01.2026.
// ⠀

namespace TiAnomalyInstaller.Logic.Services.Services.SevenZip;

/// <summary>
/// Wrapper над SharpSevenZip
/// </summary>
public interface ISevenZipService
{
    public Task ToFolderAsync(string fileName, string folderName, IProgress<byte> progress, CancellationToken token);
}

public class SevenZipService(ISharpSevenZipExtractorFactory factory): ISevenZipService
{
    public async Task ToFolderAsync(string fileName, string folderName, IProgress<byte> progress, CancellationToken token)
    {
        using var extractor = factory.MakeExtractor(fileName);
        extractor.Extracting += (_, args) => progress.Report(args.PercentDone);
        await extractor.ExtractArchiveAsync(folderName);
    }
}