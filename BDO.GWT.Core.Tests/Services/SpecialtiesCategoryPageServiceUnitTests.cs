
using BDO.GWT.Core.DTOs;

using BDO.GWT.Core.Services;
using BDO.GWT.Core.services;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;


namespace BDO.GWT.Core.Tests.Services;


public class SpecialtiesCategoryPageServiceUnitTests
{
    private Mock<IHeroService> _heroServiceMock = null!;
    private Mock<IPageUtility> _pageUtilityMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;

    private SpecialtiesCategoryPageService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _heroServiceMock = new Mock<IHeroService>();
        _pageUtilityMock = new Mock<IPageUtility>();
        _contentServiceMock = new Mock<IContentService>();

        _service = new SpecialtiesCategoryPageService(
            _heroServiceMock.Object,
            _pageUtilityMock.Object,
            _contentServiceMock.Object);
    }

    [Test]
    public async Task GetSpecialtiesCategoryPageDtoAsync_ReturnsEmptyDto_WhenCurrentPageIsNull()
    {
        var token = CancellationToken.None;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesCategory>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                token))
            .ReturnsAsync((SpecialtiesCategory?)null);

        var result = await _service.GetSpecialtiesCategoryPageDtoAsync(token);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.Null);
        Assert.That(result.BodyContent, Is.Null);
        Assert.That(result.Hero, Is.Null);
        Assert.That(result.SideBox, Is.Null);
    }

    [Test]
    public async Task GetSpecialtiesCategoryPageDtoAsync_CallsContentServiceOnce()
    {
        var page = CreatePage();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesCategory>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<Core.Data.HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        await _service.GetSpecialtiesCategoryPageDtoAsync(CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<SpecialtiesCategory>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSpecialtiesCategoryPageDtoAsync_ReturnsTitle()
    {
        var page = CreatePage();
        page.ContentTitle = "Category Title";

        SetupPage(page);

        var result = await _service.GetSpecialtiesCategoryPageDtoAsync(CancellationToken.None);

        Assert.That(result.Title, Is.EqualTo("Category Title"));
    }

    [Test]
    public async Task GetSpecialtiesCategoryPageDtoAsync_ReturnsBodyContent()
    {
        var page = CreatePage();
        page.ContentBody = "<p>Body</p>";

        SetupPage(page);

        var result = await _service.GetSpecialtiesCategoryPageDtoAsync(CancellationToken.None);

        Assert.That(result.BodyContent, Is.Not.Null);
    }

    [Test]
    public async Task GetSpecialtiesCategoryPageDtoAsync_ReturnsHeroDto()
    {
        var page = CreatePage();

        var heroDto = new HeroDto
        {
            Title = "Hero"
        };

        SetupPage(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<Core.Data.HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(heroDto);

        var result = await _service.GetSpecialtiesCategoryPageDtoAsync(CancellationToken.None);

        Assert.That(result.Hero, Is.EqualTo(heroDto));
    }

    [Test]
    public async Task GetSpecialtiesCategoryPageDtoAsync_CallsHeroServiceOnce()
    {
        SetupPage(CreatePage());

        await _service.GetSpecialtiesCategoryPageDtoAsync(CancellationToken.None);

        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(
                It.IsAny<Core.Data.HeroData>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSpecialtiesCategoryPageDtoAsync_DoesNotCreateSideBox_WhenNavigationDisabled()
    {
        var page = CreatePage();
        page.ShowSideNavigation = false;

        SetupPage(page);

        var result = await _service.GetSpecialtiesCategoryPageDtoAsync(CancellationToken.None);

        Assert.That(result.SideBox, Is.Null);

        _pageUtilityMock.Verify(
            x => x.GetSideboxDtoAsync<SpecialtiesArea, SpecialtiesCategory>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Func<SpecialtiesArea, string>>(),
                It.IsAny<System.Func<SpecialtiesCategory, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetSpecialtiesCategoryPageDtoAsync_CreatesSideBox_WhenNavigationEnabled()
    {
        var page = CreatePage();
        page.ShowSideNavigation = true;

        SetupPage(page);

        _pageUtilityMock
            .Setup(x => x.GetSideboxDtoAsync<SpecialtiesArea, SpecialtiesCategory>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Func<SpecialtiesArea, string>>(),
                It.IsAny<System.Func<SpecialtiesCategory, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SideBoxDto());

        await _service.GetSpecialtiesCategoryPageDtoAsync(CancellationToken.None);

        _pageUtilityMock.Verify(
            x => x.GetSideboxDtoAsync<SpecialtiesArea, SpecialtiesCategory>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Func<SpecialtiesArea, string>>(),
                It.IsAny<System.Func<SpecialtiesCategory, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSpecialtiesCategoryPageDtoAsync_ReturnsSideBox()
    {
        var page = CreatePage();
        page.ShowSideNavigation = true;

        var sideBox = new SideBoxDto
        {
            Title = "Navigation"
        };

        SetupPage(page);

        _pageUtilityMock
            .Setup(x => x.GetSideboxDtoAsync<SpecialtiesArea, SpecialtiesCategory>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Func<SpecialtiesArea, string>>(),
                It.IsAny<System.Func<SpecialtiesCategory, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sideBox);

        var result = await _service.GetSpecialtiesCategoryPageDtoAsync(CancellationToken.None);

        Assert.That(result.SideBox, Is.EqualTo(sideBox));
    }

    [Test]
    public async Task GetSpecialtiesCategoryPageDtoAsync_PassesSideNavigationTitle()
    {
        var page = CreatePage();
        page.ShowSideNavigation = true;
        page.SideNavigationTitle = "My Navigation";

        SetupPage(page);

        await _service.GetSpecialtiesCategoryPageDtoAsync(CancellationToken.None);

        _pageUtilityMock.Verify(
            x => x.GetSideboxDtoAsync<SpecialtiesArea, SpecialtiesCategory>(
                "My Navigation",
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Func<SpecialtiesArea, string>>(),
                It.IsAny<System.Func<SpecialtiesCategory, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSpecialtiesCategoryPageDtoAsync_PassesParentId()
    {
        var page = CreatePage();
        page.ShowSideNavigation = true;
        page.SystemFields.WebPageItemID = 123;

        SetupPage(page);

        await _service.GetSpecialtiesCategoryPageDtoAsync(CancellationToken.None);

        _pageUtilityMock.Verify(
            x => x.GetSideboxDtoAsync<SpecialtiesArea, SpecialtiesCategory>(
                It.IsAny<string>(),
                123,
                It.IsAny<int>(),
                It.IsAny<System.Func<SpecialtiesArea, string>>(),
                It.IsAny<System.Func<SpecialtiesCategory, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSpecialtiesCategoryPageDtoAsync_PassesParentSiblingId()
    {
        var page = CreatePage();
        page.ShowSideNavigation = true;
        page.SystemFields.WebPageItemParentID = 456;

        SetupPage(page);

        await _service.GetSpecialtiesCategoryPageDtoAsync(CancellationToken.None);

        _pageUtilityMock.Verify(
            x => x.GetSideboxDtoAsync<SpecialtiesArea, SpecialtiesCategory>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                456,
                It.IsAny<System.Func<SpecialtiesArea, string>>(),
                It.IsAny<System.Func<SpecialtiesCategory, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSpecialtiesCategoryPageDtoAsync_PassesCancellationTokenToHeroService()
    {
        var token = new CancellationTokenSource().Token;

        SetupPage(CreatePage());

        await _service.GetSpecialtiesCategoryPageDtoAsync(token);

        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(
                It.IsAny<Core.Data.HeroData>(),
                token),
            Times.Once);
    }

    [Test]
    public async Task GetSpecialtiesCategoryPageDtoAsync_PassesCancellationTokenToSideBoxService()
    {
        var token = new CancellationTokenSource().Token;

        var page = CreatePage();
        page.ShowSideNavigation = true;

        SetupPage(page);

        await _service.GetSpecialtiesCategoryPageDtoAsync(token);

        _pageUtilityMock.Verify(
            x => x.GetSideboxDtoAsync<SpecialtiesArea, SpecialtiesCategory>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Func<SpecialtiesArea, string>>(),
                It.IsAny<System.Func<SpecialtiesCategory, string>>(),
                token),
            Times.Once);
    }

    [Test]
    public async Task GetSpecialtiesCategoryPageDtoAsync_ReturnsExpectedDto()
    {
        var page = CreatePage();
        page.ContentTitle = "Title";
        page.ContentBody = "<p>Body</p>";
        page.ShowSideNavigation = true;

        var hero = new HeroDto();
        var sideBox = new SideBoxDto();

        SetupPage(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<Core.Data.HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(hero);

        _pageUtilityMock
            .Setup(x => x.GetSideboxDtoAsync<SpecialtiesArea, SpecialtiesCategory>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Func<SpecialtiesArea, string>>(),
                It.IsAny<System.Func<SpecialtiesCategory, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sideBox);

        var result = await _service.GetSpecialtiesCategoryPageDtoAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo("Title"));
            Assert.That(result.Hero, Is.EqualTo(hero));
            Assert.That(result.SideBox, Is.EqualTo(sideBox));
        });
    }

    private void SetupPage(SpecialtiesCategory page)
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SpecialtiesCategory>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<Core.Data.HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());
    }

    private static SpecialtiesCategory CreatePage()
    {
        return new SpecialtiesCategory
        {
            ContentTitle = "Title",
            ContentBody = "<p>Body</p>",
            ShowSideNavigation = false,
            SideNavigationTitle = "Side Nav",
            HeroLayout = "Default",
            HeroTitle = "Hero Title",
            HeroDescription = "Hero Description",
            HeroBackgroundImage = new List<ContentAttachment>(),
            HeroBackgroundImage_Mobile = new List<ContentAttachment>(),
            HeroCallToActionButtonUrl = new List<IWebPageFieldsSource>(),
            SystemFields = new WebPageFields
            {
                WebPageItemID = 100,
                WebPageItemParentID = 200
            }
        };
    }
}