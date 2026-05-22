
namespace BDO.GWT.GLOBAL.Tests.Components.Widget;

using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ExternalNavigationWidget;
using global::BDO.GWT.Core;
using global::BDO.GWT.GLOBAL.Components.Widget.LocalNavigationWidget;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NUnit.Framework;
using System.Threading.Tasks;

public class LocalNavigationWidgetUnitTests
{
    private LocalNavigationWidget _widget = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _widget = new LocalNavigationWidget
        {
            ViewComponentContext = new ViewComponentContext
            {
                ViewContext = new ViewContext
                {
                    HttpContext = new DefaultHttpContext(),
                },
            },
        };
    }

    [Test]
    public async Task Invoke_PropertiesArePopulated_MapsCorrectly()
    {
        var properties = new LocalNavigationWidgetProperties
        {
            LinkText = "Section 1",
            WidgetIdToLink = "section1"
        };

        var dto = await InvokeWidgetWithProperties(properties);

        Assert.Multiple(() =>
        {
            Assert.That(dto.Link, Is.Not.Null);
            Assert.That(dto.Link!.Title, Is.EqualTo(properties.LinkText));
            Assert.That(dto.Link.URL, Is.EqualTo("#section1"));
        });
    }

    [Test]
    public async Task Invoke_WidgetIdIsEmpty_URLIsHashOnly()
    {
        var properties = new LocalNavigationWidgetProperties
        {
            LinkText = "Section 2",
            WidgetIdToLink = ""
        };

        var dto = await InvokeWidgetWithProperties(properties);

        Assert.That(dto.Link.URL, Is.EqualTo("#"));
    }

    [Test]
    public async Task Invoke_LinkTextIsNull_TitleIsNull()
    {
        var properties = new LocalNavigationWidgetProperties
        {
            LinkText = string.Empty,
            WidgetIdToLink = "section3"
        };

        var dto = await InvokeWidgetWithProperties(properties);
        Assert.Multiple(() =>
        {
            Assert.That(dto.Link.Title, Is.Empty);
            Assert.That(dto.Link.URL, Is.EqualTo("#section3"));
        });
    }

    private async Task<LocalNavigationData> InvokeWidgetWithProperties(LocalNavigationWidgetProperties properties)
    {
        var model = new ComponentViewModel<LocalNavigationWidgetProperties>();
        typeof(ComponentViewModel<LocalNavigationWidgetProperties>)
            .GetProperty("Properties")?.SetValue(model, properties);


        var result = _widget.Invoke(model);

        Assert.That(result, Is.TypeOf<ViewViewComponentResult>());

        var viewResult = (ViewViewComponentResult)result;

        Assert.That(viewResult.ViewData, Is.TypeOf<ViewDataDictionary<LocalNavigationData>>());

        var viewData = (ViewDataDictionary<LocalNavigationData>)viewResult.ViewData!;
        var dto = viewData.Model;

        return await Task.FromResult(dto);
    }
}
