using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Services;
using BDO.GWT.GLOBAL.Widgets;
using BDO.GWT.Web.Widgets.RelatedContactsWidget;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget;

public class RelatedContactsWidgetTests
{
    private Mock<IRelatedContactsService> _relatedContactsServiceMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private RelatedContactsWidget _widget = null!;
    private DefaultHttpContext _httpContext = null!;

    [SetUp]
    public void SetUp()
    {
        _relatedContactsServiceMock = new Mock<IRelatedContactsService>();
        _contentServiceMock = new Mock<IContentService>();

        _widget = new RelatedContactsWidget(
            _relatedContactsServiceMock.Object,
            _contentServiceMock.Object);

        _httpContext = new DefaultHttpContext();
        _httpContext.RequestAborted = CancellationToken.None;

        _widget.ViewComponentContext = new ViewComponentContext
        {
            ViewContext = new ViewContext
            {
                HttpContext = _httpContext,
                ViewData = new ViewDataDictionary(
                    new EmptyModelMetadataProvider(),
                    new ModelStateDictionary())
            }
        };
    }

    private static ComponentViewModel<RelatedContactsWidgetProperties> CreateModel(
        RelatedContactsWidgetProperties properties,
        Guid? pageGuid = null)
    {
        var model = new ComponentViewModel<RelatedContactsWidgetProperties>();

        typeof(ComponentViewModel<RelatedContactsWidgetProperties>)
            .GetProperty("Properties")
            ?.SetValue(model, properties);

        if (pageGuid.HasValue)
        {
            var page = new RoutedWebPage { WebPageItemGUID = pageGuid.Value };
            typeof(ComponentViewModel<RelatedContactsWidgetProperties>)
                .GetProperty("Page")
                ?.SetValue(model, page);
        }

        return model;
    }

    private static PeopleGridDto CreateDefaultDto() =>
        new() { Items = [], TopMargin = "0px", BottomMargin = "0px" };

