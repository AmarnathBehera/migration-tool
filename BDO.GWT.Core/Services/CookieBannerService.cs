using BDO.GWT.Web.Core.DTOs;
using CMS.Helpers;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Services;

public interface ICookieBannerService
{
    Task<CookieBannerDto> GetAsync(CancellationToken cancellationToken);
}

public class CookieBannerService : ICookieBannerService
{
    private readonly ICurrentCookieLevelProvider _cookieLevelService;


    public CookieBannerService(ICurrentCookieLevelProvider cookieLevelService)
    {
        _cookieLevelService = cookieLevelService;
    }

    public async Task<CookieBannerDto> GetAsync(CancellationToken cancellationToken)
    {
        var defaultCookieConsentLevel =  _cookieLevelService.GetDefaultCookieLevel();

        return new CookieBannerDto
        {
            DefaultCookieValue = ((int)defaultCookieConsentLevel).ToString(),
        };
    }
}
