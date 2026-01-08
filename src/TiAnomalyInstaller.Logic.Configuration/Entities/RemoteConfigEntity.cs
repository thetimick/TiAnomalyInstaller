// ⠀
// RemoteConfigEntity.cs
// TiAnomalyInstaller.Logic.Configuration
// 
// Created by the_timick on 01.01.2026.
// ⠀

using JetBrains.Annotations;

namespace TiAnomalyInstaller.Logic.Configuration.Entities;

public record RemoteConfigEntity
{
    [UsedImplicitly]
    public record ArchiveEntity
    {
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

    public record HashEntity(
        string ArchiveChecksumsUrl, 
        string GameChecksumsUrl
    );

    public string Profile  { get; init; } = string.Empty;
    public HashEntity Hash { get; init; } = new(string.Empty, string.Empty);
    public List<ArchiveEntity> Archives { get; init; } = [];
}