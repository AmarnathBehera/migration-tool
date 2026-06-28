using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;
using System.Reflection;

namespace BDO.GWT.Core.Tests.Services;

public class EditorsPickCardServiceTests
{
    private Mock<ICardsService> _cardsServiceMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IWebPageUrlRetriever> _urlRetrieverMock = null!;
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private EditorsPickCardService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _cardsServiceMock = new Mock<ICardsService>();
        _contentServiceMock = new Mock<IContentService>();
        _urlRetrieverMock = new Mock<IWebPageUrlRetriever>();
        _contentRetrieverMock = new Mock<IContentRetriever>();

        _contentServiceMock
            .Setup(x => x.GetCurrentLanguage())
            .Returns("en-US");

        _service = new EditorsPickCardService(
            _cardsServiceMock.Object,
            _contentServiceMock.Object,
            _urlRetrieverMock.Object,
            _contentRetrieverMock.Object);
    }

    // ── Structure ────────────────────────────────────────────────────────────

    [Test]
    public void Service_ImplementsInterface()
    {
        Assert.That(typeof(IEditorsPickCardService).IsAssignableFrom(typeof(EditorsPickCardService)), Is.True);
    }

    [Test]
    public void Interface_DeclaresExpectedMembers()
    {
        var t = typeof(IEditorsPickCardService);
        Assert.Multiple(() =>
        {
            Assert.That(t.GetMethod(nameof(IEditorsPickCardService.CreateFiltersAsync)), Is.Not.Null);
            Assert.That(t.GetMethod(nameof(IEditorsPickCardService.GetFilteredItemsAsync)), Is.Not.Null);
        });
    }

    [Test]
    public void Service_HasExpectedConstructor()
    {
        var ctor = typeof(EditorsPickCardService)
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault();

        Assert.That(ctor, Is.Not.Null);
        Assert.That(ctor!.GetParameters(), Has.Length.GreaterThan(0));
    }

    // ── CreateFiltersAsync ───────────────────────────────────────────────────

    [Test]
    public async Task CreateFiltersAsync_DelegatesTo_CardsService()
    {
        var filters = new HashSet<string> { "contentType" };
        var expected = new[]
        {
            new FilterItemDto<OptionDto> { Name = "contentType", Type = "select", Options = [] }
        };

        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(filters, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _service.CreateFiltersAsync(filters, null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        _cardsServiceMock.Verify(
            x => x.CreateFiltersAsync(filters, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void CreateFiltersAsync_PropagatesException()
    {
        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("service failure"));

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateFiltersAsync(new HashSet<string>(), null, CancellationToken.None));
    }

    [Test]
    public async Task CreateFiltersAsync_PassesCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(),
                It.IsAny<string?>(),
                token))
            .ReturnsAsync([]);

        await _service.CreateFiltersAsync(new HashSet<string>(), null, token);

        _cardsServiceMock.Verify(
            x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(),
                It.IsAny<string?>(),
                token),
            Times.Once);
    }


}
