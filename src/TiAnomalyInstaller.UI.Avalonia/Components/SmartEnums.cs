// ⠀
// SmartEnums.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 13.01.2026.
// ⠀

using Ardalis.SmartEnum;

namespace TiAnomalyInstaller.UI.Avalonia.Components;

public sealed class ViewStateType : SmartEnum<ViewStateType>
{
    public static readonly ViewStateType None = new(nameof(None), 0);
    public static readonly ViewStateType InstallAvailable = new(nameof(InstallAvailable), 1);
    public static readonly ViewStateType PlayAvailable = new(nameof(PlayAvailable), 2);
    public static readonly ViewStateType InProgress = new(nameof(InProgress), 3);
    public static readonly ViewStateType Cancelling = new(nameof(Cancelling), 4);

    private ViewStateType(string name, int value) : base(name, value) { }
}

public sealed class ViewMainButtonType : SmartEnum<ViewMainButtonType>
{
    public static readonly ViewMainButtonType Play = new(nameof(Play), 0);
    public static readonly ViewMainButtonType Install = new(nameof(Install), 1);
    public static readonly ViewMainButtonType Settings = new(nameof(Settings), 2);
    public static readonly ViewMainButtonType Cancel = new(nameof(Cancel), 3);
    public static readonly ViewMainButtonType Exit = new(nameof(Exit), 4);
    
    public ViewMainButtonType(string name, int value) : base(name, value) { }
}