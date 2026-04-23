using BDO.GWT.Core.ValueObjects;

namespace BDO.GWT.Domain.Core.Entities;

public interface IHeroPage
{
    string HeroLayout { get; set; }

    string? HeroTitle { get; set; }

    string? HeroDescription { get; set; }

    Link? HeroBackgroundImage { get; set; }

    string? HeroBackgroundImageAltText { get; set; }

    Link? HeroBackgroundImage_Mobile { get; set; }

    string? HeroBackgroundImage_MobileAltText { get; set; }

    string HeroCallToActionButtonType { get; set; }

    string? HeroCallToActionButtonText { get; set; }

    Link? HeroCallToActionButtonUrl { get; set; }
}
