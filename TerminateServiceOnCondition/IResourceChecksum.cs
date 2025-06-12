namespace TerminateServiceOnCondition
{
  internal interface IResourceChecksum
  {
    string Resource { get; }

    Lazy<string?> Checksum { get; }
    
    bool Exists { get; }
  }
}