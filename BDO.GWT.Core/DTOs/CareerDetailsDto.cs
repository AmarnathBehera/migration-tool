
using System;
namespace BDO.GWT.Web.Core.DTOs;

public abstract class CareerDetailsDto
{
    public string Title { get; set; } = null!;

    public string? JobTitle { get; set; }

    public string? Reference { get; set; }

    public string? EmploymentType { get; set; }

    public string? Level { get; set; }

    public string[] Locations { get; set; } = Array.Empty<string>();
}
