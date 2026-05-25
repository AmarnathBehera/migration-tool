using BDO.GWT.Core;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.DTOs.Pages;
using System.Collections.Generic;

namespace BDO.GWT.Web.Core.LiveSite.DTOs.Pages;

public class EventWebinarDto
{
    public MetaDataDto? MetaData { get; set; }
    public SearchAndSEODto? SearchAndSEO { get; set; }

    public OGMetaDataDto? OGMetaData { get; set; }

    public BaseContent? BaseContent { get; set; }

    public BaseWebPage? BaseWebPage { get; set; }
    public HeroDto Hero { get; set; } = null!;

    public string? Title { get; set; }

    public string? Description { get; set; }

    public bool? CpeEvent { get; set; }

    public string? CpeCount { get; set; }

    public string? CpeCreditNotes { get; set; }

    public string? CpeDescription { get; set; }

    public string? CpeLevel { get; set; }

    public EventWebinarDetailsRegistrationCardDto RegistrationCard { get; set; } = null!;

    public List<PresenterDto>? Presenters { get; set; }

    public bool? DisablePrintAndShare { get; set; }

    public SocialMediaButtonsDto SocialMediaButtons { get; set; } = null!;
}
