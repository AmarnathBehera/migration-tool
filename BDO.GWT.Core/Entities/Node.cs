using System;

namespace BDO.GWT.Domain.Core.Entities;

public abstract class Node : DomainEntity, INode
{
    public int NodeID { get; set; }

    public string NodeAliasPath { get; set; } = null!;

    public int NodeLevel { get; set; }

    public int? NodeParentID { get; set; }

    public int NodeSiteID { get; set; }

    public Guid NodeGUID { get; set; }

    public int? NodeOrder { get; set; }

    public int? NodeLinkedNodeID { get; set; }
}
