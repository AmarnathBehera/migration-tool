using System.Reflection;
using CMS.Websites.Routing;
using XperienceCommunity.Redirects.UIPages;

namespace BDO.GWT.GLOBAL.Tests.Modules.Redirects;

public class RedirectEditModelTests
{
    [Test]
    public void Model_Defaults_TargetWebPageItemToEmptyCollection()
    {
        var model = new RedirectEditModel();

        Assert.Multiple(() =>
        {
            Assert.That(model.SourceUrl, Is.Null);
            Assert.That(model.TargetWebPageItem, Is.Empty);
        });
    }

    [Test]
    public void MapToRedirectInfo_Exists_WithExpectedParameters()
    {
        var method = typeof(RedirectEditModel).GetMethod(nameof(RedirectEditModel.MapToRedirectInfo));

        Assert.That(method, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task)));
            Assert.That(method.GetParameters(), Has.Length.EqualTo(2));
            Assert.That(method.GetParameters()[0].ParameterType, Is.EqualTo(typeof(XperienceCommunity.Redirects.RedirectInfo)));
            Assert.That(method.GetParameters()[1].ParameterType, Is.EqualTo(typeof(CMS.Websites.IWebPageUrlRetriever)));
        });
    }
}
