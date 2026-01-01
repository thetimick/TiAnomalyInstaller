// ⠀
// Constants.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 01.01.2026.
// ⠀

using System.Reflection;

namespace TiAnomalyInstaller.UI.Avalonia;

public static class Constants
{
    public static readonly string LocalConfigFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.LocalConfig.json";
}