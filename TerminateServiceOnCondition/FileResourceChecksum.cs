using System.Security.Cryptography;

namespace TerminateServiceOnCondition;

internal class FileResourceChecksum : IResourceChecksum
{
  public string Resource { get; }
    
  public Lazy<string?> Checksum { get; } 
  
  public bool Exists => File.Exists(Resource);
  
  public FileResourceChecksum(string resource)
  {
    Resource = resource;
    Checksum = new Lazy<string?>(() => GetChecksum());;
  }

  private string? GetChecksum()
  {
    if (!Exists)
    {
      return null;
    }

    using var fs = new FileStream(Resource, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

    return BitConverter.ToString(SHA256.HashData(fs));
  }
}