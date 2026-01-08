// ⠀
// WatcherService.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 08.01.2026.
// ⠀

using Microsoft.Extensions.Logging;

namespace TiAnomalyInstaller.Logic.Services;

public interface IWatcherService : IDisposable
{
    public IReadOnlyDictionary<string, bool> FolderStates { get; }

    public event EventHandler<string> FolderAppeared;
    public event EventHandler<string> FolderDisappeared;

    public void Start(string parentPath, params string[] folderNames);
    public void Stop();
}

public sealed class WatcherService(ILogger<WatcherService> logger) : IWatcherService
{
    private FileSystemWatcher? _watcher;
    private readonly Dictionary<string, bool> _folderStates =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, bool> FolderStates => _folderStates;

    public event EventHandler<string>? FolderAppeared;
    public event EventHandler<string>? FolderDisappeared;

    public void Start(string parentPath, params string[] folderNames)
    {
        if (_watcher != null)
            throw new InvalidOperationException("Watcher already started");

        if (!Directory.Exists(parentPath))
            throw new DirectoryNotFoundException(parentPath);

        if (folderNames == null || folderNames.Length == 0)
            throw new ArgumentException("Folder list is empty", nameof(folderNames));

        _folderStates.Clear();

        foreach (var folder in folderNames.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var fullPath = Path.Combine(parentPath, folder);
            _folderStates[folder] = Directory.Exists(fullPath);
        }

        _watcher = new FileSystemWatcher(parentPath)
        {
            NotifyFilter = NotifyFilters.DirectoryName,
            IncludeSubdirectories = false,
            EnableRaisingEvents = true
        };

        _watcher.Created += OnCreated;
        _watcher.Deleted += OnDeleted;
        _watcher.Renamed += OnRenamed;
    }
    
    public void Stop()
    {
        if (_watcher == null)
            return;

        _watcher.EnableRaisingEvents = false;
        _watcher.Created -= OnCreated;
        _watcher.Deleted -= OnDeleted;
        _watcher.Renamed -= OnRenamed;
        _watcher.Dispose();
        _watcher = null;
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("OnCreated {path}", e.FullPath);
        
        var folderName = Path.GetFileName(e.FullPath);
        if (_folderStates.TryGetValue(folderName, out var state) && state)
            return;
        _folderStates[folderName] = true;
        FolderAppeared?.Invoke(this, folderName);
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("OnDeleted {path}", e.FullPath);
        
        var folderName = Path.GetFileName(e.FullPath);
        if (_folderStates.TryGetValue(folderName, out var state) && !state)
            return;
        _folderStates[folderName] = false;
        FolderDisappeared?.Invoke(this, folderName);
    }
    
    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("OnRenamed {Old} => {New}", e.OldFullPath, e.FullPath);
        
        var oldFolderName = Path.GetFileName(e.OldFullPath);
        var newFolderName = Path.GetFileName(e.FullPath);
        
        if (_folderStates.TryGetValue(oldFolderName, out _))
        {
            _folderStates[oldFolderName] = false;
            FolderDisappeared?.Invoke(this, oldFolderName);
            return;
        }
        
        // ReSharper disable once InvertIf
        if (_folderStates.TryGetValue(newFolderName, out _))
        {
            _folderStates[newFolderName] = true;
            FolderAppeared?.Invoke(this, newFolderName);
        }
    }

    public void Dispose() => Stop();
}