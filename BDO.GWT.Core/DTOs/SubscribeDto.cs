using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.DTOs.Pages;

namespace BDO.GWT.Core.DTOs.Pages;

public class SubscribeDto
{
    public HeroDto Hero { get; set; } = null!;

    public string? FormType { get; set; }

    public string? EmbedCode { get; set; }

    public SubscribeFormDto? SubscribeFormDto { get; set; }
}
