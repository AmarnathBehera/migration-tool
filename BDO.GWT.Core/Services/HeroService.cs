using BDO.GWT.Domain.Core.Constants;
using BDO.GWT.Web.Core.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.Services;

public interface IHeroService
{
    Task<HeroDto> GetHeroDtoAsync(IHero data, CancellationToken cancellationToken = default);
}

public class HeroService : IHeroService
{
    public Task<HeroDto> GetHeroDtoAsync(IHero data, CancellationToken cancellationToken = default)
    {
        var dto = new HeroDto
        {
            Layout = data.HeroLayout,
            Title = data.HeroTitle,
            Description = data.HeroDescription,
            CtaButtonType = data.HeroCallToActionButtonType,

        };

        if (data.HeroLayout != HeroLayout.Banner)
        {
            dto.BackgroundImage = data.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset.Url;
            dto.BackgroundImageAltText = data.HeroBackgroundImageAltText;
            dto.MobileBackgroundImage = data.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset.Url;
            dto.MobileBackgroundImageAltText = data.HeroBackgroundImage_MobileAltText;
        }

        if (data.HeroCallToActionButtonType != null && data.HeroCallToActionButtonType != CallToActionButtonType.None)
        {
            dto.CtaButtonText = data.HeroCallToActionButtonText;
            dto.CtaButtonUrl = data.HeroCallToActionButtonUrl;
            dto.CtaButtonUrlTarget = data.HeroCallToActionButtonUrl;
        }

        return Task.FromResult(dto);
    }
}
