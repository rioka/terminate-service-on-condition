namespace TerminateServiceOnCondition;

internal class ResourceWatcher : BackgroundService
{
  #region Data

  private readonly Func<string, IResourceChecksum> _resourceChecksumFactory;
  private readonly IHostApplicationLifetime _lifetime;
  private readonly ILogger<ResourceWatcher> _logger;
  private readonly string[] _resources ;
  private readonly Dictionary<string, string?> _checksums = new Dictionary<string, string?>() ;

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
    _resources = resources.ToArray();
  }
  
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    foreach (var resource in _resources)
    {
      var resourceChecksum = _resourceChecksumFactory(resource);
      var checksum = await resourceChecksum.GetChecksum(stoppingToken);
      _checksums.Add(resource, checksum);
    }
    
    while (!stoppingToken.IsCancellationRequested)
    {
      _logger.LogInformation("Checker running at: {time}", DateTime.UtcNow);
      
      var changedResource = await ScanForChangedResources(stoppingToken);

      if (changedResource != null)
      {
        _logger.LogInformation($"{changedResource} has changed or is no longer available, terminating...");
        _lifetime.StopApplication();
      } 
      else
      {
        _logger.LogInformation("No changes detected in monitored resources.");
      }
            
      await Task.Delay(1000, stoppingToken);
    }
  }

  #region Internals

  private async Task<string?> ScanForChangedResources(CancellationToken cancellationToken)
  {
    foreach (var (resource, checksum) in _checksums)
    {
      var newResource = _resourceChecksumFactory(resource);
      var exists = await newResource.Exists(cancellationToken);
      if (!exists)
      {
        if (checksum != null)
        {
          return resource;
        }
        
        // the resource does not exist, and it did not exist before, so we can go and check the next one
        continue;
      }

      // If resource exists, but we have no previous checksum, it means it's a new resource:
      // we've found a changed resource, so we can return it
      if (checksum is null)
      {
        return resource;
      }
      
      // In any other case, compare checksums
      var currentChecksum = await newResource.GetChecksum(cancellationToken);
      // if checksum is different, the resource has changed, so we can return it
      if (currentChecksum != checksum)
      {
        return resource;
      }
    }
    
    // No changes detected in any of the resources
    return null;
  }
  
  #endregion
}