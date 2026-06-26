using System;

namespace BDO.GWT.Domain.Core.Entities;

public class UserApiPermission : DomainEntity, ISiteEntity
{
    public int UserID { get; set; }

    public int SiteID { get; set; }

    public int ApiMethodGroupID { get; set; }

    public bool AllowRead { get; set; }

    public bool AllowManage { get; set; }

    public Guid GUID { get; set; }

    public DateTime LastModified { get; set; }
}
