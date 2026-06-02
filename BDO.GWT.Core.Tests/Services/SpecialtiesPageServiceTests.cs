using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.services;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Tests.Services;

[TestFixture]
public class SpecialtiesPageServiceTests
{
    private Mock<IHeroService> _heroServiceMock = null!;
    private Mock<IPageUtility> _pageUtilityMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;

    private SpecialtiesPageService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _heroServiceMock = new Mock<IHeroService>();
        _pageUtilityMock = new Mock<IPageUtility>();
        _contentServiceMock = new Mock<IContentService>();

        _service = new SpecialtiesPageService(
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

    private static SpecialtiesPage CreatePage(
        bool showSideNavigation = true)
    {
        return new SpecialtiesPage
        {
            HeroLayout = "standard",
            HeroTitle = "Hero Title",
            HeroDescription = "Hero Description",
            HeroBackgroundImageAltText = "Desktop Alt",
            HeroBackgroundImage_MobileAltText = "Mobile Alt",
            HeroCallToActionButtonType = "Primary",
            HeroCallToActionButtonText = "Learn More",
            ShowSideNavigation = showSideNavigation,
            SideNavigationTitle = "Navigation",
            SystemFields = new CMS.Websites.WebPageFields
            {
                WebPageItemID = 100,
                WebPageItemParentID = 50
            },
            HeroBackgroundImage = Enumerable.Empty<ContentAttachment>(),
            HeroBackgroundImage_Mobile = Enumerable.Empty<ContentAttachment>(),
            HeroCallToActionButtonUrl = Enumerable.Empty<CMS.Websites.IWebPageFieldsSource>()
        };
    }

    [Test]
    public async Task GetSpecialtiesPageDtoAsync_CurrentPageNull_ReturnsEmptyDto()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SpecialtiesPage?)null);

        var result = await _service.GetSpecialtiesPageDtoAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Hero, Is.Null);
            Assert.That(result.SideBox, Is.Null);
        });

        

        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _pageUtilityMock.Verify(
            x => x.GetSideboxDtoAsync<SectionSpecialties, SpecialtiesPage>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Func<SectionSpecialties, string>>(),
                It.IsAny<System.Func<SpecialtiesPage, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetSpecialtiesPageDtoAsync_CallsRetrieveCurrentPageOnce()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePage());

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        await _service.GetSpecialtiesPageDtoAsync(CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<SpecialtiesPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSpecialtiesPageDtoAsync_ReturnsHeroFromHeroService()
    {
        var expectedHero = new HeroDto
        {
            Title = "Expected Hero"
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePage());

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHero);

        var result = await _service.GetSpecialtiesPageDtoAsync(CancellationToken.None);

        Assert.That(result.Hero, Is.EqualTo(expectedHero));
    }

    [Test]
    public async Task GetSpecialtiesPageDtoAsync_ShowSideNavigationTrue_ReturnsSidebox()
    {
        var sideBox = new SideBoxDto
        {
            Title = "Test Sidebox"
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePage(true));

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        _pageUtilityMock
            .Setup(x => x.GetSideboxDtoAsync<SpecialtiesPage>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Func<SpecialtiesPage, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sideBox);

        var result = await _service.GetSpecialtiesPageDtoAsync(CancellationToken.None);

        Assert.That(result.SideBox, Is.EqualTo(sideBox));
    }

    [Test]
    public async Task GetSpecialtiesPageDtoAsync_ShowSideNavigationFalse_DoesNotCallPageUtility()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePage(false));

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        var result = await _service.GetSpecialtiesPageDtoAsync(CancellationToken.None);

        Assert.That(result.SideBox, Is.Null);

        _pageUtilityMock.Verify(
            x => x.GetSideboxDtoAsync<SectionSpecialties, SpecialtiesPage>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Func<SectionSpecialties, string>>(),
                It.IsAny<System.Func<SpecialtiesPage, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetSpecialtiesPageDtoAsync_ShowSideNavigationTrue_CallsPageUtilityOnce()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePage(true));

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        _pageUtilityMock
            .Setup(x => x.GetSideboxDtoAsync<SpecialtiesPage>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Func<SpecialtiesPage, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SideBoxDto());

        await _service.GetSpecialtiesPageDtoAsync(CancellationToken.None);

        _pageUtilityMock.Verify(
            x => x.GetSideboxDtoAsync<SpecialtiesPage>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Func<SpecialtiesPage, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSpecialtiesPageDtoAsync_PassesSideNavigationTitleToPageUtility()
    {
        var page = CreatePage(true);
        page.SideNavigationTitle = "Custom Navigation";

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        await _service.GetSpecialtiesPageDtoAsync(CancellationToken.None);

        _pageUtilityMock.Verify(
            x => x.GetSideboxDtoAsync<SpecialtiesPage>(
                "Custom Navigation",
                page.SystemFields.WebPageItemID,
                page.SystemFields.WebPageItemParentID,
                It.IsAny<System.Func<SpecialtiesPage, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSpecialtiesPageDtoAsync_PassesCancellationTokenToHeroService()
    {
        var token = new CancellationTokenSource().Token;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                token))
            .ReturnsAsync(CreatePage());

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                token))
            .ReturnsAsync(new HeroDto());

        await _service.GetSpecialtiesPageDtoAsync(token);

        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                token),
            Times.Once);
    }

    [Test]
    public async Task GetSpecialtiesPageDtoAsync_HeroDataContainsCorrectTitle()
    {
        var page = CreatePage();
        page.HeroTitle = "Special Hero Title";

        HeroData? capturedHeroData = null;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .Callback<HeroData, CancellationToken>((h, _) => capturedHeroData = h)
            .ReturnsAsync(new HeroDto());

        await _service.GetSpecialtiesPageDtoAsync(CancellationToken.None);

        Assert.That(capturedHeroData, Is.Not.Null);
        Assert.That(capturedHeroData!.Title, Is.EqualTo("Special Hero Title"));
    }

    [Test]
    public async Task GetSpecialtiesPageDtoAsync_HeroDataContainsCorrectDescription()
    {
        var page = CreatePage();
        page.HeroDescription = "Special Description";

        HeroData? capturedHeroData = null;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .Callback<HeroData, CancellationToken>((h, _) => capturedHeroData = h)
            .ReturnsAsync(new HeroDto());

        await _service.GetSpecialtiesPageDtoAsync(CancellationToken.None);

        Assert.That(capturedHeroData!.Description, Is.EqualTo("Special Description"));
    }

    [Test]
    public async Task GetSpecialtiesPageDtoAsync_HeroDataContainsCorrectLayout()
    {
        var page = CreatePage();
        page.HeroLayout = "banner";

        HeroData? capturedHeroData = null;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .Callback<HeroData, CancellationToken>((h, _) => capturedHeroData = h)
            .ReturnsAsync(new HeroDto());

        await _service.GetSpecialtiesPageDtoAsync(CancellationToken.None);

        Assert.That(capturedHeroData!.Layout, Is.EqualTo("banner"));
    }

    [Test]
    public async Task GetSpecialtiesPageDtoAsync_HeroServiceCalledOnce()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePage());

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        await _service.GetSpecialtiesPageDtoAsync(CancellationToken.None);

        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSpecialtiesPageDtoAsync_ReturnsDtoWithHeroAndSideBox()
    {
        var hero = new HeroDto { Title = "Hero" };
        var sidebox = new SideBoxDto { Title = "Sidebox" };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePage());

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(hero);

        _pageUtilityMock
            .Setup(x => x.GetSideboxDtoAsync<SpecialtiesPage>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Func<SpecialtiesPage, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sidebox);

        var result = await _service.GetSpecialtiesPageDtoAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Hero, Is.EqualTo(hero));
            Assert.That(result.SideBox, Is.EqualTo(sidebox));
        });
    }

    [Test]
    public async Task GetSpecialtiesPageDtoAsync_PageUtilityReturnsNull_ReturnsNullSideBox()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePage());

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        _pageUtilityMock
    .Setup(x => x.GetSideboxDtoAsync<SectionSpecialties, SpecialtiesPage>(
        It.IsAny<string>(),
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<Func<SectionSpecialties, string>>(),
        It.IsAny<Func<SpecialtiesPage, string>>(),
        It.IsAny<CancellationToken>()))
    .ReturnsAsync((SideBoxDto)null!);

        var result = await _service.GetSpecialtiesPageDtoAsync(CancellationToken.None);

        Assert.That(result.SideBox, Is.Null);
    }

    [Test]
    public async Task GetSpecialtiesPageDtoAsync_UsesCurrentPageIdsForSidebox()
    {
        var page = CreatePage();
        page.SystemFields.WebPageItemID = 999;
        page.SystemFields.WebPageItemParentID = 777;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        await _service.GetSpecialtiesPageDtoAsync(CancellationToken.None);

        _pageUtilityMock.Verify(
            x => x.GetSideboxDtoAsync<SpecialtiesPage>(
                It.IsAny<string>(),
                999,
                777,

                It.IsAny<System.Func<SpecialtiesPage, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}