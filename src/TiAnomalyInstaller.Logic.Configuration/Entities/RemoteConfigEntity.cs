// ⠀
// RemoteConfigEntity.cs
// TiAnomalyInstaller.Logic.Configuration
// 
// Created by the_timick on 01.01.2026.
// ⠀

namespace TiAnomalyInstaller.Logic.Configuration.Entities;

public record RemoteConfigEntity
{
    public record ArhiveEntity
    {
        public enum ArchiveType
        {
            Vanilla,
            Organizer
        }
        
        public string Url { get; init; } = string.Empty;
        public string FileName { get; init; } = string.Empty;
        public ArchiveType Type { get; init; } = ArchiveType.Vanilla;
    }

    public List<ArhiveEntity> Arhives { get; init; } = [];
}