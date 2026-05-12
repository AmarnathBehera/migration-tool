using CMS.Core;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Forms.Web.Mvc;
using Microsoft.AspNetCore.Http;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;

public class SecurityCode : RecaptchaComponent
{

    public SecurityCode(IAppSettingsService appSettingsService, IHttpContextAccessor httpContextAccessor, ILocalizationService localizationService, IWebsiteChannelContext websiteChannelContext, IInfoProvider<WebsiteCaptchaSettingsInfo> captchaSettingInfoProvider) : base(appSettingsService, httpContextAccessor, localizationService, websiteChannelContext, captchaSettingInfoProvider)
    {
    }
}