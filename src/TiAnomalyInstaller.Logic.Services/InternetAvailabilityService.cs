// ⠀
// InternetAvailabilityService.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 31.01.2026.
// ⠀

using System.Net;
using Microsoft.Extensions.Logging;
using TiAnomalyInstaller.AppConstants.Localization;

namespace TiAnomalyInstaller.Logic.Services;

public sealed class InternetUnavailableException() : Exception(Strings.throw_internet_unavailable);

public interface IInternetAvailabilityService
{
    Task<bool> HasInternetAsync(CancellationToken token = default);
}

public sealed class InternetAvailabilityService(
    ILogger<InternetAvailabilityService> logger
) : IInternetAvailabilityService {
    public async Task<bool> HasInternetAsync(CancellationToken token = default)
    {
        try
        {
            await Dns.GetHostEntryAsync("github.com", token);
            logger.LogInformation("Internet is available: github.com resolved");
            return true;
        }
        catch (OperationCanceledException ex) when (token.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Internet availability check was cancelled");
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to determine internet availability");
            return false;
        }
    }
}