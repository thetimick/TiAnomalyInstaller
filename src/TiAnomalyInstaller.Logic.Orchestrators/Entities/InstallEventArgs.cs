// ⠀
// InstallEventArgs.cs
// TiAnomalyInstaller.Logic.Orchestrators
// 
// Created by the_timick on 10.01.2026.
// ⠀

namespace TiAnomalyInstaller.Logic.Orchestrators.Entities;

public class InstallEventArgs : EventArgs
{
    public enum InstallType
    {
        Hash,
        Download,
        Unpack,
        Merge,
        Complete
    }

    public InstallType Type { get; init; } = InstallType.Hash;
    
    public string Identifier { get; init; } = string.Empty;
    public bool IsCompleted { get; init; }
    
    public string Title { get; init; } = string.Empty;
    public double Value { get; init; }
    public bool IsIndeterminate { get; init; }
}