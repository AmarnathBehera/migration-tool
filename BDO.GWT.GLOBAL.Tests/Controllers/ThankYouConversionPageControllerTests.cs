using BDO.GWT.Web.Core.LiveSite.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

public class ThankYouConversionPageControllerTests
{
    private ThankYouConversionPageController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _controller = new ThankYouConversionPageController();
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    // --- Index -------------------------------------------------------------------

    [Test]
    public void Index_ReturnsViewResult()
    {
        var result = _controller.Index();

        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public void Index_ReturnsViewResult_WithNoViewModel()
    {
        var result = (ViewResult)_controller.Index();

        Assert.That(result.Model, Is.Null);
    }

    [Test]
    public void Index_ReturnsViewResult_WithNoExplicitViewName()
    {
        var result = (ViewResult)_controller.Index();

        Assert.That(result.ViewName, Is.Null.Or.Empty);
    }
}
