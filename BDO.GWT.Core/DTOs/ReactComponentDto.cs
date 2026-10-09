using BDO.GWT.Common.Options;

namespace BDO.GWT.Core.DTOs;

public class ReactComponentDto<TProps>
{
    public TProps? Props { get; set; }

    public ReactComponentOptions? Options { get; set; }
}