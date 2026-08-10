using BDO.GWT.Web.Core.LiveSite.Enums;
using BDO.GWT.Web.Core.LiveSite.Services;
using CMS.Helpers;
using Microsoft.AspNetCore.Http;

#pragma warning disable CS0618 // CookieHelper is obsolete; retained intentionally until Kentico cookie accessor migration.

namespace BDO.GWT.Xperience.Core.LiveSite.Services;
public interface ICookieConsentService
{
    void SetVisitorCookieConsentLevel(CookieConsentLevel cookieConsentLevel);

    Task<CookieConsentLevel> GetDefaultCookieConsentLevelAsync(CancellationToken cancellationToken);

    void RemoveNotAcceptedCookies();
}
public class CookieConsentService : ICookieConsentService
{
    // all non registered cookies will be treated as marketing cookies (200)
    private static readonly (string CookieName, CookieConsentLevel cookieConsentLevel)[] CookiesToRegister =
    {
        ("cookie-accepted", CookieConsentLevel.Essential),
    };

    private readonly ICurrentCookieLevelProvider _currentCookieLevelProvider;
    private readonly IVisitorContext _visitorContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieConsentService(ICurrentCookieLevelProvider currentCookieLevelProvider, IVisitorContext visitorContext, IHttpContextAccessor httpContextAccessor)
    {
        _currentCookieLevelProvider = currentCookieLevelProvider;
        _visitorContext = visitorContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetVisitorCookieConsentLevel(CookieConsentLevel cookieConsentLevel)
    {
        _currentCookieLevelProvider.SetCurrentCookieLevel((int)cookieConsentLevel);
    }

    public Task<CookieConsentLevel> GetDefaultCookieConsentLevelAsync(CancellationToken cancellationToken)
    {
        var level = _currentCookieLevelProvider.GetDefaultCookieLevel();
        return Task.FromResult((CookieConsentLevel)level);
    }

    public void RemoveNotAcceptedCookies()
    {
        var cookieNames = _httpContextAccessor.HttpContext?.Request.Cookies.Keys ?? [];

        var antiforgeryCookieName = cookieNames.FirstOrDefault(x => x.StartsWith(".AspNetCore.Antiforgery."));

        if (antiforgeryCookieName is not null)
        {
            var allowed = CookieHelper.IsCookieAllowed(antiforgeryCookieName);

            if (!allowed)
            {
                CookieHelper.RegisterCookie(antiforgeryCookieName, (int)CookieConsentLevel.Essential, true);
            }
        }

        CookieHelper.RemoveAllCookies((int)_visitorContext.CookieConsentLevel);
    }

    public static void RegisterCookies()
    {
        foreach (var (cookieName, cookieConsentLevel) in CookiesToRegister)
        {
            CookieHelper.RegisterCookie(cookieName, (int)cookieConsentLevel);
        }
    }
}

#pragma warning restore CS0618
