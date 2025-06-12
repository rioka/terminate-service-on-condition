namespace TerminateServiceOnCondition
{
  internal interface IResourceChecksum
  {
    string Resource { get; }

    Task<string?> GetChecksum(CancellationToken cancellationToken);

    Task<bool> Exists(CancellationToken cancellationToken);
  }
}