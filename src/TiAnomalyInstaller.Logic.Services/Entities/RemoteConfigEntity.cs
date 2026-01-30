// ⠀
// RemoteConfigEntity.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 21.01.2026.
// ⠀

using System.Text.Json.Serialization;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TiAnomalyInstaller.Logic.Services.Entities;

public sealed partial record RemoteConfigEntity
{
    public string SchemaVersion { get; init; } = null!;
    public MetadataEntity Metadata { get; init; } = null!;
    public VisualEntity Visual { get; init; } = null!;
    public SizeInfoEntity Size { get; init; } = null!;
    public ArchiveEntity Archives { get; init; } = null!;
}

public sealed partial record RemoteConfigEntity
{
    public sealed record MetadataEntity
    {
        public string Title { get; init; } = null!;
        public string Profile { get; init; } = null!;
        public string LatestVersion { get; init; } = null!;
    }
    
    public sealed record VisualEntity
    {
        public string? BackgroundImage { get; init; } = null!;
    }
    
    public sealed record SizeInfoEntity
    {
        public long DownloadBytes { get; init; }
        public long InstallBytes { get; init; }
        
        public long OverallBytes => DownloadBytes + InstallBytes;
    }

    public sealed record ArchiveEntity
    {
        public List<ArchiveItemEntity> Install { get; init; } = null!;
        public List<ArchiveItemEntity> Patch { get; init; } = null!;

        [JsonIgnore]
        public string? Version => Patch.Last().Patch?.ToVersion ?? Install.Last().Version;
    }
    
    public sealed partial record ArchiveItemEntity
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum ArchiveTypeEnum
        {
            Install,
            Patch
        }
        
        public string DisplayName { get; init; } = null!;
        public ArchiveTypeEnum ArchiveType { get; init; }
        public string Url { get; init; } = null!;
        public string FileName { get; init; } = null!;
        public string ExtractToFolder { get; init; } = null!;
        public string? Version { get; init; }
        public PatchInfoEntity? Patch { get; init; }
        public ChecksumEntity Checksum { get; init; } = null!;
    }

    public sealed partial record ArchiveItemEntity
    {
        public sealed record PatchInfoEntity
        {
            public string FromVersion { get; init; } = null!;
            public string ToVersion   { get; init; } = null!;
        }

        public sealed record ChecksumEntity
        {
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public enum ChecksumAlgorithmEnum
            {
                MD5
            }
            
            public ChecksumAlgorithmEnum Algorithm { get; init; }
            public string Value { get; init; } = null!;
        }
    }
}