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
    private static readonly string[] ExpectedMultipleFilterNames = ["officeLocation", "service"];

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
       
        _websiteChannelContextMock.Setup(c => c.WebsiteChannelID).Returns(1);

        _service = new CardsService(
            _contentServiceMock.Object,
            _websiteChannelContextMock.Object
            );
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
    public async Task CreateFiltersAsync_ReturnsMultiplePageFilters()
    {
        var filters = new HashSet<string> { "officeLocation", "service" };

        _contentServiceMock
            .Setup(s => s.GetItemsByTypesAsync(
                It.IsAny<Type[]>(),
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object>());

        var result = await _service.CreateFiltersAsync(filters, null, CancellationToken.None);

        Assert.That(result, Has.Length.EqualTo(2));
        Assert.That(result.Select(r => r.Name), Is.EquivalentTo(ExpectedMultipleFilterNames));
    }

    [Test]
    public async Task CreateFiltersAsync_SkipsUnknownFilterNames_AndKeepsKnownOnes()
    {
        var filters = new HashSet<string> { "unknownFilter", "officeLocation" };

        _contentServiceMock
            .Setup(s => s.GetItemsByTypesAsync(
                It.IsAny<Type[]>(),
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object>());

        var result = await _service.CreateFiltersAsync(filters, null, CancellationToken.None);

        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("officeLocation"));
    }

    [Test]
    public async Task CreateFiltersAsync_PassesCancellationToken_ToContentService()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var filters = new HashSet<string> { "officeLocation" };

        _contentServiceMock
            .Setup(s => s.GetItemsByTypesAsync(
                It.IsAny<Type[]>(),
                1,
                token))
            .ReturnsAsync(new List<object>());

        await _service.CreateFiltersAsync(filters, null, token);

        _contentServiceMock.Verify(
            s => s.GetItemsByTypesAsync(It.IsAny<Type[]>(), 1, token),
            Times.Once);
    }

    [Test]
    public void CreateFiltersAsync_PropagatesException_FromContentService()
    {
        var filters = new HashSet<string> { "officeLocation" };

        _contentServiceMock
            .Setup(s => s.GetItemsByTypesAsync(
                It.IsAny<Type[]>(),
                1,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateFiltersAsync(filters, null, CancellationToken.None));
    }

    [Test]
    public async Task CreateFiltersAsync_UsesWebsiteChannelID_FromContext()
    {
        _websiteChannelContextMock.Setup(c => c.WebsiteChannelID).Returns(42);
        var filters = new HashSet<string> { "officeLocation" };

        _contentServiceMock
            .Setup(s => s.GetItemsByTypesAsync(
                It.IsAny<Type[]>(),
                42,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object>());

        await _service.CreateFiltersAsync(filters, null, CancellationToken.None);

        _contentServiceMock.Verify(
            s => s.GetItemsByTypesAsync(It.IsAny<Type[]>(), 42, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task CreateFiltersAsync_Empty_WhenNoFilters()
    {
        var result = await _service.CreateFiltersAsync(new HashSet<string>(), null, CancellationToken.None);

        Assert.That(result, Is.Empty);
    }
}