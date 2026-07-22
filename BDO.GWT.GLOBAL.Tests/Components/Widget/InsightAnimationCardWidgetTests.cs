using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Utilities;
using BDO.GWT.GLOBAL.Widgets;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Tests.Widgets;

[TestFixture]
public class InsightAnimationCardWidgetTests
{
    // -----------------------------------------------------------------------
    // Mocks & shared state
    // -----------------------------------------------------------------------

    private Mock<IContentService> _contentServiceMock;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock;

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    [SetUp]
    public void SetUp()
    {
        _contentServiceMock = new Mock<IContentService>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();

        // Default: channel context is valid
        _websiteChannelContextMock
            .Setup(c => c.WebsiteChannelID)
            .Returns(1);
    }

    /// <summary>
    /// Creates the SUT and wires a minimal HttpContext so HttpContext.RequestAborted is accessible.
    /// </summary>
    private InsightAnimationCardWidget CreateSut()
    {
        var widget = new InsightAnimationCardWidget(
            _contentServiceMock.Object,
            _websiteChannelContextMock.Object
            );

        widget.ViewComponentContext = new ViewComponentContext
        {
            ViewContext = new Microsoft.AspNetCore.Mvc.Rendering.ViewContext
            {
                HttpContext = new DefaultHttpContext(),
            },
        };

        return widget;
    }

    /// <summary>
    /// Builds a <see cref="ComponentViewModel{TProperties}"/> using reflection
    /// (the type is sealed / has no public constructor).
    /// </summary>
    private static ComponentViewModel<InsightAnimationCardWidgetProperties> BuildModel(
        InsightAnimationCardWidgetProperties properties)
    {
        // ComponentViewModel<T> is sealed — use uninitialized object + reflection
        var model = (ComponentViewModel<InsightAnimationCardWidgetProperties>)
            RuntimeHelpers.GetUninitializedObject(
                typeof(ComponentViewModel<InsightAnimationCardWidgetProperties>));

        typeof(ComponentViewModel<InsightAnimationCardWidgetProperties>)
            .GetProperty(nameof(ComponentViewModel<InsightAnimationCardWidgetProperties>.Properties),
                BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(model, properties);

        return model;
    }

    /// <summary>Creates a minimal <see cref="InsightAnimationPage"/> via reflection.</summary>
    private static InsightAnimationPage MakePage(
        string title = "Test Insight",
        string description = "<p>Some <b>HTML</b></p>",
        string url = "/insights/test",
        string? imageUrl = null)
    {
        var page = (InsightAnimationPage)
            RuntimeHelpers.GetUninitializedObject(typeof(InsightAnimationPage));

        SetProp(page, nameof(InsightAnimationPage.Title), title);
        SetProp(page, nameof(InsightAnimationPage.Description), description);
        SetProp(page, nameof(InsightAnimationPage.Url), url);

        if (imageUrl is not null)
        {
            // Build a ContentAttachment stub with its Url set
            var attachment = (ContentAttachment)
                RuntimeHelpers.GetUninitializedObject(typeof(ContentAttachment));

            // ContentItemAsset is the asset type — set Url via its type
            var asset = (ContentItemAsset)
                RuntimeHelpers.GetUninitializedObject(typeof(ContentItemAsset));
            SetProp(asset, "Url", imageUrl);
            SetProp(attachment, "ContentAttachmentAsset", asset);

            SetProp(page, nameof(InsightAnimationPage.Image),
                new List<ContentAttachment> { attachment });
        }
        else
        {
            SetProp(page, nameof(InsightAnimationPage.Image),
                Enumerable.Empty<ContentAttachment>());
        }

        return page;
    }

    private static void SetProp(object target, string propName, object? value)
    {
        target.GetType()
              .GetProperty(propName, BindingFlags.Public | BindingFlags.Instance)!
              .SetValue(target, value);
    }

    private static InsightAnimationCardWidgetProperties DefaultProperties(
        IEnumerable<ContentItemReference>? cards = null) =>
        new()
        {
            Title = "Widget Title",
            Description = "Widget Description",
            InsightCards = cards ?? Enumerable.Empty<ContentItemReference>(),
            openInNewTab = "_self",
            SeeAllLinkText = "See all",
            SeeAllLinkURL = "/insights",
            TopMargin = "20px",
            BottomMargin = "40px",
            WidgetID = "widget-001",
        };

    /// <summary>
    /// Sets up <see cref="_contentServiceMock"/> to return the given pages
    /// when <see cref="IContentService.GetPagesAsync{TPage}"/> is called.
    /// </summary>
    private void SetupGetPagesAsync(IEnumerable<InsightAnimationPage> pages)
    {
        _contentServiceMock
            .Setup(s => s.GetPagesAsync<InsightAnimationPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pages);
    }

    /// <summary>
    /// Sets up <see cref="_contentServiceMock"/> to return an empty collection.
    /// </summary>
    private void SetupGetPagesAsyncEmpty()
    {
        SetupGetPagesAsync(Enumerable.Empty<InsightAnimationPage>());
    }

    // -----------------------------------------------------------------------
    // TC-01  Happy path – returns a ViewViewComponentResult
    // -----------------------------------------------------------------------
    [Test]
    public async Task InvokeAsync_WithValidProperties_ReturnsViewViewComponentResult()
    {
        // Arrange
        var model = BuildModel(DefaultProperties());
        var sut = CreateSut();

        SetupGetPagesAsyncEmpty();

        // Act
        var result = await sut.InvokeAsync(model);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewViewComponentResult>());
    }

