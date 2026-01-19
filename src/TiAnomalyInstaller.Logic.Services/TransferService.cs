// ⠀
// TransferService.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 12.01.2026.
// ⠀

namespace TiAnomalyInstaller.Logic.Services;

public interface ITransferService
{
    Task MoveDirectory(string source, string destination, CancellationToken token);
}

public partial class TransferService: ITransferService
{
    public async Task MoveDirectory(string source, string destination, CancellationToken token)
    {
        await MoveFolderContentsAsync(source, destination);
        if (Directory.Exists(source))
            Directory.Delete(source, true);
    }
}

// Private Methods

public partial class TransferService
{
    private static async Task MoveFolderContentsAsync(string sourceDir, string targetDir, bool overwrite = true)
    {
        if (!Directory.Exists(sourceDir))
            return;

        Directory.CreateDirectory(targetDir);

        var files = Directory.GetFiles(sourceDir, "*", SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(targetDir, fileName);

            await Task.Run(() => File.Move(file, destFile, overwrite));
        }

        var directories = Directory.GetDirectories(sourceDir);
        foreach (var dir in directories)
        {
            var dirName = Path.GetFileName(dir);
            var destDir = Path.Combine(targetDir, dirName);
            
            await MoveFolderContentsAsync(dir, destDir, overwrite);
            await Task.Run(() => Directory.Delete(dir));
        }
    }
}