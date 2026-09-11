using System;

namespace BDO.GWT.Web.Core.DTOs.Pages;

public class InsightSourceDto
{
    public string? FirmName { get; set; }

    public string? Link { get; set; }

    public DateTime? CopyDate { get; set; } = null!;
}
