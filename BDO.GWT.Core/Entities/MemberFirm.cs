using BDO.GWT.Core.ValueObjects;

namespace BDO.GWT.Domain.Core.Entities;

public class MemberFirm : DomainEntity
{
    public string Name { get; set; } = null!;

    public AbsoluteLink? Website { get; set; } = null!;
}
