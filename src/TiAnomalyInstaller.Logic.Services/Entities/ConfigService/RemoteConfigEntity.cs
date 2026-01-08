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
    public string Profile  { get; init; } = string.Empty;
    public HashEntity Hash { get; init; } = new(string.Empty, string.Empty);
    public List<ArchiveEntity> Archives { get; init; } = [];
    public string Version { get; init; } = string.Empty;
    
    // Computed
    
    [JsonIgnore]
    public SemVersion ParsedVersion => SemVersion.Parse(Version);
}

// Nested

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

        public record OperationEntity
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum OperationType
            {
                DeleteFile,
                DeleteFolder,
                ClearFolder
            }
            
            public string Path { get; init; } = string.Empty;
            public OperationType Type { get; init; } = OperationType.DeleteFile;
        }
        
        public string Url { get; init; } = string.Empty;
        public string FileName { get; init; } = string.Empty;
        public ArchiveType Type { get; init; } = ArchiveType.Vanilla;
        public string Hash { get; init; } = string.Empty;
        public List<OperationEntity> Operations { get; init; } = [];
    }
}

public partial record RemoteConfigEntity
{
    public record HashEntity(
        string ArchiveChecksumsUrl, 
        string GameChecksumsUrl
    );
}