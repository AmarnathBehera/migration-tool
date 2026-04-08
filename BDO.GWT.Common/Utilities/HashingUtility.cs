using Blake3;
using System;
using System.Text;
using System.Text.Json;

namespace BDO.GWT.Common.Utilities;

public static class HashingUtility
{
    public static string GetBase64EncodedHash(object obj)
    {
        var json = JsonSerializer.Serialize(obj);

        var bytes = Encoding.UTF8.GetBytes(json);

        var hash = Hasher.Hash(bytes);

        var hashBytes = hash.AsSpan();

        var encodedHash = Convert.ToBase64String(hashBytes);

        return encodedHash;
    }
}
