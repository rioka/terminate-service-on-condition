namespace TerminateServiceOnCondition;

internal class ResourceWatcher : BackgroundService
{
  #region Data

  private readonly IHostApplicationLifetime _lifetime;
  private readonly ILogger<ResourceWatcher> _logger;
  private readonly Dictionary<string, string?> _resources ;

  #endregion

  public ResourceWatcher(IEnumerable<string> resources, IHostApplicationLifetime lifetime, ILogger<ResourceWatcher> logger)
  {
    _lifetime = lifetime;
    _logger = logger;
    _resources = resources
      .Select(f => new FileChecksum(f))
      .ToDictionary(x => x.Filename, x => x.Checksum.Value);
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
        _logger.LogInformation($"{scanResult.Filename} has changed, terminating...");
        _lifetime.StopApplication();
      } 
      else
      {
        _logger.LogInformation("No changes detected in target files.");
      }
            
      await Task.Delay(1000, stoppingToken);
    }
  }

  #region Internals

  private static (bool Changed, string Filename) ChecksumChanged(string file, string? previousChecksum)
  {
    var fileExists = File.Exists(file);
    if (!fileExists)
    {
      // If the file does not exist, but we have a previous checksum, it means the file was deleted
      return (previousChecksum != null, file);
    }

    // If file exists, but we have no previous checksum, it means it's a new file
    if (fileExists && previousChecksum is null)
    {
      return (true, file);
    }

    // In any other case, compare checksums
    var checksum = new FileChecksum(file).Checksum.Value;
    return (checksum != previousChecksum!, file);  
  }

  #endregion
}