using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Web.Core.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.Services;

public interface IHeroService
{
    Task<HeroDto> GetHeroDtoAsync(HeroData data, CancellationToken cancellationToken = default);
}

public class HeroService : IHeroService
{
    public Task<HeroDto> GetHeroDtoAsync(HeroData data, CancellationToken cancellationToken = default)
    {
        var dto = new HeroDto
        {
            Layout = data.Layout,
            Title = data.Title,
            Description = data.Description,
            CtaButtonType = data.CallToActionButtonType,
            WidgetID = data.WidgetID,
        };

        if (data.Layout != HeroLayout.Banner)
        {
            dto.BackgroundImage = data.BackgroundImage?.ToHtmlCompatibleString();
            dto.BackgroundImageAltText = data.BackgroundImageAltText;
            dto.MobileBackgroundImage = data.MobileBackgroundImage?.ToHtmlCompatibleString();
            dto.MobileBackgroundImageAltText = data.MobileBackgroundImageAltText;
        }

        if (data.CallToActionButtonType != null && data.CallToActionButtonType != CallToActionButtonType.None)
        {
            dto.CtaButtonText = data.CallToActionButtonText;
            dto.CtaButtonUrl = data.CallToActionButtonUrl?.ToHtmlCompatibleString();
            dto.CtaButtonUrlTarget = data.CtaButtonUrlTarget;
        }

        return Task.FromResult(dto);
    }
}
