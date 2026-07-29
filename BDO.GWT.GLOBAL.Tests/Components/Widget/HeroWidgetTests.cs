using BDO.GWT.Core;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Components.Widget.HeroWidget;
using BDO.GWT.Web.Core.Data;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Tests.Widgets;

[TestFixture]
public class HeroWidgetTests
{
    private Mock<IHeroService> _heroServiceMock;
    private Mock<IContentService> _contentServiceMock;
    private HeroWidget _widget;

    [SetUp]
    public void SetUp()
    {
        _heroServiceMock = new Mock<IHeroService>();
        _contentServiceMock = new Mock<IContentService>();
        _widget = new HeroWidget(_heroServiceMock.Object, _contentServiceMock.Object);

        var httpContext = new DefaultHttpContext();
        _widget.ViewComponentContext = new ViewComponentContext
        {
            ViewContext = new Microsoft.AspNetCore.Mvc.Rendering.ViewContext
            {
                HttpContext = httpContext
            }
        };
    }

    private static ComponentViewModel<HeroWidgetProperties> BuildModel(HeroWidgetProperties properties)
    {
        var model = (ComponentViewModel<HeroWidgetProperties>)
            System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(
                typeof(ComponentViewModel<HeroWidgetProperties>));

        typeof(ComponentViewModel<HeroWidgetProperties>)
            .GetProperty(nameof(ComponentViewModel<HeroWidgetProperties>.Properties))!
            .SetValue(model, properties);

        return model;
    }

    // -------------------------------------------------------------------------
    // HeroWidget — InvokeAsync
    // -------------------------------------------------------------------------

    [Test]
    public async Task InvokeAsync_BannerLayout_DoesNotResolveImages()
    {
        // Arrange
        var props = new HeroWidgetProperties
        {
            Layout = HeroLayout.Banner,
            Title = "Test Banner",
            BackgroundImage = Enumerable.Empty<ContentItemReference>(),
            MobileBackgroundImage = Enumerable.Empty<ContentItemReference>(),
            CtaButtonType = CallToActionButtonType.None,
        };
        var model = BuildModel(props);
        _heroServiceMock
            .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto { Layout = HeroLayout.Banner });

        // Act
        await _widget.InvokeAsync(model);

