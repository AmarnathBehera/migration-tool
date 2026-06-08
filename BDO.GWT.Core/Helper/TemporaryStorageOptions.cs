using System;

namespace BDO.GWT.Core.Helper;

public class TemporaryStorageOptions
{
    /// <summary>
    /// TimeSpan for temporary storage expiry.
    /// </summary>
    public TimeSpan ExpirationDuration { get; set; } = TimeSpan.FromHours(1);
}
