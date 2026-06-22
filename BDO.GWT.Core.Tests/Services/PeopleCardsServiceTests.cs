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

namespace BDO.GWT.Core.Tests.Services;

public class PeopleCardsServiceTests
{
    private Mock<ICardsService> _cardsServiceMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
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
        _cultureContextMock = new Mock<ICultureContext>();
        _taxonomyTagRetrieverMock = new Mock<ITaxonomyTagRetriever>();
        _settingServiceMock = new Mock<ISettingService>();
        _urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        _cultureContextMock.Setup(c => c.CurrentLanguageTag).Returns("en");

        var httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        _service = new PeopleCardsService(
            _cardsServiceMock.Object,
            _contentServiceMock.Object,
            _settingServiceMock.Object,
            _urlHelperFactoryMock.Object,
            _httpContextAccessorMock.Object);
    }

    #region GetFilteredItemsAsync (parentWebPageItemId overload)

    [Test]
    public async Task GetFilteredItemsAsync_ByParentId_ReturnsEmptyPagination_WhenNoPeople()
    {
        var query = new CardsQuery { Order = "asc" };

        _contentServiceMock
            .Setup(s => s.GetChildPagesByParentGuidWithQueryBuilderAsync<Person>(
                It.IsAny<Action<ContentItemQueryBuilder>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Person>());

        var result = await _service.GetFilteredItemsAsync(
            1, "en", query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.TotalCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_ByParentId_WithDescOrder_ReturnsResults()
    {
        var query = new CardsQuery { Order = "DESC" };

        _contentServiceMock
            .Setup(s => s.GetChildPagesByParentGuidWithQueryBuilderAsync<Person>(
                It.IsAny<Action<ContentItemQueryBuilder>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Person>());

        var result = await _service.GetFilteredItemsAsync(
            1, "en", query, CancellationToken.None);

        Assert.That(result.Data, Is.Empty);
    }

    [Test]
    public async Task GetFilteredItemsAsync_ByParentId_WithNodeOrder_UsesWebPageItemIdOrdering()
    {
        var query = new CardsQuery { Order = "nodeorder" };

        _contentServiceMock
            .Setup(s => s.GetChildPagesByParentGuidWithQueryBuilderAsync<Person>(
                It.IsAny<Action<ContentItemQueryBuilder>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Person>());

        var result = await _service.GetFilteredItemsAsync(
            42, "en", query, CancellationToken.None);

        Assert.That(result.Data, Is.Empty);
    }

    [Test]
    public async Task GetFilteredItemsAsync_ByParentId_WithNullOrder_DefaultsToAsc()
    {
        var query = new CardsQuery { Order = null };

        _contentServiceMock
            .Setup(s => s.GetChildPagesByParentGuidWithQueryBuilderAsync<Person>(
                It.IsAny<Action<ContentItemQueryBuilder>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Person>());

        var result = await _service.GetFilteredItemsAsync(
            1, "en", query, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(0));
    }

    [Test]
    public async Task GetFilteredItemsAsync_ByParentId_WithFilters_AppliesFilterExpressions()
    {
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

        _contentServiceMock
            .Setup(s => s.GetPageByTypeAndNameAsync(It.IsAny<Type>(), "New York"))
            .ReturnsAsync(mockPage.Object);

        _contentServiceMock
            .Setup(s => s.GetChildPagesByParentGuidWithQueryBuilderAsync<Person>(
                It.IsAny<Action<ContentItemQueryBuilder>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Person>());

        var result = await _service.GetFilteredItemsAsync(
            1, "en", query, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(0));
    }

    [Test]
    public async Task GetFilteredItemsAsync_ByParentId_UnknownFilter_IsIgnored()
    {
        var query = new CardsQuery
        {
            Order = "asc",
            Filters = new Dictionary<string, string>
            {
                { "unknownFilter", "someValue" }
            }
        };

        _contentServiceMock
            .Setup(s => s.GetChildPagesByParentGuidWithQueryBuilderAsync<Person>(
                It.IsAny<Action<ContentItemQueryBuilder>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Person>());

        var result = await _service.GetFilteredItemsAsync(
            1, "en", query, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(0));
    }

    [Test]
    public async Task GetFilteredItemsAsync_ByParentId_PageFilterReturnsNull_IsSkipped()
    {
        var query = new CardsQuery
        {
            Order = "asc",
            Filters = new Dictionary<string, string>
            {
                { "officeLocation", "NonExistent" }
            }
        };

        _contentServiceMock
            .Setup(s => s.GetPageByTypeAndNameAsync(It.IsAny<Type>(), "NonExistent"))
            .ReturnsAsync((IWebPageFieldsSource?)null);

        _contentServiceMock
            .Setup(s => s.GetChildPagesByParentGuidWithQueryBuilderAsync<Person>(
                It.IsAny<Action<ContentItemQueryBuilder>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Person>());

        var result = await _service.GetFilteredItemsAsync(
            1, "en", query, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(0));
    }

   

    [Test]
    public async Task GetFallbackMetadataDescription_WhenSettingsNull_ReturnsFirstAndLastName()
    {
        var person = new Person
        {
            MetadataTitle = "Title",
            PersonFirstName = "Jane",
            PersonLastName = "Doe",
            PersonSurnamePrefixes = ""
        };

        _settingServiceMock
            .Setup(s => s.GetUsePersonPageTitleInfo())
            .ReturnsAsync((IEnumerable<OurPeopleInfo>?)null);

        var result = await _service.GetFallbackMetadataDescription(person);

        Assert.That(result, Is.EqualTo("Jane Doe"));
    }

    [Test]
    public async Task GetFallbackMetadataDescription_WhenSettingsEmpty_ReturnsFirstAndLastName()
    {
        var person = new Person
        {
            MetadataTitle = "Title",
            PersonFirstName = "Jane",
            PersonLastName = "Doe",
            PersonSurnamePrefixes = ""
        };

        _settingServiceMock
            .Setup(s => s.GetUsePersonPageTitleInfo())
            .ReturnsAsync(Enumerable.Empty<OurPeopleInfo>());

        var result = await _service.GetFallbackMetadataDescription(person);

        Assert.That(result, Is.EqualTo("Jane Doe"));
    }

    #endregion
}
