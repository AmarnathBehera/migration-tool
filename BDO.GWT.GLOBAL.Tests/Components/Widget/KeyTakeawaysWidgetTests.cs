using BDO.GWT.Core.DTOs;
using BDO.GWT.GLOBAL.Components.Widget.KeyTakeawaysWidget;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NUnit.Framework;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget;

public class KeyTakeawaysWidgetTests
{
    private KeyTakeawaysWidget _widget = null!;

    [SetUp]
    public void Setup()
    {
        _widget = new KeyTakeawaysWidget();

        _widget.ViewComponentContext = new ViewComponentContext
        {
            ViewContext = new ViewContext
            {
                HttpContext = new DefaultHttpContext(),
                ViewData = new ViewDataDictionary(
                    new EmptyModelMetadataProvider(),
                    new ModelStateDictionary())
            }
        };
    }

    private static ComponentViewModel<KeyTakeawaysWidgetProperties> GetModel(
        KeyTakeawaysWidgetProperties props)
    {
        var model = new ComponentViewModel<KeyTakeawaysWidgetProperties>();

        typeof(ComponentViewModel<KeyTakeawaysWidgetProperties>)
            .GetProperty("Properties")
            ?.SetValue(model, props);

        return model;
    }

    [Test]
    public async Task InvokeAsync_ReturnsViewResult()
    {
        var props = new KeyTakeawaysWidgetProperties
        {
            Title = "Test Title",
            Content = "Test Content"
        };

        var model = GetModel(props);

        var result = await _widget.InvokeAsync(model);

        Assert.That(result, Is.TypeOf<ViewViewComponentResult>());
    }

    [Test]
    public async Task InvokeAsync_MapsTitleCorrectly()
    {
        var props = new KeyTakeawaysWidgetProperties
        {
            Title = "Key Takeaway Title"
        };

        var model = GetModel(props);

        var result = await _widget.InvokeAsync(model);

        var viewModel =
            (result as ViewViewComponentResult)?.ViewData?.Model
            as KeyTakeawayDto;

        Assert.That(viewModel, Is.Not.Null);
        Assert.That(viewModel!.Title, Is.EqualTo("Key Takeaway Title"));
    }

    [Test]
    public async Task InvokeAsync_MapsContentCorrectly()
    {
        var props = new KeyTakeawaysWidgetProperties
        {
            Content = "<p>Rich text content</p>"
        };

        var model = GetModel(props);

        var result = await _widget.InvokeAsync(model);

        var viewModel =
            (result as ViewViewComponentResult)?.ViewData?.Model
            as KeyTakeawayDto;

        Assert.That(viewModel, Is.Not.Null);
        Assert.That(viewModel!.Content, Is.EqualTo("<p>Rich text content</p>"));
    }

    [Test]
    public async Task InvokeAsync_MapsBackgroundColourCorrectly()
    {
        var props = new KeyTakeawaysWidgetProperties
        {
            BackgroundColour = "slate"
        };

        var model = GetModel(props);

        var result = await _widget.InvokeAsync(model);

        var viewModel =
            (result as ViewViewComponentResult)?.ViewData?.Model
            as KeyTakeawayDto;

        Assert.That(viewModel, Is.Not.Null);
        Assert.That(viewModel!.BackgroundColour, Is.EqualTo("slate"));
    }

    [Test]
    public async Task InvokeAsync_MapsContentAlignmentCorrectly()
    {
        var props = new KeyTakeawaysWidgetProperties
        {
            ContentAlignment = "center"
        };

        var model = GetModel(props);

        var result = await _widget.InvokeAsync(model);

        var viewModel =
            (result as ViewViewComponentResult)?.ViewData?.Model
            as KeyTakeawayDto;

        Assert.That(viewModel, Is.Not.Null);
        Assert.That(viewModel!.ContentAlignment, Is.EqualTo("center"));
    }

    [Test]
    public async Task InvokeAsync_MapsContentWidthCorrectly()
    {
        var props = new KeyTakeawaysWidgetProperties
        {
            ContentWidth = "section-container--xl"
        };

        var model = GetModel(props);

        var result = await _widget.InvokeAsync(model);

        var viewModel =
            (result as ViewViewComponentResult)?.ViewData?.Model
            as KeyTakeawayDto;

        Assert.That(viewModel, Is.Not.Null);
        Assert.That(viewModel!.ContentWidth, Is.EqualTo("section-container--xl"));
    }

