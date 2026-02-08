// ⠀
// ILoadable.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 08.02.2026.
// ⠀

using System.Threading.Tasks;

namespace TiAnomalyInstaller.UI.Avalonia.Components.Interfaces;

public interface ILoadable
{
    Task LoadContentAsync();
}