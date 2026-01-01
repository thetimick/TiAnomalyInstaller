// ⠀
// LocalConfigEntity.cs
// TiAnomalyInstaller.Logic.Configuration
// 
// Created by the_timick on 01.01.2026.
// ⠀

namespace TiAnomalyInstaller.Logic.Configuration.Entities;

public record LocalConfigEntity
{
    public string Title { get; init; } = string.Empty;
    public string ConfigUrl { get; init; } = string.Empty;
}