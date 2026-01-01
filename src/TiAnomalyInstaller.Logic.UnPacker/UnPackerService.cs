using SharpSevenZip;

namespace TiAnomalyLauncher.Extractor;

public static class UnPackerService
{
    public static async Task ToFolderAsync(string fileName, string directory, EventHandler<byte>? progress = null)
    {
        using var extractor = new SharpSevenZipExtractor(fileName);
        if (progress != null)
            extractor.Extracting += (sender, args) => progress.Invoke(sender, args.PercentDone);
        await extractor.ExtractArchiveAsync(directory);
    }
}