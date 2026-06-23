using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class BlogContributorsPageServiceTests
{
    private Mock<IPeopleCardsService> _peopleCardsServiceMock;
    private Mock<IContentService> _contentServiceMock;
    private Mock<IHeroService> _heroServiceMock;
    private Mock<ICultureContext> _cultureContextMock;
    private Mock<IPreferredLanguageRetriever> _preferredLanguageRetrieverMock;
    private BlogContributorsPageService _service;

    [SetUp]
    public void Setup()
    {
        _peopleCardsServiceMock = new Mock<IPeopleCardsService>();
        _contentServiceMock = new Mock<IContentService>();
        _heroServiceMock = new Mock<IHeroService>();
        _cultureContextMock = new Mock<ICultureContext>();
        _preferredLanguageRetrieverMock = new Mock<IPreferredLanguageRetriever>();

        _service = new BlogContributorsPageService(
            _peopleCardsServiceMock.Object,
            _contentServiceMock.Object,
            _heroServiceMock.Object,
            _cultureContextMock.Object,
            _preferredLanguageRetrieverMock.Object);
    }

    #region GetSectionPeoplePageDtoAsync

    [Test]
    public async Task GetSectionPeoplePageDtoAsync_ReturnsDto_WithCorrectHero()
    {
        var expectedHero = new HeroDto { Title = "Test Hero" };
        _heroServiceMock
            .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHero);

        var page = CreateBlogContributorsPage();

        var result = await _service.GetSectionPeoplePageDtoAsync(page, CancellationToken.None);

        Assert.That(result.Hero, Is.EqualTo(expectedHero));
    }

    [Test]
    public async Task GetSectionPeoplePageDtoAsync_CallsHeroService_WithCorrectHeroData()
    {
        HeroData? capturedHeroData = null;
        _heroServiceMock
            .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .Callback<HeroData, CancellationToken>((data, _) => capturedHeroData = data)
            .ReturnsAsync(new HeroDto());

        var page = CreateBlogContributorsPage(
            heroLayout: "full-width",
            heroTitle: "Contributors",
            heroDescription: "Our blog contributors");

        await _service.GetSectionPeoplePageDtoAsync(page, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(capturedHeroData, Is.Not.Null);
            Assert.That(capturedHeroData!.Layout, Is.EqualTo("full-width"));
            Assert.That(capturedHeroData.Title, Is.EqualTo("Contributors"));
            Assert.That(capturedHeroData.Description, Is.EqualTo("Our blog contributors"));
        });
    }

    [Test]
    public async Task GetSectionPeoplePageDtoAsync_ReturnsDto_WithPageSize20()
    {
        _heroServiceMock
            .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        var page = CreateBlogContributorsPage();

        var result = await _service.GetSectionPeoplePageDtoAsync(page, CancellationToken.None);

        Assert.That(result.PageSize, Is.EqualTo(20));
    }

    [Test]
    public async Task GetSectionPeoplePageDtoAsync_ReturnsDto_WithCorrectApiEndpoint()
    {
        var pageGuid = Guid.NewGuid();
        _heroServiceMock
            .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        var page = CreateBlogContributorsPage(contentItemGuid: pageGuid);

        var result = await _service.GetSectionPeoplePageDtoAsync(page, CancellationToken.None);

        Assert.That(result.ApiEndpoint, Is.EqualTo($"BlogContributors/{pageGuid}/Get"));
    }

    [Test]
    public async Task GetSectionPeoplePageDtoAsync_ReturnsDto_WithSortByFilter()
    {
        _heroServiceMock
            .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        var page = CreateBlogContributorsPage(defaultOrder: "asc");

        var result = await _service.GetSectionPeoplePageDtoAsync(page, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.SortBy, Is.Not.Null);
            Assert.That(result.SortBy!.Name, Is.EqualTo("Order"));
            Assert.That(result.SortBy.Type, Is.EqualTo("select"));
            Assert.That(result.SortBy.DefaultValue, Is.EqualTo("asc"));
            Assert.That(result.SortBy.Options, Has.Length.EqualTo(3));
        });
    }

    [Test]
    public async Task GetSectionPeoplePageDtoAsync_SortByOptions_ContainExpectedValues()
    {
        _heroServiceMock
            .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        var page = CreateBlogContributorsPage();

        var result = await _service.GetSectionPeoplePageDtoAsync(page, CancellationToken.None);

        var optionValues = result.SortBy!.Options.Select(o => o.Value).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(optionValues, Does.Contain("asc"));
            Assert.That(optionValues, Does.Contain("desc"));
            Assert.That(optionValues, Does.Contain("nodeorder"));
        });
    }

    [Test]
    public async Task GetSectionPeoplePageDtoAsync_PassesCorrectCancellationToken()
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _heroServiceMock
            .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), token))
            .ReturnsAsync(new HeroDto());

        var page = CreateBlogContributorsPage();

        await _service.GetSectionPeoplePageDtoAsync(page, token);

        _heroServiceMock.Verify(
            s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), token),
            Times.Once);
    }

    [Test]
    public async Task GetSectionPeoplePageDtoAsync_WhenNoBackgroundImages_HeroDataImagesAreNull()
    {
        HeroData? capturedHeroData = null;
        _heroServiceMock
            .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .Callback<HeroData, CancellationToken>((data, _) => capturedHeroData = data)
            .ReturnsAsync(new HeroDto());

        var page = CreateBlogContributorsPage();
        page.HeroBackgroundImage = Enumerable.Empty<ContentAttachment>();
        page.HeroBackgroundImage_Mobile = Enumerable.Empty<ContentAttachment>();
        page.HeroCallToActionButtonUrl = Enumerable.Empty<IWebPageFieldsSource>();

        await _service.GetSectionPeoplePageDtoAsync(page, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(capturedHeroData!.BackgroundImage, Is.Null);
            Assert.That(capturedHeroData.MobileBackgroundImage, Is.Null);
            Assert.That(capturedHeroData.CallToActionButtonUrl, Is.Null);
        });
    }

    #endregion

    #region GeBlogContributorsPaginationDtoAsync

    [Test]
    public async Task GeBlogContributorsPaginationDtoAsync_ReturnsDto_WhenPageExists()
    {
        var pageGuid = Guid.NewGuid();

        var cts = new CancellationTokenSource();
        var expectedPagination = new PaginationDto<PersonCardDto>
        {
            Data = [new PersonCardDto { DisplayName = "John Doe" }],
            TotalCount = 1
        };

        SetupContentServiceReturnsPage(pageGuid);
        SetupPeopleCardsServiceReturns(expectedPagination);

        var result = await _service.GetBlogContributorsPaginationDtoAsync(pageGuid, "en", new CardsQuery(), cts.Token);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Data, Has.Length.EqualTo(1));
        });
    }

    [Test]
    public void GeBlogContributorsPaginationDtoAsync_ThrowsMissingSectionPageException_WhenPageNotFound()
    {
        var pageGuid = Guid.NewGuid();

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<BlogContributors>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<BlogContributors>());

        Assert.ThrowsAsync<MissingSectionPageException>(async () =>
            await _service.GetBlogContributorsPaginationDtoAsync(
                pageGuid, "en", new CardsQuery(), CancellationToken.None));
    }

    [Test]
    public async Task GeBlogContributorsPaginationDtoAsync_CallsPeopleCardsService_WithCorrectWebPageItemId()
    {
        var pageGuid = Guid.NewGuid();
        var webPageItemId = 42;

        SetupContentServiceReturnsPage(pageGuid, webPageItemId);
        SetupPeopleCardsServiceReturns(new PaginationDto<PersonCardDto> { Data = [], TotalCount = 0 });

        await _service.GetBlogContributorsPaginationDtoAsync(
            pageGuid, "en", new CardsQuery(), CancellationToken.None);

        _peopleCardsServiceMock.Verify(
            s => s.GetFilteredItemsAsync(
                webPageItemId,
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GeBlogContributorsPaginationDtoAsync_UsesPreferredLanguage()
    {
        var pageGuid = Guid.NewGuid();
        const string expectedLanguage = "en";

        _preferredLanguageRetrieverMock.Setup(p => p.Get()).Returns(expectedLanguage);
        SetupContentServiceReturnsPage(pageGuid);
        SetupPeopleCardsServiceReturns(new PaginationDto<PersonCardDto> { Data = [], TotalCount = 0 });

        await _service.GetBlogContributorsPaginationDtoAsync(
            pageGuid, "en", new CardsQuery(), CancellationToken.None);

        _peopleCardsServiceMock.Verify(
            s => s.GetFilteredItemsAsync(
                It.IsAny<int>(),
                expectedLanguage,
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GeBlogContributorsPaginationDtoAsync_PassesCardsQueryThrough()
    {
        var pageGuid = Guid.NewGuid();
        var cardsQuery = new CardsQuery { CurrentPage = 2, PageSize = 10 };

        SetupContentServiceReturnsPage(pageGuid);
        SetupPeopleCardsServiceReturns(new PaginationDto<PersonCardDto> { Data = [], TotalCount = 0 });

        await _service.GetBlogContributorsPaginationDtoAsync(
            pageGuid, "en", cardsQuery, CancellationToken.None);

        _peopleCardsServiceMock.Verify(
            s => s.GetFilteredItemsAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                cardsQuery,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GeBlogContributorsPaginationDtoAsync_UsesLanguageParameter_ForPageRetrieval()
    {
        var pageGuid = Guid.NewGuid();
        const string expectedLanguage = "de-DE";

        SetupContentServiceReturnsPage(pageGuid);
        SetupPeopleCardsServiceReturns(new PaginationDto<PersonCardDto> { Data = [], TotalCount = 0 });

        await _service.GetBlogContributorsPaginationDtoAsync(
            pageGuid, expectedLanguage, new CardsQuery(), CancellationToken.None);

        _contentServiceMock.Verify(
            s => s.GetPagesAsync<BlogContributors>(
                It.Is<RetrievePagesParameters>(p => p.LanguageName == expectedLanguage),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GeBlogContributorsPaginationDtoAsync_DisablesLanguageFallbacks()
    {
        var pageGuid = Guid.NewGuid();

        SetupContentServiceReturnsPage(pageGuid);
        SetupPeopleCardsServiceReturns(new PaginationDto<PersonCardDto> { Data = [], TotalCount = 0 });

        await _service.GetBlogContributorsPaginationDtoAsync(
            pageGuid, "en", new CardsQuery(), CancellationToken.None);

        _contentServiceMock.Verify(
            s => s.GetPagesAsync<BlogContributors>(
                It.Is<RetrievePagesParameters>(p => p.UseLanguageFallbacks == false),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Helpers

    private static BlogContributors CreateBlogContributorsPage(
        string heroLayout = "default",
        string heroTitle = "Title",
        string heroDescription = "Description",
        string? defaultOrder = null,
        Guid? contentItemGuid = null)
    {
        var guid = contentItemGuid ?? Guid.NewGuid();
        return new BlogContributors
        {
            SystemFields = new WebPageFields { ContentItemGUID = guid },
            HeroLayout = heroLayout,
            HeroTitle = heroTitle,
            HeroDescription = heroDescription,
            HeroBackgroundImage = Enumerable.Empty<ContentAttachment>(),
            HeroBackgroundImageAltText = "bg alt",
            HeroBackgroundImage_Mobile = Enumerable.Empty<ContentAttachment>(),
            HeroBackgroundImage_MobileAltText = "mobile alt",
            HeroCallToActionButtonType = "primary",
            HeroCallToActionButtonText = "Click",
            HeroCallToActionButtonUrl = Enumerable.Empty<IWebPageFieldsSource>(),
            PeopleListDefaultOrder = defaultOrder ?? "asc",
        };
    }

    private void SetupContentServiceReturnsPage(Guid pageGuid, int webPageItemId = 1)
    {
        var page = new BlogContributors
        {
            SystemFields = new WebPageFields
            {
                ContentItemGUID = pageGuid,
                WebPageItemID = webPageItemId
            },
            PeopleListDefaultOrder = "asc",
            HeroBackgroundImage = Enumerable.Empty<ContentAttachment>(),
            HeroBackgroundImage_Mobile = Enumerable.Empty<ContentAttachment>(),
            HeroCallToActionButtonUrl = Enumerable.Empty<IWebPageFieldsSource>(),
        };

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<BlogContributors>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { page });
    }

    private void SetupPeopleCardsServiceReturns(PaginationDto<PersonCardDto> result)
    {
        _preferredLanguageRetrieverMock.Setup(p => p.Get()).Returns("en");

        _peopleCardsServiceMock
            .Setup(s => s.GetFilteredItemsAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    #endregion
}