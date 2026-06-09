using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Components.Widget.TabAccordionWidget;
using BDO.GWT.Web.Widgets.TabAccordionWidget;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget;

[TestFixture]
public class TabAccordionWidgetTests
{
    private Mock<ITabAccordionService> _serviceMock = null!;
    private TabAccordionWidget _widget = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<ITabAccordionService>();

        _widget = new TabAccordionWidget(
            _serviceMock.Object);

        _widget.ViewComponentContext =
            new ViewComponentContext
            {
                ViewContext = new ViewContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
    }

    [Test]
    public async Task InvokeAsync_ReturnsViewResult()
    {
        _serviceMock
            .Setup(x => x.GetTabAccordionAsync(
                It.IsAny<TabAccordionData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TabAccordionDto());

        var model = CreateModel();

        var result = await _widget.InvokeAsync(model);

        Assert.That(result,
            Is.InstanceOf<ViewViewComponentResult>());
    }

    [Test]
    public async Task InvokeAsync_MapsTitle()
    {
        _serviceMock
            .Setup(x => x.GetTabAccordionAsync(
                It.IsAny<TabAccordionData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TabAccordionDto());

        var model = CreateModel();

        await _widget.InvokeAsync(model);

        _serviceMock.Verify(
            x => x.GetTabAccordionAsync(
                It.Is<TabAccordionData>(
                    d => d.Title == "My Title"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task InvokeAsync_MapsLayout()
    {
        _serviceMock
            .Setup(x => x.GetTabAccordionAsync(
                It.IsAny<TabAccordionData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TabAccordionDto());

        var model = CreateModel();

        await _widget.InvokeAsync(model);

        _serviceMock.Verify(
            x => x.GetTabAccordionAsync(
                It.Is<TabAccordionData>(
                    d => d.Layout == "accordion"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task InvokeAsync_MapsWidgetId()
    {
        _serviceMock
            .Setup(x => x.GetTabAccordionAsync(
                It.IsAny<TabAccordionData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TabAccordionDto());

        var model = CreateModel();

        await _widget.InvokeAsync(model);

        _serviceMock.Verify(
            x => x.GetTabAccordionAsync(
                It.Is<TabAccordionData>(
                    d => d.WidgetID == "widget-1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task InvokeAsync_IndustryPages_AreMapped()
    {
        var guid = Guid.NewGuid();

        _serviceMock
            .Setup(x => x.GetTabAccordionAsync(
                It.IsAny<TabAccordionData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TabAccordionDto());

        var model = CreateModel();

        model.Properties.IndustryPages =
        [
            new() { Identifier = guid }
        ];

        await _widget.InvokeAsync(model);

        _serviceMock.Verify(
            x => x.GetTabAccordionAsync(
                It.Is<TabAccordionData>(
                    d => d.PageGuids.Contains(guid)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static ComponentViewModel<TabAccordionWidgetProperties> CreateModel()
    {
        var model =
            new ComponentViewModel<TabAccordionWidgetProperties>();

        typeof(ComponentViewModel<TabAccordionWidgetProperties>)
            .GetProperty("Properties")
            ?.SetValue(
                model,
                new TabAccordionWidgetProperties
                {
                    Title = "My Title",
                    Layout = "accordion",
                    WidgetID = "widget-1",
                    TopMargin = "20px",
                    BottomMargin = "40px"
                });

        return model;
    }

    [Test]
    public async Task InvokeAsync_ReturnsServiceDtoAsViewModel()
    {
        var dto = new TabAccordionDto
        {
            Title = "Returned Title"
        };

        _serviceMock
            .Setup(x => x.GetTabAccordionAsync(
                It.IsAny<TabAccordionData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var model = CreateModel();

        var result = await _widget.InvokeAsync(model);

        var viewResult = result as ViewViewComponentResult;

        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult!.ViewData!.Model, Is.SameAs(dto));
    }

    [Test]
    public async Task InvokeAsync_MapsMargins()
    {
        _serviceMock
            .Setup(x => x.GetTabAccordionAsync(
                It.IsAny<TabAccordionData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TabAccordionDto());

        var model = CreateModel();

        await _widget.InvokeAsync(model);

        _serviceMock.Verify(
            x => x.GetTabAccordionAsync(
                It.Is<TabAccordionData>(
                    d => d.TopMargin == "20px"
                      && d.BottomMargin == "40px"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}