using BDO.GWT.Web.Core.LiveSite.Enums;
using BDO.GWT.Web.Core.LiveSite.Services;
using CMS.Helpers;

namespace BDO.GWT.Xperience.Core.LiveSite.Services;
public interface IVisitorContext
{
    CookieConsentLevel CookieConsentLevel { get; }
}
public class VisitorContext : IVisitorContext
{
    private readonly ICurrentCookieLevelProvider _currentCookieLevelProvider;

    public VisitorContext(ICurrentCookieLevelProvider currentCookieLevelProvider)
    {
        _currentCookieLevelProvider = currentCookieLevelProvider;
    }

    public CookieConsentLevel CookieConsentLevel => (CookieConsentLevel)_currentCookieLevelProvider.GetCurrentCookieLevel();
}
