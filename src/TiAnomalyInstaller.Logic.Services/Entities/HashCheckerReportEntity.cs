// ⠀
// HashCheckerReportEntity.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 07.01.2026.
// ⠀

using System.Collections.Concurrent;

namespace TiAnomalyInstaller.Logic.Services.Entities;

public partial record HashCheckerReportEntity(
    ConcurrentBag<string> Complete,
    ConcurrentBag<HashCheckerReportEntity.ErrorEntity> Error,
    ConcurrentBag<string> NotFound
);

public partial record HashCheckerReportEntity
{
    public record ErrorEntity(
        string Path, 
        string HashByFile,
        string HashByServer
    );
}