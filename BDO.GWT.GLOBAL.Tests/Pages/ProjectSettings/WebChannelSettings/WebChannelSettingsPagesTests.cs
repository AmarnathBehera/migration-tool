using System.Reflection;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;

namespace BDO.GWT.GLOBAL.Tests.Pages.ProjectSettings.WebChannelSettings
{
    public class SeoSettingsEditPageTests
    {
        [Test]
        public void Page_InheritsFromInfoEditPage()
        {
            var baseType = typeof(SeoSettingsEditPage).BaseType;
            Assert.That(baseType, Is.Not.Null);
            Assert.That(baseType!.IsGenericType, Is.True);
            Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(SEOSettingsInfo)));
        }

        [Test]
        public void Page_ExposesExpectedConstructor()
        {
            var ctor = typeof(SeoSettingsEditPage).GetConstructor(new[]
            {
                typeof(IFormComponentMapper),
                typeof(IFormDataBinder),
                typeof(IInfoProvider<SEOSettingsInfo>),
                typeof(IInfoProvider<WebChannelSettingsInfo>),
                typeof(IChannelAccessService),
            });

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void WebChannelSettingsId_HasPageParameterAttribute()
        {
            var prop = typeof(SeoSettingsEditPage).GetProperty(nameof(SeoSettingsEditPage.WebChannelSettingsId));
            Assert.That(prop, Is.Not.Null);
            Assert.That(prop!.GetCustomAttribute<PageParameterAttribute>(), Is.Not.Null);
        }

        [Test]
        public void ObjectId_HasPageParameterAttribute()
        {
            var prop = typeof(SeoSettingsEditPage).GetProperty(nameof(SeoSettingsEditPage.ObjectId));
            Assert.That(prop, Is.Not.Null);
            Assert.That(prop!.GetCustomAttribute<PageParameterAttribute>(), Is.Not.Null);
        }

        [Test]
        public void OverridesConfigurePage()
        {
            var method = typeof(SeoSettingsEditPage).GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(SeoSettingsEditPage)));
        }

        [Test]
        public void OverridesGetSubmitSuccessResponse()
        {
            var method = typeof(SeoSettingsEditPage).GetMethod(
                "GetSubmitSuccessResponse",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(SeoSettingsEditPage)));
        }
    }

    public class WebChannelSettingsEditSectionTests
    {
        [Test]
        public void Section_InheritsFromEditSectionPage()
        {
            var baseType = typeof(WebChannelSettingsEditSection).BaseType;
            Assert.That(baseType, Is.Not.Null);
            Assert.That(baseType!.IsGenericType, Is.True);
            Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(WebChannelSettingsInfo)));
        }
    }

    public class WebChannelSettingsListingPageTests
    {
        [Test]
        public void Page_InheritsFromListingPage()
        {
            Assert.That(typeof(ListingPage).IsAssignableFrom(typeof(WebChannelSettingsListingPage)), Is.True);
        }

        [Test]
        public void Page_ExposesExpectedConstructor()
        {
            var ctor = typeof(WebChannelSettingsListingPage).GetConstructor(new[]
            {
                typeof(IInfoProvider<ChannelInfo>),
                typeof(IInfoProvider<WebChannelSettingsInfo>),
                typeof(IInfoProvider<SEOSettingsInfo>),
                typeof(IChannelAccessService),
            });

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void OverridesConfigurePage()
        {
            var method = typeof(WebChannelSettingsListingPage).GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(WebChannelSettingsListingPage)));
        }
    }

    public class WebChannelSnippetCreatePageTests
    {
        [Test]
        public void Page_InheritsFromCreatePage()
        {
            var baseType = typeof(WebChannelSnippetCreatePage).BaseType;
            Assert.That(baseType, Is.Not.Null);
            Assert.That(baseType!.IsGenericType, Is.True);
            Assert.Multiple(() =>
            {
                Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(WebChannelSnippetInfo)));
                Assert.That(baseType.GetGenericArguments()[1], Is.EqualTo(typeof(WebChannelSnippetEditSection)));
            });
        }

        [Test]
        public void Page_ExposesExpectedConstructor()
        {
            var ctor = typeof(WebChannelSnippetCreatePage).GetConstructor(new[]
            {
                typeof(IFormComponentMapper),
                typeof(IFormDataBinder),
                typeof(IPageLinkGenerator),
            });

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void WebChannelSettingsId_HasPageParameterAttribute()
        {
            var prop = typeof(WebChannelSnippetCreatePage).GetProperty(
                nameof(WebChannelSnippetCreatePage.WebChannelSettingsId));

            Assert.That(prop, Is.Not.Null);
            Assert.That(prop!.GetCustomAttribute<PageParameterAttribute>(), Is.Not.Null);
        }

        [Test]
        public void OverridesConfigurePage()
        {
            var method = typeof(WebChannelSnippetCreatePage).GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(WebChannelSnippetCreatePage)));
        }

        [Test]
        public void OverridesFinalizeInfoObject()
        {
            var method = typeof(WebChannelSnippetCreatePage).GetMethod(
                "FinalizeInfoObject",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(WebChannelSnippetCreatePage)));
        }
    }

    public class WebChannelSnippetEditPageTests
    {
        [Test]
        public void Page_InheritsFromInfoEditPage()
        {
            var baseType = typeof(WebChannelSnippetEditPage).BaseType;
            Assert.That(baseType, Is.Not.Null);
            Assert.That(baseType!.IsGenericType, Is.True);
            Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(WebChannelSnippetInfo)));
        }

        [Test]
        public void Page_ExposesExpectedConstructor()
        {
            var ctor = typeof(WebChannelSnippetEditPage).GetConstructor(new[]
            {
                typeof(IFormComponentMapper),
                typeof(IFormDataBinder),
            });

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void ObjectId_HasPageParameterAttribute()
        {
            var prop = typeof(WebChannelSnippetEditPage).GetProperty(nameof(WebChannelSnippetEditPage.ObjectId));
            Assert.That(prop, Is.Not.Null);
            Assert.That(prop!.GetCustomAttribute<PageParameterAttribute>(), Is.Not.Null);
        }

        [Test]
        public void OverridesConfigurePage()
        {
            var method = typeof(WebChannelSnippetEditPage).GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(WebChannelSnippetEditPage)));
        }
    }

    public class WebChannelSnippetEditSectionTests
    {
        [Test]
        public void Section_InheritsFromEditSectionPage()
        {
            var baseType = typeof(WebChannelSnippetEditSection).BaseType;
            Assert.That(baseType, Is.Not.Null);
            Assert.That(baseType!.IsGenericType, Is.True);
            Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(WebChannelSnippetInfo)));
        }
    }

    public class WebChannelSnippetListingPageTests
    {
        [Test]
        public void Page_InheritsFromListingPage()
        {
            Assert.That(typeof(ListingPage).IsAssignableFrom(typeof(WebChannelSnippetListingPage)), Is.True);
        }

        [Test]
        public void WebChannelSettingsId_HasPageParameterAttribute()
        {
            var prop = typeof(WebChannelSnippetListingPage).GetProperty(
                nameof(WebChannelSnippetListingPage.WebChannelSettingsId));

            Assert.That(prop, Is.Not.Null);
            Assert.That(prop!.GetCustomAttribute<PageParameterAttribute>(), Is.Not.Null);
        }

        [Test]
        public void OverridesConfigurePage()
        {
            var method = typeof(WebChannelSnippetListingPage).GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(WebChannelSnippetListingPage)));
        }

        [Test]
        public void OverridesDelete_WithPageCommandAttribute()
        {
            var method = typeof(WebChannelSnippetListingPage).GetMethod(
                "Delete",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(WebChannelSnippetListingPage)));
            Assert.That(method.GetCustomAttribute<PageCommandAttribute>(), Is.Not.Null);
        }
    }
}
