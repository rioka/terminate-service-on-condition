using System.Globalization;
using System.Security.Cryptography;

namespace TerminateServiceOnCondition;

internal class FileResourceChecksum : IResourceChecksum
{
  public string Resource { get; }
  
  public FileResourceChecksum(string resource)
  {
    Resource = resource;
  }

  public async Task<string?> GetChecksum(CancellationToken cancellationToken)
  {
    if (!await Exists(cancellationToken))
    {
      return null;
    }

    using var fs = new FileStream(Resource, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    var hash = await SHA256.HashDataAsync(fs, cancellationToken);
    
    return BitConverter.ToString(hash);
  }

  public Task<bool> Exists(CancellationToken cancellationToken) => Task.FromResult(File.Exists(Resource));
}