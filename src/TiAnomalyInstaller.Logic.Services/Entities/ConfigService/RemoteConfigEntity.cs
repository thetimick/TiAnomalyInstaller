// ⠀
// RemoteConfigEntity.cs
// TiAnomalyInstaller.Logic.Configuration
// 
// Created by the_timick on 01.01.2026.
// ⠀

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Semver;

// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TiAnomalyInstaller.Logic.Services.Entities.ConfigService;

public partial record RemoteConfigEntity
{
    public string? CustomBackgroundImageUrl { get; init; }
    public string Profile { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public SizeEntity Size { get; init; } = new(-1, -1);
    public List<ArchiveEntity> Archives { get; init; } = [];
    
    // Computed
    
    [JsonIgnore]
    public SemVersion ParsedVersion => SemVersion.Parse(Version);
}

// Nested

// Size

public partial record RemoteConfigEntity
{
    public record SizeEntity(long Size, long SizeForInstall);
}

// Archive

public partial record RemoteConfigEntity
{
    public record ArchiveEntity
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ArchiveType
        {
            Vanilla,
            Organizer
        }
        
        public string Url { get; init; } = string.Empty;
        public string FileName { get; init; } = string.Empty;
        public ArchiveType Type { get; init; } = ArchiveType.Vanilla;
        public string Hash { get; init; } = string.Empty;
    }
}