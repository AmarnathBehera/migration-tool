namespace BDO.GWT.GLOBAL.API.Data;

public class LicenseKeyOutput
{
    public required int ID { get; set; }

    /// <summary>The raw stored license key value (base64 envelope).</summary>
    public string? Key { get; set; }
}

public class LicenseInfo
{
    public string? Id { get; set; }

    /// <summary>Licensee/subscription owner, e.g. company name.</summary>
    public string? Owner { get; set; }

    /// <summary>Underlying subscription end date (not the auto-refreshing key expiry).</summary>
    public DateTime? ExpirationDateUtc { get; set; }
}

public class LimitationsInfo
{
    public int Channels { get; set; }

    public int MicroChannels { get; set; }
}

public class KeyInfo
{
    public string? Id { get; set; }

    public string? Version { get; set; }

    public DateTime? IssuedDateUtc { get; set; }

    /// <summary>Operational expiry of this specific generated key; auto-refreshes with subscription sync.</summary>
    public DateTime? ExpirationDateUtc { get; set; }

    /// <summary>e.g. "Development", "Production".</summary>
    public string? Type { get; set; }

    /// <summary>Friendly name assigned when the key was generated.</summary>
    public string? Name { get; set; }

    public bool Synchronization { get; set; }
}

public sealed class LicenseKeyEnvelope
{
    public string? Data { get; set; }
    public string? Signature { get; set; }
}

public sealed class LicenseKeyPayload
{
    public string Tier { get; set; } = default!;
    public LicenseInfo License { get; set; } = default!;
    public LimitationsInfo Limitations { get; set; } = default!;
    public KeyInfo Key { get; set; } = default!;
}