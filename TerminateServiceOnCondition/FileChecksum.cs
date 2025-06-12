using System.Security.Cryptography;

namespace TerminateServiceOnCondition;

internal class FileChecksum
{
  public string Filename { get; }
    
  public Lazy<string?> Checksum { get; } 
  public FileChecksum(string file)
  {
    Filename = file;
    Checksum = new Lazy<string?>(() => GetChecksum());;
  }

  private string? GetChecksum()
  {
    if (!File.Exists(Filename))
    {
      return null;
    }

    using var fs = new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

    return BitConverter.ToString(SHA256.HashData(fs));
  }
}