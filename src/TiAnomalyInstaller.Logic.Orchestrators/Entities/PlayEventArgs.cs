// ⠀
// PlayEventArgs.cs
// TiAnomalyInstaller.Logic.Orchestrators
// 
// Created by the_timick on 11.01.2026.
// ⠀

namespace TiAnomalyInstaller.Logic.Orchestrators.Entities;

public class PlayEventArgs(
    string msg = "",
    bool isCompleted = false
) : EventArgs {
    
    public string Message  { get; } = msg;
    public bool IsCompleted { get; } = isCompleted;
}