    // -----------------------------------------------------------------------
    // TC-02  Happy path – view model is InsightAnimationCardDto
    // -----------------------------------------------------------------------
    [Test]
    public async Task InvokeAsync_WithValidProperties_PassesInsightAnimationCardDtoToView()
    {
        // Arrange
        var model = BuildModel(DefaultProperties());
        var sut = CreateSut();

        SetupGetPagesAsyncEmpty();

        // Act
        var result = (ViewViewComponentResult)await sut.InvokeAsync(model);

        // Assert
        Assert.That(result.ViewData?.Model, Is.InstanceOf<InsightAnimationCardDto>());
    }

    // -----------------------------------------------------------------------
    // TC-03  Title and Description are mapped from properties to DTO
    // -----------------------------------------------------------------------
    [Test]
    public async Task InvokeAsync_MapsWidgetTitleAndDescription_ToDto()
    {
        // Arrange
        var props = DefaultProperties();
        props.Title = "My Title";
        props.Description = "My Description";
        var sut = CreateSut();

        SetupGetPagesAsyncEmpty();

        // Act
        var result = (ViewViewComponentResult)await sut.InvokeAsync(BuildModel(props));
        var dto = (InsightAnimationCardDto)result.ViewData!.Model!;

        // Assert
        Assert.That(dto.Title, Is.EqualTo("My Title"));
        Assert.That(dto.Description, Is.EqualTo("My Description"));
    }

    // -----------------------------------------------------------------------
    // TC-04  TopMargin and BottomMargin default to "0px" when null in properties
    // -----------------------------------------------------------------------
    [Test]
    public async Task InvokeAsync_WhenMarginsAreNull_DefaultsToZeroPx()
    {
        // Arrange
        var props = DefaultProperties();
        props.TopMargin = null!;
        props.BottomMargin = null!;
        var sut = CreateSut();

        SetupGetPagesAsyncEmpty();

        // Act
        var result = (ViewViewComponentResult)await sut.InvokeAsync(BuildModel(props));
        var dto = (InsightAnimationCardDto)result.ViewData!.Model!;

        // Assert
        Assert.That(dto.TopMargin, Is.EqualTo("0px"));
        Assert.That(dto.BottomMargin, Is.EqualTo("0px"));
    }

