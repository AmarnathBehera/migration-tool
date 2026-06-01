using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.services;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Tests.Services;

[TestFixture]
public class SpecialtiesAreaPageServiceTests
{
    private Mock<IHeroService> _heroServiceMock = null!;
    private Mock<IPageUtility> _pageUtilityMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;

    private SpecialtiesAreaPageService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _heroServiceMock = new Mock<IHeroService>();
        _pageUtilityMock = new Mock<IPageUtility>();
        _contentServiceMock = new Mock<IContentService>();

        _service = new SpecialtiesAreaPageService(
            _heroServiceMock.Object,
            _pageUtilityMock.Object,
            _contentServiceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _heroServiceMock.Reset();
        _pageUtilityMock.Reset();
        _contentServiceMock.Reset();
    }

    private static SpecialtiesArea CreatePage(bool showSideNavigation = false)
    {
        return new SpecialtiesArea
        {
            ContentTitle = "Area Title",
            ContentBody = "<p>Body Content</p>",
            HeroLayout = "default",
            HeroTitle = "Hero Title",
            HeroDescription = "Hero Description",
            HeroBackgroundImageAltText = "Desktop Alt",
            HeroBackgroundImage_MobileAltText = "Mobile Alt",
            HeroCallToActionButtonType = "primary",
            HeroCallToActionButtonText = "Learn More",
            ShowSideNavigation = showSideNavigation,
            SideNavigationTitle = "Navigation",
            SystemFields = new CMS.Websites.WebPageFields
            {
                WebPageItemID = 100,
                WebPageItemParentID = 200
            }
        };
    }

    [Test]
    public async Task GetSpecialtiesAreaPageDtoAsync_CurrentPageNull_ReturnsEmptyDto()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesArea>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SpecialtiesArea?)null);

        var result = await _service.GetSpecialtiesAreaPageDtoAsync(
            CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.Null);
        Assert.That(result.Hero, Is.Null);
        Assert.That(result.SideBox, Is.Null);
    }

    [Test]
    public async Task GetSpecialtiesAreaPageDtoAsync_ReturnsDtoWithMappedValues()
    {
        var page = CreatePage();

        var heroDto = new HeroDto
        {
            Title = "Hero"
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesArea>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(heroDto);

        var result = await _service.GetSpecialtiesAreaPageDtoAsync(
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo(page.ContentTitle));
            Assert.That(result.Hero, Is.EqualTo(heroDto));
        });
    }

    [Test]
    public async Task GetSpecialtiesAreaPageDtoAsync_MapsHeroDataCorrectly()
    {
        var page = CreatePage();

        HeroData? capturedHeroData = null;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesArea>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .Callback<HeroData, CancellationToken>(
                (heroData, _) => capturedHeroData = heroData)
            .ReturnsAsync(new HeroDto());

        await _service.GetSpecialtiesAreaPageDtoAsync(
            CancellationToken.None);

        Assert.That(capturedHeroData, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(capturedHeroData!.Title,
                Is.EqualTo(page.HeroTitle));

            Assert.That(capturedHeroData.Description,
                Is.EqualTo(page.HeroDescription));

            Assert.That(capturedHeroData.Layout,
                Is.EqualTo(page.HeroLayout));

            Assert.That(capturedHeroData.CallToActionButtonText,
                Is.EqualTo(page.HeroCallToActionButtonText));

            Assert.That(capturedHeroData.CallToActionButtonType,
                Is.EqualTo(page.HeroCallToActionButtonType));
        });
    }

    [Test]
    public async Task GetSpecialtiesAreaPageDtoAsync_ShowSideNavigationFalse_DoesNotCallPageUtility()
    {
        var page = CreatePage(false);

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesArea>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        await _service.GetSpecialtiesAreaPageDtoAsync(
            CancellationToken.None);

        _pageUtilityMock.Verify(
            x => x.GetSideboxDtoAsync<SpecialtiesPage, SpecialtiesArea>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Func<SpecialtiesPage, string>>(),
                It.IsAny<System.Func<SpecialtiesArea, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetSpecialtiesAreaPageDtoAsync_ShowSideNavigationTrue_CallsPageUtility()
    {
        var page = CreatePage(true);

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesArea>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        _pageUtilityMock
            .Setup(x => x.GetSideboxDtoAsync<
                SpecialtiesPage,
                SpecialtiesArea>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Func<SpecialtiesPage, string>>(),
                It.IsAny<System.Func<SpecialtiesArea, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SideBoxDto());

        await _service.GetSpecialtiesAreaPageDtoAsync(
            CancellationToken.None);

        _pageUtilityMock.Verify(
            x => x.GetSideboxDtoAsync<SpecialtiesPage, SpecialtiesArea>(
                page.SideNavigationTitle,
                page.SystemFields.WebPageItemID,
                page.SystemFields.WebPageItemParentID,
                It.IsAny<System.Func<SpecialtiesPage, string>>(),
                It.IsAny<System.Func<SpecialtiesArea, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSpecialtiesAreaPageDtoAsync_ShowSideNavigationTrue_ReturnsSideBox()
    {
        var page = CreatePage(true);

        var sideBox = new SideBoxDto
        {
            Title = "Navigation"
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesArea>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        _pageUtilityMock
            .Setup(x => x.GetSideboxDtoAsync<
                SpecialtiesPage,
                SpecialtiesArea>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Func<SpecialtiesPage, string>>(),
                It.IsAny<System.Func<SpecialtiesArea, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sideBox);

        var result = await _service.GetSpecialtiesAreaPageDtoAsync(
            CancellationToken.None);

        Assert.That(result.SideBox, Is.EqualTo(sideBox));
    }

    [Test]
    public async Task GetSpecialtiesAreaPageDtoAsync_CallsHeroServiceOnce()
    {
        var page = CreatePage();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesArea>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        await _service.GetSpecialtiesAreaPageDtoAsync(
            CancellationToken.None);

        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSpecialtiesAreaPageDtoAsync_ForwardsCancellationTokenToHeroService()
    {
        var page = CreatePage();

        var token = new CancellationTokenSource().Token;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesArea>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                token))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                token))
            .ReturnsAsync(new HeroDto());

        await _service.GetSpecialtiesAreaPageDtoAsync(token);

        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                token),
            Times.Once);
    }

    [Test]
    public async Task GetSpecialtiesAreaPageDtoAsync_ForwardsCancellationTokenToPageUtility()
    {
        var page = CreatePage(true);

        var token = new CancellationTokenSource().Token;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesArea>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                token))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                token))
            .ReturnsAsync(new HeroDto());

        _pageUtilityMock
            .Setup(x => x.GetSideboxDtoAsync<
                SpecialtiesPage,
                SpecialtiesArea>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Func<SpecialtiesPage, string>>(),
                It.IsAny<System.Func<SpecialtiesArea, string>>(),
                token))
            .ReturnsAsync(new SideBoxDto());

        await _service.GetSpecialtiesAreaPageDtoAsync(token);

        _pageUtilityMock.Verify(
            x => x.GetSideboxDtoAsync<
                SpecialtiesPage,
                SpecialtiesArea>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Func<SpecialtiesPage, string>>(),
                It.IsAny<System.Func<SpecialtiesArea, string>>(),
                token),
            Times.Once);
    }

    [Test]
    public async Task GetSpecialtiesAreaPageDtoAsync_WhenSideNavigationDisabled_ReturnsNullSideBox()
    {
        var page = CreatePage(false);

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesArea>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        var result = await _service.GetSpecialtiesAreaPageDtoAsync(
            CancellationToken.None);

        Assert.That(result.SideBox, Is.Null);
    }
}