namespace CoMind.Domain.Common;

public interface ISyncable
{
    SyncVersion SyncVersion { get; }
}
