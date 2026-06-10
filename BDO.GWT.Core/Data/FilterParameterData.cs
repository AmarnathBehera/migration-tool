using System;

namespace BDO.GWT.Core.Data;

public enum FilterParameterType
{
    Page,
    Entity,
    Field,
    Tagpage
}

public record FilterParameterData(string AdminDisplayName, string LiveSiteDisplayName, Type EntityType, FilterParameterType FilterParameterType);
