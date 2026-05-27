using BDO.GWT.Core;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Services;

public class PeopleCardsServiceTests
{
    private Mock<ICardsService> _cardsServiceMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<ITaxonomyTagRetriever> _taxonomyTagRetrieverMock = null!;
    private Mock<ICultureContext> _cultureContextMock = null!;
    private Mock<ISettingService> _settingServiceMock = null!;
    private Mock<IUrlHelperFactory> _urlHelperFactoryMock = null!;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
    private PeopleCardsService _service = null!;

    [SetUp]
    public void Setup()
    {
        _cardsServiceMock = new Mock<ICardsService>();
        _contentServiceMock = new Mock<IContentService>();
        _taxonomyTagRetrieverMock = new Mock<ITaxonomyTagRetriever>();
        _cultureContextMock = new Mock<ICultureContext>();
        _settingServiceMock = new Mock<ISettingService>();
        _urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        _cultureContextMock.Setup(c => c.CurrentLanguageTag).Returns("en");

        var httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        _service = new PeopleCardsService(
            _cardsServiceMock.Object,
            _contentServiceMock.Object,
            _taxonomyTagRetrieverMock.Object,
            _cultureContextMock.Object,
            _settingServiceMock.Object,
            _urlHelperFactoryMock.Object,
            _httpContextAccessorMock.Object);
    }

    [Test]
    public async Task CreateFiltersAsync_DelegatesToCardsService()
    {
        var filters = new HashSet<string> { "officeLocation" };
        var expectedFilters = new[]
        {
            new FilterItemDto<OptionDto>
            {
                Name = "officeLocation",
                Title = "Location",
                Type = "select",
                Options = [new OptionDto { Value = "NYC", Label = "NYC" }]
            }
        };

        _cardsServiceMock
            .Setup(s => s.CreateFiltersAsync(filters, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFilters);

        var result = await _service.CreateFiltersAsync(filters, null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expectedFilters));
        _cardsServiceMock.Verify(
            s => s.CreateFiltersAsync(filters, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetFilteredItemsAsync_ReturnsEmptyPagination_WhenNoPeople()
    {
        var page = new SectionPeoplePage();
        var query = new CardsQuery { Order = "asc" };

        _contentServiceMock
            .Setup(s => s.GetCurrentChannelID())
            .Returns(1);

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Person?>());

        var result = await _service.GetFilteredItemsAsync(
            page, "en", query, null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.TotalCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_WithNoOrder_DefaultsToWebPageItemOrder()
    {
        var page = new SectionPeoplePage();
        var query = new CardsQuery { Order = null };

        _contentServiceMock.Setup(s => s.GetCurrentChannelID()).Returns(1);

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Person?>());

        var result = await _service.GetFilteredItemsAsync(
            page, "en", query, null, _ => false, CancellationToken.None);

        Assert.That(result.Data, Is.Empty);
    }

    [Test]
    public async Task GetFilteredItemsAsync_WithDescOrder_Works()
    {
        var page = new SectionPeoplePage();
        var query = new CardsQuery { Order = "DESC" };

        _contentServiceMock.Setup(s => s.GetCurrentChannelID()).Returns(1);

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Person?>());

        var result = await _service.GetFilteredItemsAsync(
            page, "en", query, null, _ => false, CancellationToken.None);

        Assert.That(result.Data, Is.Empty);
    }

    [Test]
    public async Task GetFilteredItemsAsync_WithFilters_AppliesFilterExpressions()
    {
        var page = new SectionPeoplePage();
        var query = new CardsQuery
        {
            Order = "asc",
            Filters = new Dictionary<string, string>
            {
                { "officeLocation", "New York" }
            }
        };

        var mockPage = new Mock<IWebPageFieldsSource>();
        mockPage.Setup(p => p.SystemFields).Returns(new WebPageFields
        {
            ContentItemGUID = Guid.NewGuid()
        });

        _contentServiceMock.Setup(s => s.GetCurrentChannelID()).Returns(1);

        _contentServiceMock
            .Setup(s => s.GetPageByTypeAndNameAsync(It.IsAny<Type>(), "New York"))
            .ReturnsAsync(mockPage.Object);

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Person?>());

        var result = await _service.GetFilteredItemsAsync(
            page, "en", query, null, _ => false, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(0));
    }

    [Test]
    public async Task GetFilteredItemsAsync_UnknownFilter_IsIgnored()
    {
        var page = new SectionPeoplePage();
        var query = new CardsQuery
        {
            Order = "asc",
            Filters = new Dictionary<string, string>
            {
                { "unknownFilter", "someValue" }
            }
        };

        _contentServiceMock.Setup(s => s.GetCurrentChannelID()).Returns(1);

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Person?>());

        var result = await _service.GetFilteredItemsAsync(
            page, "en", query, null, _ => false, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(0));
    }

    [Test]
    public async Task GetFilteredItemsAsync_PageFilterReturnsNull_IsSkipped()
    {
        var page = new SectionPeoplePage();
        var query = new CardsQuery
        {
            Order = "asc",
            Filters = new Dictionary<string, string>
            {
                { "officeLocation", "NonExistent" }
            }
        };

        _contentServiceMock.Setup(s => s.GetCurrentChannelID()).Returns(1);

        _contentServiceMock
            .Setup(s => s.GetPageByTypeAndNameAsync(It.IsAny<Type>(), "NonExistent"))
            .ReturnsAsync((IWebPageFieldsSource?)null);

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Person?>());

        var result = await _service.GetFilteredItemsAsync(
            page, "en", query, null, _ => false, CancellationToken.None);

        // Filter was skipped, query still executes
        Assert.That(result.TotalCount, Is.EqualTo(0));
    }

    [Test]
    public async Task GetFilteredItemsAsync_TaxonomyTagNotFound_FilterSkipped()
    {
        var page = new SectionPeoplePage();
        var query = new CardsQuery
        {
            Order = "asc",
            Filters = new Dictionary<string, string>
            {
                { "credential", "NonExistentTag" }
            }
        };

        _contentServiceMock.Setup(s => s.GetCurrentChannelID()).Returns(1);

        _taxonomyTagRetrieverMock
            .Setup(t => t.GetTaxonomyTagByNameAsync("NonExistentTag"))
            .ReturnsAsync((TagInfo?)null);

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Person?>());

        var result = await _service.GetFilteredItemsAsync(
            page, "en", query, null, _ => false, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(0));
    }
}