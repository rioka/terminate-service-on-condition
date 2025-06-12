namespace TerminateServiceOnCondition;

internal class ResourceWatcher : BackgroundService
{
  #region Data

  private readonly Func<string, IResourceChecksum> _resourceChecksumFactory;
  private readonly IHostApplicationLifetime _lifetime;
  private readonly ILogger<ResourceWatcher> _logger;
  private readonly Dictionary<string, string?> _resources ;

  #endregion

  public ResourceWatcher(
    IEnumerable<string> resources,
    Func<string, IResourceChecksum> resourceChecksumFactory,
    IHostApplicationLifetime lifetime, 
    ILogger<ResourceWatcher> logger)
  {
    _resourceChecksumFactory = resourceChecksumFactory;
    _lifetime = lifetime;
    _logger = logger;
    _resources = resources
      .Select(r => _resourceChecksumFactory(r))
      .ToDictionary(x => x.Resource, x => x.Checksum.Value);
  }
  
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      _logger.LogInformation("Checker running at: {time}", DateTime.UtcNow);
            
      var scanResult = _resources
        .Select(kvp => ChecksumChanged(kvp.Key, kvp.Value))
        .FirstOrDefault(x => x.Changed);

      if (scanResult.Changed)
      {
        _logger.LogInformation($"{scanResult.Resource} has changed, terminating...");
        _lifetime.StopApplication();
      } 
      else
      {
        _logger.LogInformation("No changes detected in target resources.");
      }
            
      await Task.Delay(1000, stoppingToken);
    }
  }

  #region Internals

  private (bool Changed, string Resource) ChecksumChanged(string resource, string? previousChecksum)
  {
    var newResource = _resourceChecksumFactory(resource);
    if (!newResource.Exists)
    {
      // If the resource does not exist, but we have a previous checksum, it means the resource has been deleted
      return (previousChecksum != null, resource);
    }

    // If resource exists, but we have no previous checksum, it means it's a new resource
    if (previousChecksum is null)
    {
      return (true, resource);
    }

    // In any other case, compare checksums
    return (newResource.Checksum.Value != previousChecksum, resource);  
  }

  #endregion
}