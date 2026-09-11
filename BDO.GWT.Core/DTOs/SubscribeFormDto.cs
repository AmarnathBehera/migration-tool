using BDO.GWT.Core.DTOs;
using System;

namespace BDO.GWT.Web.Core.DTOs.Pages;

public class SubscribeFormDto
{
    public bool IsV3 { get; set; }

    public string SiteKey { get; set; } = null!;

    public string RedirectUrl { get; set; } = null!;

    public string EndpointRoute { get; set; } = null!;

    public OptionDto[] Newsletters { get; set; } = Array.Empty<OptionDto>();

    public OptionDto[] Countries { get; set; } = Array.Empty<OptionDto>();

    public bool ShowCompanyField { get; set; }

    public bool IsCompanyRequired { get; set; }

    public bool ShowJobTitleField { get; set; }

    public bool IsJobTitleRequired { get; set; }

    public bool ShowCountryField { get; set; }

    public bool IsCountryRequired { get; set; }

    public bool AddConsentToForm { get; set; }
}
