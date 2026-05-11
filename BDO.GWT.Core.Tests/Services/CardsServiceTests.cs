using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Services;

public class CardsServiceTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
    private Mock<ITaxonomyTagRetriever> _taxonomyTagRetrieverMock = null!;
    private Mock<ICultureContext> _cultureContextMock = null!;
    private CardsService _service = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _taxonomyTagRetrieverMock = new Mock<ITaxonomyTagRetriever>();
        _cultureContextMock = new Mock<ICultureContext>();

        _cultureContextMock.Setup(c => c.CurrentLanguageTag).Returns("en");
        _websiteChannelContextMock.Setup(c => c.WebsiteChannelID).Returns(1);

        _service = new CardsService(
            _contentServiceMock.Object,
            _websiteChannelContextMock.Object,
            _taxonomyTagRetrieverMock.Object,
            _cultureContextMock.Object);
    }

    [Test]
    public async Task CreateFiltersAsync_ReturnsEmpty_WhenNoValidFilterNames()
    {
        var filters = new HashSet<string> { "unknownFilter" };

        var result = await _service.CreateFiltersAsync(filters, null, CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task CreateFiltersAsync_ReturnsPageFilter_ForOfficeLocation()
    {
        var filters = new HashSet<string> { "officeLocation" };

        var mockOffice = new Mock<LocationOffice>();
        var systemFields = new Mock<IContentItemFieldsSource>();
        _contentServiceMock
            .Setup(s => s.GetItemsByTypesAsync(
                 It.IsAny<Type[]>(),
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object>());

        var result = await _service.CreateFiltersAsync(filters, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("officeLocation"));
            Assert.That(result[0].Type, Is.EqualTo("select"));
            Assert.That(result[0].Options, Is.Empty);
        });
    }

    [Test]
    public async Task CreateFiltersAsync_ReturnsTaxonomyFilter_ForCredential()
    {
        var filters = new HashSet<string> { "credential" };

        var tags = new List<Tag>
        {
            new() { Name = "CPA" },
            new() { Name = "CFA" }
        };

        _taxonomyTagRetrieverMock
            .Setup(t => t.GetTaxonomyTagsAsync(
                It.IsAny<string>(),
                "en",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        var result = await _service.CreateFiltersAsync(filters, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("credential"));
            Assert.That(result[0].Options, Has.Length.EqualTo(2));
            Assert.That(result[0].Options[0].Value, Is.EqualTo("CFA"));
            Assert.That(result[0].Options[1].Value, Is.EqualTo("CPA"));
        });
    }

    [Test]
    public async Task CreateFiltersAsync_ReturnsMultipleFilters_ForMixedTypes()
    {
        var filters = new HashSet<string> { "officeLocation", "credential" };

        _contentServiceMock
            .Setup(s => s.GetItemsByTypesAsync(
                It.IsAny<Type[]>(),
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object>());

        _taxonomyTagRetrieverMock
            .Setup(t => t.GetTaxonomyTagsAsync(
                It.IsAny<string>(),
                "en",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tag> { new() { Name = "CPA" } });

        var result = await _service.CreateFiltersAsync(filters, null, CancellationToken.None);

        Assert.That(result, Has.Length.EqualTo(2));
    }

    [Test]
    public async Task CreateFiltersAsync_ExcludesEmptyTaxonomyTags()
    {
        var filters = new HashSet<string> { "profileGroup" };

        var tags = new List<Tag>
        {
            new() { Name = "Audit" },
            new() { Name = "" },
            new() { Name = null! }
        };

        _taxonomyTagRetrieverMock
            .Setup(t => t.GetTaxonomyTagsAsync(
                It.IsAny<string>(),
                "en",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        var result = await _service.CreateFiltersAsync(filters, null, CancellationToken.None);

        Assert.That(result[0].Options, Has.Length.EqualTo(1));
        Assert.That(result[0].Options[0].Value, Is.EqualTo("Audit"));
    }
}