    [Test]
    public async Task InvokeAsync_MapsTopMarginCorrectly()
    {
        var props = new KeyTakeawaysWidgetProperties
        {
            TopMargin = "40px"
        };

        var model = GetModel(props);

        var result = await _widget.InvokeAsync(model);

        var viewModel =
            (result as ViewViewComponentResult)?.ViewData?.Model
            as KeyTakeawayDto;

        Assert.That(viewModel, Is.Not.Null);
        Assert.That(viewModel!.TopMargin, Is.EqualTo("40px"));
    }

    [Test]
    public async Task InvokeAsync_MapsBottomMarginCorrectly()
    {
        var props = new KeyTakeawaysWidgetProperties
        {
            BottomMargin = "60px"
        };

        var model = GetModel(props);

        var result = await _widget.InvokeAsync(model);

        var viewModel =
            (result as ViewViewComponentResult)?.ViewData?.Model
            as KeyTakeawayDto;

        Assert.That(viewModel, Is.Not.Null);
        Assert.That(viewModel!.BottomMargin, Is.EqualTo("60px"));
    }

    [Test]
    public async Task InvokeAsync_MapsWidgetIdCorrectly()
    {
        var props = new KeyTakeawaysWidgetProperties
        {
            WidgetID = "key-takeaway-widget"
        };

        var model = GetModel(props);

        var result = await _widget.InvokeAsync(model);

        var viewModel =
            (result as ViewViewComponentResult)?.ViewData?.Model
            as KeyTakeawayDto;

        Assert.That(viewModel, Is.Not.Null);
        Assert.That(viewModel!.WidgetID, Is.EqualTo("key-takeaway-widget"));
    }

    [Test]
    public async Task InvokeAsync_DefaultValues_AreMappedCorrectly()
    {
        var props = new KeyTakeawaysWidgetProperties
        {
            Title = "Default Test",
            Content = "Default Content"
        };

        var model = GetModel(props);

        var result = await _widget.InvokeAsync(model);

        var viewModel =
            (result as ViewViewComponentResult)?.ViewData?.Model
            as KeyTakeawayDto;

        Assert.That(viewModel, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel!.ContentAlignment, Is.EqualTo("left"));
            Assert.That(viewModel!.ContentWidth, Is.EqualTo("section-container"));
            Assert.That(viewModel!.TopMargin, Is.EqualTo("0px"));
            Assert.That(viewModel!.BottomMargin, Is.EqualTo("0px"));
        });
    }

    [Test]
    public async Task InvokeAsync_MapsAllPropertiesCorrectly()
    {
        var props = new KeyTakeawaysWidgetProperties
        {
            Title = "Complete Title",
            Content = "<p>Complete Content</p>",
            BackgroundColour = "pale-charcoal",
            ContentAlignment = "right",
            ContentWidth = "section-container--xxl",
            TopMargin = "20px",
            BottomMargin = "80px",
            WidgetID = "complete-widget"
        };

        var model = GetModel(props);

        var result = await _widget.InvokeAsync(model);

        var viewModel =
            (result as ViewViewComponentResult)?.ViewData?.Model
            as KeyTakeawayDto;

        Assert.That(viewModel, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel!.Title, Is.EqualTo("Complete Title"));
            Assert.That(viewModel!.Content, Is.EqualTo("<p>Complete Content</p>"));
            Assert.That(viewModel!.BackgroundColour, Is.EqualTo("pale-charcoal"));
            Assert.That(viewModel!.ContentAlignment, Is.EqualTo("right"));
            Assert.That(viewModel!.ContentWidth, Is.EqualTo("section-container--xxl"));
            Assert.That(viewModel!.TopMargin, Is.EqualTo("20px"));
            Assert.That(viewModel!.BottomMargin, Is.EqualTo("80px"));
            Assert.That(viewModel!.WidgetID, Is.EqualTo("complete-widget"));
        });
    }
}