        // Assert
        _contentServiceMock.Verify(
            s => s.GetItemAsync<ContentAttachment>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task InvokeAsync_WithBackgroundImage_ResolvesContentAttachment()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var attachment = new ContentAttachment
        {
            ContentAttachmentAsset = new ContentItemAsset { Url = "https://cdn.bdo.com/hero.jpg" }
        };
        var props = new HeroWidgetProperties
        {
            Layout = HeroLayout.BannerWithImage,
            BackgroundImage = new[] { new ContentItemReference { Identifier = imageId } },
            MobileBackgroundImage = Enumerable.Empty<ContentItemReference>(),
            CtaButtonType = CallToActionButtonType.None,
        };
        var model = BuildModel(props);

        _contentServiceMock
            .Setup(s => s.GetItemAsync<ContentAttachment>(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attachment);
        _heroServiceMock
            .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        // Act
        await _widget.InvokeAsync(model);

        // Assert
        _contentServiceMock.Verify(
            s => s.GetItemAsync<ContentAttachment>(imageId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task InvokeAsync_WithMobileBackgroundImage_ResolvesContentAttachment()
    {
        // Arrange
        var mobileImageId = Guid.NewGuid();
        var attachment = new ContentAttachment
        {
            ContentAttachmentAsset = new ContentItemAsset { Url = "https://cdn.bdo.com/hero-mobile.jpg" }
        };
        var props = new HeroWidgetProperties
        {
            Layout = HeroLayout.BannerWithImage,
            BackgroundImage = Enumerable.Empty<ContentItemReference>(),
            MobileBackgroundImage = new[] { new ContentItemReference { Identifier = mobileImageId } },
            CtaButtonType = CallToActionButtonType.None,
        };
        var model = BuildModel(props);

        _contentServiceMock
            .Setup(s => s.GetItemAsync<ContentAttachment>(mobileImageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attachment);
        _heroServiceMock
            .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        // Act
        await _widget.InvokeAsync(model);

        // Assert
        _contentServiceMock.Verify(
            s => s.GetItemAsync<ContentAttachment>(mobileImageId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task InvokeAsync_AttachmentHasNullUrl_PassesNullBackgroundImageToService()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var attachment = new ContentAttachment { ContentAttachmentAsset = null };
        var props = new HeroWidgetProperties
        {
            Layout = HeroLayout.Hero,
            BackgroundImage = new[] { new ContentItemReference { Identifier = imageId } },
            MobileBackgroundImage = Enumerable.Empty<ContentItemReference>(),
            CtaButtonType = CallToActionButtonType.None,
        };
        var model = BuildModel(props);

        _contentServiceMock
            .Setup(s => s.GetItemAsync<ContentAttachment>(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attachment);

        HeroData capturedData = null;
        _heroServiceMock
            .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .Callback<HeroData, CancellationToken>((d, _) => capturedData = d)
            .ReturnsAsync(new HeroDto());

        // Act
        await _widget.InvokeAsync(model);

        // Assert
        Assert.That(capturedData.BackgroundImage, Is.Null);
    }

    [Test]
    public async Task InvokeAsync_ReturnsViewResult_WithCorrectViewPath()
    {
        // Arrange
        var props = new HeroWidgetProperties
        {
            Layout = HeroLayout.Banner,
            BackgroundImage = Enumerable.Empty<ContentItemReference>(),
            MobileBackgroundImage = Enumerable.Empty<ContentItemReference>(),
            CtaButtonType = CallToActionButtonType.None,
        };
        var model = BuildModel(props);
        _heroServiceMock
            .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        // Act
        var result = await _widget.InvokeAsync(model);

        // Assert
        var viewResult = result as ViewViewComponentResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult!.ViewName, Is.EqualTo("~/Components/Widget/HeroWidget/_HeroWidget.cshtml"));
    }

    [Test]
    public async Task InvokeAsync_PassesWidgetIdFromPropertiesToService()
    {
        // Arrange
        var widgetId = "hero-widget-001";
        var props = new HeroWidgetProperties
        {
            Layout = HeroLayout.Banner,
            BackgroundImage = Enumerable.Empty<ContentItemReference>(),
            MobileBackgroundImage = Enumerable.Empty<ContentItemReference>(),
            CtaButtonType = CallToActionButtonType.None,
            WidgetID = widgetId,
        };
        var model = BuildModel(props);

        HeroData capturedData = null;
        _heroServiceMock
            .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .Callback<HeroData, CancellationToken>((d, _) => capturedData = d)
            .ReturnsAsync(new HeroDto());

        // Act
        await _widget.InvokeAsync(model);

        // Assert
        Assert.That(capturedData.WidgetID, Is.EqualTo(widgetId));
    }

    // -------------------------------------------------------------------------
    // HeroService — GetHeroDtoAsync
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetHeroDtoAsync_BannerLayout_DoesNotPopulateImageFields()
    {
        // Arrange
        var service = new HeroService();
        var data = new HeroData
        {
            Layout = HeroLayout.Banner,
            BackgroundImage = Link.Parse("https://cdn.bdo.com/hero.jpg"),
            MobileBackgroundImage = Link.Parse("https://cdn.bdo.com/hero-mobile.jpg"),
            CallToActionButtonType = CallToActionButtonType.None,
        };

        // Act
        var dto = await service.GetHeroDtoAsync(data);

        // Assert
        Assert.That(dto.BackgroundImage, Is.Null);
        Assert.That(dto.MobileBackgroundImage, Is.Null);
    }

    [Test]
    public async Task GetHeroDtoAsync_NonBannerLayout_PopulatesImageFields()
    {
        // Arrange
        var service = new HeroService();
        var data = new HeroData
        {
            Layout = HeroLayout.BannerWithImage,
            BackgroundImage = Link.Parse("https://cdn.bdo.com/hero.jpg"),
            BackgroundImageAltText = "Hero alt",
            MobileBackgroundImage = Link.Parse("https://cdn.bdo.com/hero-mobile.jpg"),
            MobileBackgroundImageAltText = "Mobile alt",
            CallToActionButtonType = CallToActionButtonType.None,
        };

        // Act
        var dto = await service.GetHeroDtoAsync(data);

        // Assert
        Assert.That(dto.BackgroundImage, Is.Not.Null);
        Assert.That(dto.BackgroundImageAltText, Is.EqualTo("Hero alt"));
        Assert.That(dto.MobileBackgroundImage, Is.Not.Null);
        Assert.That(dto.MobileBackgroundImageAltText, Is.EqualTo("Mobile alt"));
    }

    [Test]
    public async Task GetHeroDtoAsync_CtaButtonTypeNone_DoesNotPopulateCtaFields()
    {
        // Arrange
        var service = new HeroService();
        var data = new HeroData
        {
            Layout = HeroLayout.Banner,
            CallToActionButtonType = CallToActionButtonType.None,
            CallToActionButtonText = "Click me",
            CallToActionButtonUrl = Link.Parse("https://bdo.com"),
        };

        // Act
        var dto = await service.GetHeroDtoAsync(data);

        // Assert
        Assert.That(dto.CtaButtonText, Is.Null);
        Assert.That(dto.CtaButtonUrl, Is.Null);
        Assert.That(dto.CtaButtonUrlTarget, Is.Null);
    }

    [Test]
    public async Task GetHeroDtoAsync_CtaButtonTypePrimary_PopulatesCtaFields()
    {
        // Arrange
        var service = new HeroService();
        var data = new HeroData
        {
            Layout = HeroLayout.Banner,
            CallToActionButtonType = CallToActionButtonType.Primary,
            CallToActionButtonText = "Learn More",
            CallToActionButtonUrl = Link.Parse("https://bdo.com/services"),
            CtaButtonUrlTarget = "_blank",
        };

        // Act
        var dto = await service.GetHeroDtoAsync(data);

        // Assert
        Assert.That(dto.CtaButtonText, Is.EqualTo("Learn More"));
        Assert.That(dto.CtaButtonUrl, Is.Not.Null);
        Assert.That(dto.CtaButtonUrlTarget, Is.EqualTo("_blank"));
    }

    [Test]
    public async Task GetHeroDtoAsync_NullCtaButtonType_DoesNotPopulateCtaFields()
    {
        // Arrange
        var service = new HeroService();
        var data = new HeroData
        {
            Layout = HeroLayout.Banner,
            CallToActionButtonType = null,
            CallToActionButtonText = "Click me",
        };

        // Act
        var dto = await service.GetHeroDtoAsync(data);

        // Assert
        Assert.That(dto.CtaButtonText, Is.Null);
        Assert.That(dto.CtaButtonUrl, Is.Null);
    }

    [Test]
    public async Task GetHeroDtoAsync_MapsLayoutTitleDescriptionAndWidgetId()
    {
        // Arrange
        var service = new HeroService();
        var data = new HeroData
        {
            Layout = HeroLayout.Hero,
            Title = "Welcome to BDO",
            Description = "Your trusted partner",
            WidgetID = "hero-42",
            CallToActionButtonType = CallToActionButtonType.None,
        };

        // Act
        var dto = await service.GetHeroDtoAsync(data);

        // Assert
        Assert.That(dto.Layout, Is.EqualTo(HeroLayout.Hero));
        Assert.That(dto.Title, Is.EqualTo("Welcome to BDO"));
        Assert.That(dto.Description, Is.EqualTo("Your trusted partner"));
        Assert.That(dto.WidgetID, Is.EqualTo("hero-42"));
    }
}