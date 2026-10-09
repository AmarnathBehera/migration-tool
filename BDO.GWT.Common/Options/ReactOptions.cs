using System;

namespace BDO.GWT.Common.Options;

public record ReactOptions
{
    public required Uri ServerUrl { get; init; }
}
