using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace BDO.GWT.Core.Helper;

public interface ITemporaryStorage
{
    bool Contains(Guid guid);

    bool TryGet(Guid guid, [NotNullWhen(true)] out object? value);

    Guid Set(object value);
}

public class TemporaryStorage : ITemporaryStorage
{
    private readonly IDictionary<Guid, object> _objectByGuid = new ConcurrentDictionary<Guid, object>();
    private readonly IDictionary<object, Guid> _guidByObject = new ConcurrentDictionary<object, Guid>();
    private readonly IOptions<TemporaryStorageOptions> _temporaryStorageOptions;
    private readonly ConcurrentQueue<StorageState> _storageStates = new();
    private readonly object _cleanupLock = new();

    public TemporaryStorage(IOptions<TemporaryStorageOptions> temporaryStorageOptions)
    {
        _temporaryStorageOptions = temporaryStorageOptions;
    }

    public bool Contains(Guid guid)
    {
        return _objectByGuid.ContainsKey(guid);
    }

    public bool TryGet(Guid guid, [NotNullWhen(true)] out object? value)
    {
        return _objectByGuid.TryGetValue(guid, out value);
    }

    public Guid Set(object value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (_guidByObject.TryGetValue(value, out var guid))
        {
            return guid;
        }

        guid = Guid.NewGuid();

        _objectByGuid[guid] = value;

        _guidByObject[value] = guid;

        _storageStates.Enqueue(new StorageState(guid));

        CleanupExpiredItems();

        return guid;
    }

    private void CleanupExpiredItems()
    {
        lock (_cleanupLock)
        {
            while (_storageStates.TryPeek(out var state))
            {
                var isExpired = DateTime.UtcNow - state.CreationTimestamp >= _temporaryStorageOptions.Value.ExpirationDuration;

                if (!isExpired)
                {
                    break;
                }

                if (!_storageStates.TryDequeue(out var expiredState))
                {
                    continue;
                }

                var guid = expiredState.StateGUID;

                if (_objectByGuid.TryGetValue(guid, out var obj))
                {
                    _guidByObject.Remove(obj);

                    _objectByGuid.Remove(guid);
                }
            }
        }
    }

    private class StorageState
    {
        public Guid StateGUID { get; }

        public DateTime CreationTimestamp { get; }

        public StorageState(Guid guid)
        {
            StateGUID = guid;
            CreationTimestamp = DateTime.UtcNow;
        }
    }
}
