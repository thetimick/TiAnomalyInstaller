// ⠀
// LocalConfigEntity.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 07.01.2026.
// ⠀

using System.Text.Json.Serialization;
using Semver;

namespace TiAnomalyInstaller.Logic.Services.Entities.ConfigService;

public record LocalConfigEntity
{
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    
    [JsonIgnore]
    public SemVersion? ParsedVersion => SemVersion.TryParse(Version, out var version) 
        ? version 
        : null;
}