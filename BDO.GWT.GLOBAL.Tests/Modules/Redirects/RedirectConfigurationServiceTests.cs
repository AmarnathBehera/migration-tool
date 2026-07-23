using CMS.DataEngine;
using XperienceCommunity.Redirects.Services;

namespace BDO.GWT.GLOBAL.Tests.Modules.Redirects;

public class RedirectConfigurationServiceTests
{
    [Test]
    public void Service_ImplementsIRedirectService()
    {
        Assert.That(typeof(IRedirectService).IsAssignableFrom(typeof(RedirectService)), Is.True);
    }

    [Test]
    public void Service_ExposesExpectedConstructor()
    {
        var ctor = typeof(RedirectService).GetConstructors().Single();

        Assert.That(ctor.GetParameters(), Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(ctor.GetParameters()[0].ParameterType, Is.EqualTo(typeof(IInfoProvider<XperienceCommunity.Redirects.RedirectInfo>)));
            Assert.That(ctor.GetParameters()[1].ParameterType, Is.EqualTo(typeof(CMS.Helpers.IProgressiveCache)));
        });
    }

    [Test]
    public void GetRedirects_ReturnType_IsTaskOfReadOnlyCollection()
    {
        var method = typeof(RedirectService).GetMethod(nameof(RedirectService.GetRedirects));

        Assert.That(method, Is.Not.Null);
        Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task<IReadOnlyCollection<XperienceCommunity.Redirects.RedirectInfo>>)));
    }
}