    private void SetupContentServiceMock(IWebPageFieldsSource? page = null)
    {
        _contentServiceMock
            .Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<IWebPageFieldsSource> { page ?? new Mock<IWebPageFieldsSource>().Object });
    }

    [Test]
    public async Task InvokeAsync_WithNullPage_ReturnsEmptyDto()
    {
        var result = await _widget.InvokeAsync(
            CreateModel(new RelatedContactsWidgetProperties(), pageGuid: null));

        var viewResult = result as ViewViewComponentResult;
        var dto = viewResult?.ViewData?.Model as PeopleGridDto;

        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.Items, Is.Null.Or.Empty);
    }

    [Test]
    public async Task InvokeAsync_WithNullPage_DoesNotCallService()
    {
        await _widget.InvokeAsync(CreateModel(new RelatedContactsWidgetProperties(), pageGuid: null));

        _relatedContactsServiceMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task InvokeAsync_WhenContentServiceReturnsEmpty_ReturnsEmptyDto()
    {
        var pageGuid = Guid.NewGuid();

        _contentServiceMock
            .Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(Enumerable.Empty<IWebPageFieldsSource>().ToList());

        var result = await _widget.InvokeAsync(
            CreateModel(new RelatedContactsWidgetProperties(), pageGuid));

        var viewResult = result as ViewViewComponentResult;
        var dto = viewResult?.ViewData?.Model as PeopleGridDto;

        Assert.That(dto, Is.Not.Null);
        _relatedContactsServiceMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task InvokeAsync_WithAllProperties_MapsDataToServiceCorrectly()
    {
        var pageGuid = Guid.NewGuid();
        SetupContentServiceMock();

        RelatedContactsData? capturedData = null;
        _relatedContactsServiceMock
            .Setup(x => x.GetRelatedContactsDtoAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<RelatedContactsData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, RelatedContactsData, CancellationToken>((_, d, _) => capturedData = d)
            .ReturnsAsync(CreateDefaultDto());

        var properties = new RelatedContactsWidgetProperties
        {
            Title = "Related Contacts",
            Description = "Description text",
            OrderCriteria = "lastName",
            IsDescendingOrder = true,
            ShowOffice = true,
            SeeAllLinkText = "See All",
            SeeAllLinkURL = "/people",
            SeeAllLinkURLTarget = "_blank",
            TopMargin = "20px",
            BottomMargin = "40px",
            WidgetID = "related-contacts-1"
        };

        await _widget.InvokeAsync(CreateModel(properties, pageGuid));

        Assert.That(capturedData, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedData!.Title, Is.EqualTo("Related Contacts"));
            Assert.That(capturedData!.Description, Is.EqualTo("Description text"));
            Assert.That(capturedData!.OrderCriteria, Is.EqualTo("lastName"));
            Assert.That(capturedData!.IsDescendingOrder, Is.True);
            Assert.That(capturedData!.ShowOffice, Is.True);
            Assert.That(capturedData!.SeeAllLinkText, Is.EqualTo("See All"));
            Assert.That(capturedData!.SeeAllLinkUrl, Is.EqualTo("/people"));
            Assert.That(capturedData!.SeeAllLinkURLTarget, Is.EqualTo("_blank"));
            Assert.That(capturedData!.TopMargin, Is.EqualTo("20px"));
            Assert.That(capturedData!.BottomMargin, Is.EqualTo("40px"));
            Assert.That(capturedData!.WidgetID, Is.EqualTo("related-contacts-1"));
        });
    }

    [Test]
    public async Task InvokeAsync_WithDefaultProperties_PassesDefaultValuesToService()
    {
        var pageGuid = Guid.NewGuid();
        SetupContentServiceMock();

        RelatedContactsData? capturedData = null;
        _relatedContactsServiceMock
            .Setup(x => x.GetRelatedContactsDtoAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<RelatedContactsData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, RelatedContactsData, CancellationToken>((_, d, _) => capturedData = d)
            .ReturnsAsync(CreateDefaultDto());

        await _widget.InvokeAsync(CreateModel(new RelatedContactsWidgetProperties(), pageGuid));

        Assert.That(capturedData, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedData!.IsDescendingOrder, Is.False);
            Assert.That(capturedData!.ShowOffice, Is.False);
            Assert.That(capturedData!.SeeAllLinkURLTarget, Is.EqualTo("_self"));
            Assert.That(capturedData!.TopMargin, Is.EqualTo("0px"));
            Assert.That(capturedData!.BottomMargin, Is.EqualTo("0px"));
        });
    }

    [Test]
    public async Task InvokeAsync_PassesCancellationTokenToService()
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        _httpContext.RequestAborted = token;

        var pageGuid = Guid.NewGuid();
        SetupContentServiceMock();

        CancellationToken capturedToken = default;
        _relatedContactsServiceMock
            .Setup(x => x.GetRelatedContactsDtoAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<RelatedContactsData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, RelatedContactsData, CancellationToken>((_, _, t) => capturedToken = t)
            .ReturnsAsync(CreateDefaultDto());

        await _widget.InvokeAsync(CreateModel(new RelatedContactsWidgetProperties(), pageGuid));

        Assert.That(capturedToken, Is.EqualTo(token));
    }

    [Test]
    public async Task InvokeAsync_ReturnsViewWithServiceResult()
    {
        var pageGuid = Guid.NewGuid();
        SetupContentServiceMock();

        var expectedDto = new PeopleGridDto
        {
            Title = "Returned Title",
            Items = [new PersonCardDto { DisplayName = "John Doe" }],
            TopMargin = "20px",
            BottomMargin = "40px",
            WidgetID = "widget-id"
        };

        _relatedContactsServiceMock
            .Setup(x => x.GetRelatedContactsDtoAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<RelatedContactsData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _widget.InvokeAsync(CreateModel(new RelatedContactsWidgetProperties(), pageGuid));

        var viewResult = result as ViewViewComponentResult;
        var dto = viewResult?.ViewData?.Model as PeopleGridDto;

        Assert.That(dto, Is.Not.Null);
        Assert.That(dto, Is.SameAs(expectedDto));
    }

    [Test]
    public async Task InvokeAsync_ReturnsCorrectViewPath()
    {
        var pageGuid = Guid.NewGuid();
        SetupContentServiceMock();

        _relatedContactsServiceMock
            .Setup(x => x.GetRelatedContactsDtoAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<RelatedContactsData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateDefaultDto());

        var result = await _widget.InvokeAsync(CreateModel(new RelatedContactsWidgetProperties(), pageGuid));

        var viewResult = result as ViewViewComponentResult;

        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult!.ViewName, Is.EqualTo("~/Components/Widget/RelatedContactsWidget/_RelatedContactsWidget.cshtml"));
    }

    [Test]
    public async Task InvokeAsync_PassesCurrentPageToService()
    {
        var pageGuid = Guid.NewGuid();
        var currentPage = new Mock<IWebPageFieldsSource>().Object;
        SetupContentServiceMock(currentPage);

        IWebPageFieldsSource? capturedPage = null;
        _relatedContactsServiceMock
            .Setup(x => x.GetRelatedContactsDtoAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<RelatedContactsData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, RelatedContactsData, CancellationToken>((p, _, _) => capturedPage = p)
            .ReturnsAsync(CreateDefaultDto());

        await _widget.InvokeAsync(CreateModel(new RelatedContactsWidgetProperties(), pageGuid));

        Assert.That(capturedPage, Is.SameAs(currentPage));
    }

    [Test]
    public async Task InvokeAsync_WithPageGuid_CallsGetItemsByGUIDWithCorrectGuid()
    {
        var pageGuid = Guid.NewGuid();
        List<Guid>? capturedGuids = null;

        _contentServiceMock
            .Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
            .Callback<List<Guid>>(g => capturedGuids = g)
            .ReturnsAsync(Enumerable.Empty<IWebPageFieldsSource>().ToList());

        await _widget.InvokeAsync(CreateModel(new RelatedContactsWidgetProperties(), pageGuid));

        Assert.That(capturedGuids, Is.Not.Null);
        Assert.That(capturedGuids, Contains.Item(pageGuid));
    }

    [Test]
    public async Task InvokeAsync_WithNullPage_ReturnsCorrectViewPath()
    {
        var result = await _widget.InvokeAsync(
            CreateModel(new RelatedContactsWidgetProperties(), pageGuid: null));

        var viewResult = result as ViewViewComponentResult;

        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult!.ViewName, Is.EqualTo("~/Components/Widget/RelatedContactsWidget/_RelatedContactsWidget.cshtml"));
    }
}
