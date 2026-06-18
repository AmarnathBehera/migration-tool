using System.Collections.Generic;

namespace BDO.GWT.Core.DTOs.Pages;

public class MicrositeMasterPageDto
{
    public string? HeadHtml { get; set; }

    public string? BodyTopHtml { get; set; }

    public string? BodyBottomHtml { get; set; }

    public IEnumerable<string> CustomJsLinks { get; set; } = new List<string>();

    public IEnumerable<string> CustomCssLinks { get; set; } = new List<string>();
}