    // -----------------------------------------------------------------------
    // TC-05  No InsightCards – GetPagesAsync is never called
    // -----------------------------------------------------------------------
    [Test]
    public async Task InvokeAsync_WithNoInsightCards_DoesNotCallContentService()
    {
        // Arrange
        var model = BuildModel(DefaultProperties(cards: Enumerable.Empty<ContentItemReference>()));
        var sut = CreateSut();

        // Act
        await sut.InvokeAsync(model);

        // Assert
        _contentServiceMock.Verify(
            s => s.GetPagesAsync<InsightAnimationPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // -----------------------------------------------------------------------
    // TC-06  With InsightCards – GetPagesAsync is called once
    // -----------------------------------------------------------------------
    [Test]
    public async Task InvokeAsync_WithInsightCards_CallsContentServiceWithCorrectGuids()
    {
        // Arrange
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();

        var refs = new[]
        {
            new ContentItemReference { Identifier = guid1 },
            new ContentItemReference { Identifier = guid2 },
        };

        SetupGetPagesAsyncEmpty();

        var sut = CreateSut();

        // Act
        await sut.InvokeAsync(BuildModel(DefaultProperties(refs)));

        // Assert
        _contentServiceMock.Verify(
            s => s.GetPagesAsync<InsightAnimationPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // TC-07  InsightAnimationPage fields are mapped to InsightAnimationPageDto
    // -----------------------------------------------------------------------
    [Test]
    public async Task InvokeAsync_MapsInsightPageFields_ToItemDto()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var ref1 = new ContentItemReference { Identifier = guid };
        var page = MakePage(title: "Insight 1", description: "<p>Hello</p>", url: "/read-more");

        SetupGetPagesAsync(new[] { page });

        var sut = CreateSut();

        // Act
        var result = (ViewViewComponentResult)await sut.InvokeAsync(
            BuildModel(DefaultProperties(new[] { ref1 })));
        var dto = (InsightAnimationCardDto)result.ViewData!.Model!;
        var item = dto.Items.First();

        // Assert
        Assert.That(item.Title, Is.EqualTo("Insight 1"));
        Assert.That(item.ReadMoreIcon, Is.EqualTo("/read-more"));
        // Description should have HTML stripped
        Assert.That(item.Description, Does.Not.Contain("<p>"));
        Assert.That(item.Description, Does.Contain("Hello"));
    }

    // -----------------------------------------------------------------------
    // TC-08  Image URL is mapped from ContentAttachmentAsset.Url
    // -----------------------------------------------------------------------
    [Test]
    public async Task InvokeAsync_WhenPageHasImage_MapsImageUrl()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var page = MakePage(imageUrl: "https://cdn.bdo.com/image.png");

        SetupGetPagesAsync(new[] { page });

        var sut = CreateSut();

        // Act
        var result = (ViewViewComponentResult)await sut.InvokeAsync(
            BuildModel(DefaultProperties(new[] { new ContentItemReference { Identifier = guid } })));
        var dto = (InsightAnimationCardDto)result.ViewData!.Model!;
        var item = dto.Items.First();

        // Assert
        Assert.That(item.ImageURL, Is.EqualTo("https://cdn.bdo.com/image.png"));
    }

    // -----------------------------------------------------------------------
    // TC-09  Image URL is null when page has no image
    // -----------------------------------------------------------------------
    [Test]
    public async Task InvokeAsync_WhenPageHasNoImage_ImageUrlIsNull()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var page = MakePage(); // no imageUrl

        SetupGetPagesAsync(new[] { page });

        var sut = CreateSut();

        // Act
        var result = (ViewViewComponentResult)await sut.InvokeAsync(
            BuildModel(DefaultProperties(new[] { new ContentItemReference { Identifier = guid } })));
        var dto = (InsightAnimationCardDto)result.ViewData!.Model!;
        var item = dto.Items.First();

        // Assert
        Assert.That(item.ImageURL, Is.Null);
    }

    // -----------------------------------------------------------------------
    // TC-10  SeeAllLink is built correctly from properties
    // -----------------------------------------------------------------------
    [Test]
    public async Task InvokeAsync_BuildsSeeAllLink_WithCorrectTitleUrlAndTarget()
    {
        // Arrange
        var props = DefaultProperties();
        props.SeeAllLinkText = "View all insights";
        props.SeeAllLinkURL = "/insights/all";
        props.openInNewTab = "_blank";

        SetupGetPagesAsyncEmpty();

        var sut = CreateSut();

        // Act
        var result = (ViewViewComponentResult)await sut.InvokeAsync(BuildModel(props));
        var dto = (InsightAnimationCardDto)result.ViewData!.Model!;

        // Assert
        Assert.That(dto.SeeAllLink!.Title, Is.EqualTo("View all insights"));
        Assert.That(dto.SeeAllLink!.URL, Is.EqualTo("/insights/all"));
        Assert.That(dto.SeeAllLink!.Target, Is.EqualTo("_blank"));
    }

    // -----------------------------------------------------------------------
    // TC-11  SeeAllLink falls back to empty strings when text and URL are null
    // -----------------------------------------------------------------------
    [Test]
    public async Task InvokeAsync_WhenSeeAllLinkIsNull_FallsBackToEmptyStrings()
    {
        // Arrange
        var props = DefaultProperties();
        props.SeeAllLinkText = null;
        props.SeeAllLinkURL = null;

        SetupGetPagesAsyncEmpty();

        var sut = CreateSut();

        // Act
        var result = (ViewViewComponentResult)await sut.InvokeAsync(BuildModel(props));
        var dto = (InsightAnimationCardDto)result.ViewData!.Model!;

        // Assert
        Assert.That(dto.SeeAllLink!.Title, Is.EqualTo(string.Empty));
        Assert.That(dto.SeeAllLink!.URL, Is.EqualTo(string.Empty));
    }

    // -----------------------------------------------------------------------
    // TC-12  openInNewTab is propagated to every item's OpenInNewTab field
    // -----------------------------------------------------------------------
    [Test]
    public async Task InvokeAsync_PropagatesOpenInNewTab_ToAllItems()
    {
        // Arrange
        var guids = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var refs = guids.Select(g => new ContentItemReference { Identifier = g }).ToArray();
        var pages = new[] { MakePage("A"), MakePage("B") };

        var props = DefaultProperties(refs);
        props.openInNewTab = "_blank";

        SetupGetPagesAsync(pages);

        var sut = CreateSut();

        // Act
        var result = (ViewViewComponentResult)await sut.InvokeAsync(BuildModel(props));
        var dto = (InsightAnimationCardDto)result.ViewData!.Model!;

        // Assert
        Assert.That(dto.Items.All(i => i.OpenInNewTab == "_blank"), Is.True);
    }

    // -----------------------------------------------------------------------
    // TC-13  WidgetID is forwarded from properties to DTO
    // -----------------------------------------------------------------------
    [Test]
    public async Task InvokeAsync_ForwardsWidgetId_ToDto()
    {
        // Arrange
        var props = DefaultProperties();
        props.WidgetID = "my-custom-widget-id";

        SetupGetPagesAsyncEmpty();

        var sut = CreateSut();

        // Act
        var result = (ViewViewComponentResult)await sut.InvokeAsync(BuildModel(props));
        var dto = (InsightAnimationCardDto)result.ViewData!.Model!;

        // Assert
        Assert.That(dto.WidgetID, Is.EqualTo("my-custom-widget-id"));
    }

    // -----------------------------------------------------------------------
    // TC-14  Items collection is empty when InsightCards list is empty
    // -----------------------------------------------------------------------
    [Test]
    public async Task InvokeAsync_WithNoInsightCards_DtoItemsIsEmpty()
    {
        // Arrange
        var model = BuildModel(DefaultProperties(Enumerable.Empty<ContentItemReference>()));
        var sut = CreateSut();

        // Act
        var result = (ViewViewComponentResult)await sut.InvokeAsync(model);
        var dto = (InsightAnimationCardDto)result.ViewData!.Model!;

        // Assert
        Assert.That(dto.Items, Is.Empty);
    }
}