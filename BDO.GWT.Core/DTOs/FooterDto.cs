
using System.Collections.Generic;

namespace BDO.GWT.Core.DTOs;

public class FooterDto
{
    public bool DisplaySubscriptionsSection { get; set; }

    public string? SubscriptionButtonLink { get; set; }

    public List<SocialButtonDto> SocialButtons { get; set; }

    public List<FooterLinkDto> FooterLinks { get; set; }
}

