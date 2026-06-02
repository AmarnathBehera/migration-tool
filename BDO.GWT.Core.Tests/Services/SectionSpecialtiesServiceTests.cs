using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.services;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Tests.Services;

[TestFixture]
public class SectionSpecialtiesServiceTests
{
    private Mock<IHeroService> _heroServiceMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IPageUtility> _pageUtilityMock = null!;

    private SectionSpecialtiesService _service = null!;

    // ─── Setup / TearDown ────────────────────────────────────────────────────

    [SetUp]
    public void SetUp()
    {
        _heroServiceMock = new Mock<IHeroService>();
        _contentServiceMock = new Mock<IContentService>();
        _pageUtilityMock = new Mock<IPageUtility>();

        _service = new SectionSpecialtiesService(
            _heroServiceMock.Object,
            _contentServiceMock.Object,
            _pageUtilityMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _heroServiceMock.Reset();
        _contentServiceMock.Reset();
        _pageUtilityMock.Reset();
    }

    // ─── Factory helpers ─────────────────────────────────────────────────────

    private static SectionSpecialties CreatePage(
        string gridTitle = "Grid Title",
        string gridSummary = "Grid Summary",
        bool showMultiLinks = true,
        int webPageItemId = 100)
    {
        return new SectionSpecialties
        {
            HeroLayout = "default",
            HeroTitle = "Hero Title",
            HeroDescription = "Hero Description",
            HeroBackgroundImageAltText = "Desktop Alt",
            HeroBackgroundImage_MobileAltText = "Mobile Alt",
            HeroCallToActionButtonType = "primary",
            HeroCallToActionButtonText = "Learn More",
            GridTitle = gridTitle,
            GridSummary = gridSummary,
            ShowMultiLinks = showMultiLinks,
            SystemFields = new CMS.Websites.WebPageFields
            {
                WebPageItemID = webPageItemId
            }
        };
    }

    private static MultiLinkCardDto[] CreateCards(int count = 2)
    {
        var cards = new MultiLinkCardDto[count];
        for (var i = 0; i < count; i++)
        {
            cards[i] = new MultiLinkCardDto
            {
                Title = $"Card {i + 1}",
                Description = $"Desc {i + 1}",
                Url = $"/card-{i + 1}"
            };
        }
        return cards;
    }

    // =========================================================================
    // 1. Returns empty DTO when current page is null
    // =========================================================================
    [Test]
    public async Task GetSectionSpecialtiesDtoAsync_CurrentPageNull_ReturnsEmptyDto()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionSpecialties>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SectionSpecialties?)null);

        var result = await _service.GetSectionSpecialtiesDtoAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Hero, Is.Null);
            Assert.That(result.MultiLinksCardsGrid, Is.Null);
        });

    }

    // =========================================================================
    // 2. Neither HeroService nor PageUtility is called when page is null
    // =========================================================================
    [Test]
    public async Task GetSectionSpecialtiesDtoAsync_CurrentPageNull_DoesNotCallDependencies()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionSpecialties>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SectionSpecialties?)null);

        await _service.GetSectionSpecialtiesDtoAsync(CancellationToken.None);

        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _pageUtilityMock.Verify(
            x => x.GetMultiLinkCardsAsync<SpecialtiesCategory, SpecialtiesArea>(
                It.IsAny<int>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string?>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesArea, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // =========================================================================
    // 3. ContentService is called with LinkedItemsMaxLevel = 4
    // =========================================================================
    [Test]
    public async Task GetSectionSpecialtiesDtoAsync_CallsContentServiceWithLinkedItemsMaxLevel4()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionSpecialties>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SectionSpecialties?)null);

        await _service.GetSectionSpecialtiesDtoAsync(CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<SectionSpecialties>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================================
    // 4. HeroDto returned from HeroService is assigned to the result
    // =========================================================================
    [Test]
    public async Task GetSectionSpecialtiesDtoAsync_AssignsHeroDtoFromHeroService()
    {
        var page = CreatePage();
        var heroDto = new HeroDto { Layout = "default", Title = "My Hero" };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionSpecialties>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(heroDto);

        _pageUtilityMock
            .Setup(x => x.GetMultiLinkCardsAsync<SpecialtiesCategory, SpecialtiesArea>(
                It.IsAny<int>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string?>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesArea, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MultiLinkCardDto>());

        var result = await _service.GetSectionSpecialtiesDtoAsync(CancellationToken.None);

        Assert.That(result.Hero, Is.SameAs(heroDto));
    }

    // =========================================================================
    // 5. HeroData is built with correct field values from the page
    // =========================================================================
    [Test]
    public async Task GetSectionSpecialtiesDtoAsync_MapsHeroDataFieldsCorrectly()
    {
        var page = CreatePage();
        HeroData? capturedHeroData = null;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionSpecialties>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .Callback<HeroData, CancellationToken>((hd, _) => capturedHeroData = hd)
            .ReturnsAsync(new HeroDto());

        _pageUtilityMock
            .Setup(x => x.GetMultiLinkCardsAsync<SpecialtiesCategory, SpecialtiesArea>(
                It.IsAny<int>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string?>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesArea, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MultiLinkCardDto>());

        await _service.GetSectionSpecialtiesDtoAsync(CancellationToken.None);

        Assert.That(capturedHeroData, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(capturedHeroData!.Layout, Is.EqualTo(page.HeroLayout));
            Assert.That(capturedHeroData.Title, Is.EqualTo(page.HeroTitle));
            Assert.That(capturedHeroData.Description, Is.EqualTo(page.HeroDescription));
            Assert.That(capturedHeroData.BackgroundImageAltText, Is.EqualTo(page.HeroBackgroundImageAltText));
            Assert.That(capturedHeroData.MobileBackgroundImageAltText, Is.EqualTo(page.HeroBackgroundImage_MobileAltText));
            Assert.That(capturedHeroData.CallToActionButtonType, Is.EqualTo(page.HeroCallToActionButtonType));
            Assert.That(capturedHeroData.CallToActionButtonText, Is.EqualTo(page.HeroCallToActionButtonText));
        });
    }

    // =========================================================================
    // 6. BackgroundImage is null in HeroData when HeroBackgroundImage is empty
    // =========================================================================
    [Test]
    public async Task GetSectionSpecialtiesDtoAsync_WhenBgImageEmpty_HeroDataBackgroundImageIsNull()
    {
        var page = CreatePage();
        page.HeroBackgroundImage = Array.Empty<ContentAttachment>();

        HeroData? capturedHeroData = null;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionSpecialties>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .Callback<HeroData, CancellationToken>((hd, _) => capturedHeroData = hd)
            .ReturnsAsync(new HeroDto());

        _pageUtilityMock
            .Setup(x => x.GetMultiLinkCardsAsync<SpecialtiesCategory, SpecialtiesArea>(
                It.IsAny<int>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string?>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesArea, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MultiLinkCardDto>());

        await _service.GetSectionSpecialtiesDtoAsync(CancellationToken.None);

        Assert.That(capturedHeroData!.BackgroundImage, Is.Null);
    }

    // =========================================================================
    // 7. MobileBackgroundImage is null in HeroData when mobile image is empty
    // =========================================================================
    [Test]
    public async Task GetSectionSpecialtiesDtoAsync_WhenMobileImageEmpty_HeroDataMobileImageIsNull()
    {
        var page = CreatePage();
        page.HeroBackgroundImage_Mobile = Array.Empty<ContentAttachment>();

        HeroData? capturedHeroData = null;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionSpecialties>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .Callback<HeroData, CancellationToken>((hd, _) => capturedHeroData = hd)
            .ReturnsAsync(new HeroDto());

        _pageUtilityMock
            .Setup(x => x.GetMultiLinkCardsAsync<SpecialtiesCategory, SpecialtiesArea>(
                It.IsAny<int>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string?>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesArea, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MultiLinkCardDto>());

        await _service.GetSectionSpecialtiesDtoAsync(CancellationToken.None);

        Assert.That(capturedHeroData!.MobileBackgroundImage, Is.Null);
    }

    // =========================================================================
    // 8. CTA URL is null in HeroData when HeroCallToActionButtonUrl is empty
    // =========================================================================
    [Test]
    public async Task GetSectionSpecialtiesDtoAsync_WhenCtaUrlEmpty_HeroDataCtaUrlIsNull()
    {
        var page = CreatePage();
        page.HeroCallToActionButtonUrl = Array.Empty<IWebPageFieldsSource>();

        HeroData? capturedHeroData = null;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionSpecialties>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .Callback<HeroData, CancellationToken>((hd, _) => capturedHeroData = hd)
            .ReturnsAsync(new HeroDto());

        _pageUtilityMock
            .Setup(x => x.GetMultiLinkCardsAsync<SpecialtiesCategory, SpecialtiesArea>(
                It.IsAny<int>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string?>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesArea, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MultiLinkCardDto>());

        await _service.GetSectionSpecialtiesDtoAsync(CancellationToken.None);

        Assert.That(capturedHeroData!.CallToActionButtonUrl, Is.Null);
    }

    // =========================================================================
    // 9. Grid Title and Description are mapped from the page
    // =========================================================================
    [Test]
    public async Task GetSectionSpecialtiesDtoAsync_MapsGridTitleAndDescriptionCorrectly()
    {
        var page = CreatePage(gridTitle: "My Grid", gridSummary: "My Summary");

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionSpecialties>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        _pageUtilityMock
            .Setup(x => x.GetMultiLinkCardsAsync<SpecialtiesCategory, SpecialtiesArea>(
                It.IsAny<int>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string?>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesArea, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MultiLinkCardDto>());

        var result = await _service.GetSectionSpecialtiesDtoAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.MultiLinksCardsGrid!.Title, Is.EqualTo("My Grid"));
            Assert.That(result.MultiLinksCardsGrid!.Description, Is.EqualTo("My Summary"));
        });
    }

    // =========================================================================
    // 10. IsMultiLinks is mapped from ShowMultiLinks on the page
    // =========================================================================
    [TestCase(true)]
    [TestCase(false)]
    public async Task GetSectionSpecialtiesDtoAsync_MapsIsMultiLinksFromPage(bool showMultiLinks)
    {
        var page = CreatePage(showMultiLinks: showMultiLinks);

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionSpecialties>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        _pageUtilityMock
            .Setup(x => x.GetMultiLinkCardsAsync<SpecialtiesCategory, SpecialtiesArea>(
                It.IsAny<int>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string?>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesArea, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MultiLinkCardDto>());

        var result = await _service.GetSectionSpecialtiesDtoAsync(CancellationToken.None);

        Assert.That(result.MultiLinksCardsGrid!.IsMultiLinks, Is.EqualTo(showMultiLinks));
    }

    // =========================================================================
    // 11. Cards returned from PageUtility are assigned to the grid
    // =========================================================================
    [Test]
    public async Task GetSectionSpecialtiesDtoAsync_AssignsCardsFromPageUtility()
    {
        var page = CreatePage(webPageItemId: 100);
        var cards = CreateCards(3);

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionSpecialties>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        _pageUtilityMock
            .Setup(x => x.GetMultiLinkCardsAsync<SpecialtiesCategory, SpecialtiesArea>(
                It.IsAny<int>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string?>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesArea, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cards);

        var result = await _service.GetSectionSpecialtiesDtoAsync(CancellationToken.None);

        Assert.That(result.MultiLinksCardsGrid!.Cards, Is.SameAs(cards));
    }

    // =========================================================================
    // 12. GetMultiLinkCardsAsync is called with the page's WebPageItemID
    // =========================================================================
    [Test]
    public async Task GetSectionSpecialtiesDtoAsync_CallsPageUtilityWithCorrectWebPageItemId()
    {
        var page = CreatePage(webPageItemId: 42);

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionSpecialties>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        _pageUtilityMock
            .Setup(x => x.GetMultiLinkCardsAsync<SpecialtiesCategory, SpecialtiesArea>(
                42,
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string?>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesArea, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MultiLinkCardDto>());

        await _service.GetSectionSpecialtiesDtoAsync(CancellationToken.None);

        _pageUtilityMock.Verify(
            x => x.GetMultiLinkCardsAsync<SpecialtiesCategory, SpecialtiesArea>(
                42,
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string?>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesArea, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================================
    // 13. HeroService is called exactly once
    // =========================================================================
    [Test]
    public async Task GetSectionSpecialtiesDtoAsync_CallsHeroServiceExactlyOnce()
    {
        var page = CreatePage();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionSpecialties>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        _pageUtilityMock
            .Setup(x => x.GetMultiLinkCardsAsync<SpecialtiesCategory, SpecialtiesArea>(
                It.IsAny<int>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string?>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesArea, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MultiLinkCardDto>());

        await _service.GetSectionSpecialtiesDtoAsync(CancellationToken.None);

        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================================
    // 14. CancellationToken is forwarded to HeroService
    // =========================================================================
    [Test]
    public async Task GetSectionSpecialtiesDtoAsync_ForwardsCancellationTokenToHeroService()
    {
        var page = CreatePage();
        var token = new CancellationTokenSource().Token;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionSpecialties>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                token))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), token))
            .ReturnsAsync(new HeroDto());

        _pageUtilityMock
            .Setup(x => x.GetMultiLinkCardsAsync<SpecialtiesCategory, SpecialtiesArea>(
                It.IsAny<int>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string?>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesArea, string>>(),
                token))
            .ReturnsAsync(Array.Empty<MultiLinkCardDto>());

        await _service.GetSectionSpecialtiesDtoAsync(token);

        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), token),
            Times.Once);
    }

    // =========================================================================
    // 15. CancellationToken is forwarded to PageUtility
    // =========================================================================
    [Test]
    public async Task GetSectionSpecialtiesDtoAsync_ForwardsCancellationTokenToPageUtility()
    {
        var page = CreatePage();
        var token = new CancellationTokenSource().Token;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionSpecialties>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                token))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), token))
            .ReturnsAsync(new HeroDto());

        _pageUtilityMock
            .Setup(x => x.GetMultiLinkCardsAsync<SpecialtiesCategory, SpecialtiesArea>(
                It.IsAny<int>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string?>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesArea, string>>(),
                token))
            .ReturnsAsync(Array.Empty<MultiLinkCardDto>());

        await _service.GetSectionSpecialtiesDtoAsync(token);

        _pageUtilityMock.Verify(
            x => x.GetMultiLinkCardsAsync<SpecialtiesCategory, SpecialtiesArea>(
                It.IsAny<int>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesCategory, string?>>(),
                It.IsAny<Func<SpecialtiesCategory, string>>(),
                It.IsAny<Func<SpecialtiesArea, string>>(),
                token),
            Times.Once);
    }
}