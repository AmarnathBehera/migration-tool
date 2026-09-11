using System;

namespace BDO.GWT.Domain.Core.Entities;

public class UserApiPermission : DomainEntity, ISiteEntity
{
    /// <summary>
    /// Gets or sets the ID of the user associated with this permission. This property is used to identify which user has the specified API permissions.
    /// </summary>
    public int UserID { get; set; }

    public int SiteID { get; set; }

    /// <summary>
    /// Gets or sets the ID of the API method group associated with this permission. This property is used to identify which group of API methods the user has permissions for.
    /// </summary>
    public int ApiMethodGroupID { get; set; }

    public bool AllowRead { get; set; }

    public bool AllowManage { get; set; }

    public Guid GUID { get; set; }

    public DateTime LastModified { get; set; }
}
