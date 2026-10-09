using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs.Pages;
public class CareerContentDto 
{
    public string? Content { get; set; }

    public string? ContactPerson { get; set; }

    public string Title { get; set; } = null!;

    public string? JobTitle { get; set; }

    public string? Reference { get; set; }

    public string? EmploymentType { get; set; }

    public string? Level { get; set; }

    public string[] Locations { get; set; } = Array.Empty<string>();
}
