using CMS.Core;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Forms.Web.Mvc;
using Microsoft.AspNetCore.Http;

namespace BDO.GWT.GLOBAL.Tests.Components.FormComponents.SecurityCode
{
    public class SecurityCodeTests
    {
        

        private static Type ComponentType =>
            typeof(BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents.SecurityCode);

        [Test]
        public void SecurityCode_InheritsFrom_RecaptchaComponent()
        {
            Assert.That(typeof(RecaptchaComponent).IsAssignableFrom(ComponentType), Is.True);
        }

        [Test]
        public void SecurityCode_ExposesExpectedConstructor()
        {
            var ctor = ComponentType.GetConstructor(new[]
            {
                typeof(IAppSettingsService),
                typeof(IHttpContextAccessor),
                typeof(ILocalizationService),
                typeof(IWebsiteChannelContext),
                typeof(IInfoProvider<WebsiteCaptchaSettingsInfo>),
            });

            Assert.That(ctor, Is.Not.Null,
                "SecurityCode must expose a constructor accepting Kentico's reCAPTCHA dependencies.");
        }
    }
}