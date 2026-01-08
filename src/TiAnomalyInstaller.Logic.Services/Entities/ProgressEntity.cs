// ⠀
// ProgressEntity.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 07.01.2026.
// ⠀

namespace TiAnomalyInstaller.Logic.Services.Entities;

public record ProgressEntity(
    double ProgressPercentage,
    double AverageBytesPerSecondSpeed,
    long ReceivedBytesSize,
    long TotalBytesToReceive,
    (bool IsCompleted, Exception? ex) Result
);