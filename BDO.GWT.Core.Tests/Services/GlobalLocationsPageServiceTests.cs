using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Entities;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class GlobalLocationsPageServiceTests
{
    private Mock<IGlobalLocationCustomTableDataService> _globalLocationCustomTableDataServiceMock;
    private Mock<IHeroService> _heroServiceMock;
    private Mock<ISettingService> _settingServiceMock;

    private GlobalLocationsPageService _service;

    [SetUp]
    public void Setup()
    {
        _globalLocationCustomTableDataServiceMock = new Mock<IGlobalLocationCustomTableDataService>();
        _heroServiceMock = new Mock<IHeroService>();
        _settingServiceMock = new Mock<ISettingService>();

        _heroServiceMock
            .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        _globalLocationCustomTableDataServiceMock
            .Setup(s => s.GetAllGlobalLocationsAsync())
            .ReturnsAsync(new List<GlobalLocation>());

        _service = new GlobalLocationsPageService(
            _globalLocationCustomTableDataServiceMock.Object,
            _heroServiceMock.Object,
            _settingServiceMock.Object);
    }

    private static BDO.GlobalLocations CreatePage(
        string? heroLayout = "banner",
        string? heroTitle = "Global Locations",
        string? heroDescription = "Find our offices",
        string? descriptionTitle = "Our Presence",
        string? descriptionBody = "<p>Body content</p>")
    {
        return new BDO.GlobalLocations
        {
            HeroLayout = heroLayout!,
            HeroTitle = heroTitle!,
            HeroDescription = heroDescription!,
            HeroBackgroundImageAltText = "bg-alt",
            HeroBackgroundImage_MobileAltText = "mobile-alt",
            HeroCallToActionButtonType = "primary",
            HeroCallToActionButtonText = "Learn more",
            DescriptionTitleforGlobal = descriptionTitle!,
            DescriptionBodyforGlobal = descriptionBody!,
        };
    }

    [Test]
    public async Task GetGlobalLocationsPageDtoAsync_ReturnsDto_WithHeroFromHeroService()
    {
        var expectedHero = new HeroDto { Title = "Hero Title" };
        _heroServiceMock
            .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHero);

        var result = await _service.GetGlobalLocationsPageDtoAsync(CreatePage(), CancellationToken.None);

        Assert.That(result.Hero, Is.EqualTo(expectedHero));
    }

    [Test]
    public async Task GetGlobalLocationsPageDtoAsync_PassesHeroDataFromPageToHeroService()
    {
        HeroData? captured = null;
        _heroServiceMock
            .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .Callback<HeroData, CancellationToken>((d, _) => captured = d)
            .ReturnsAsync(new HeroDto());

        var page = CreatePage(heroLayout: "full-width", heroTitle: "T", heroDescription: "D");

        await _service.GetGlobalLocationsPageDtoAsync(page, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.Layout, Is.EqualTo("full-width"));
            Assert.That(captured.Title, Is.EqualTo("T"));
            Assert.That(captured.Description, Is.EqualTo("D"));
            Assert.That(captured.BackgroundImageAltText, Is.EqualTo("bg-alt"));
            Assert.That(captured.MobileBackgroundImageAltText, Is.EqualTo("mobile-alt"));
            Assert.That(captured.CallToActionButtonType, Is.EqualTo("primary"));
            Assert.That(captured.CallToActionButtonText, Is.EqualTo("Learn more"));
            Assert.That(captured.BackgroundImage, Is.Null);
            Assert.That(captured.MobileBackgroundImage, Is.Null);
            Assert.That(captured.CallToActionButtonUrl, Is.Null);
        });
    }

    [Test]
    public async Task GetGlobalLocationsPageDtoAsync_MainGrid_UsesPageTitleAndDescription()
    {
        var page = CreatePage(descriptionTitle: "Presence", descriptionBody: "<p>Hello</p>");

        var result = await _service.GetGlobalLocationsPageDtoAsync(page, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.MainGrid, Is.Not.Null);
            Assert.That(result.MainGrid.Title, Is.EqualTo("Presence"));
            Assert.That(result.MainGrid.Description, Is.EqualTo("<p>Hello</p>"));
        });
    }

    [Test]
    public async Task GetGlobalLocationsPageDtoAsync_MainGrid_MapsRegionCodesToRegionNames()
    {
        _globalLocationCustomTableDataServiceMock
            .Setup(s => s.GetAllGlobalLocationsAsync())
            .ReturnsAsync(new List<GlobalLocation>
            {
                new() { Title = "New York", Region = "1", Url = "/us/ny" },
                new() { Title = "London",   Region = "2", Url = "/uk/london" },
                new() { Title = "Tokyo",    Region = "3", Url = "/jp/tokyo" },
            });

        var result = await _service.GetGlobalLocationsPageDtoAsync(CreatePage(), CancellationToken.None);

        var regions = result.MainGrid.Locations.Select(l => l.Region).ToList();
        Assert.That(regions, Is.EquivalentTo(new[] { "AMERICAS", "EMEA", "ASIA PACIFIC" }));
    }

    [Test]
    public async Task GetGlobalLocationsPageDtoAsync_MainGrid_GroupsLocationsByRegion()
    {
        _globalLocationCustomTableDataServiceMock
            .Setup(s => s.GetAllGlobalLocationsAsync())
            .ReturnsAsync(new List<GlobalLocation>
            {
                new() { Title = "New York",    Region = "1", Url = "/us/ny" },
                new() { Title = "Los Angeles", Region = "1", Url = "/us/la" },
                new() { Title = "London",      Region = "2", Url = "/uk/london" },
            });

        var result = await _service.GetGlobalLocationsPageDtoAsync(CreatePage(), CancellationToken.None);

        var americas = result.MainGrid.Locations.Single(r => r.Region == "AMERICAS");
        var emea = result.MainGrid.Locations.Single(r => r.Region == "EMEA");

        Assert.Multiple(() =>
        {
            Assert.That(americas.RegionalLocations, Has.Count.EqualTo(2));
            Assert.That(emea.RegionalLocations, Has.Count.EqualTo(1));
            Assert.That(emea.RegionalLocations[0].Title, Is.EqualTo("London"));
            Assert.That(emea.RegionalLocations[0].URL, Is.EqualTo("/uk/london"));
        });
    }

    [Test]
    public async Task GetGlobalLocationsPageDtoAsync_MainGrid_OrdersRegionalLocationsByTitle()
    {
        _globalLocationCustomTableDataServiceMock
            .Setup(s => s.GetAllGlobalLocationsAsync())
            .ReturnsAsync(new List<GlobalLocation>
            {
                new() { Title = "New York",    Region = "1", Url = "/us/ny" },
                new() { Title = "Chicago",     Region = "1", Url = "/us/chi" },
                new() { Title = "Los Angeles", Region = "1", Url = "/us/la" },
            });

        var result = await _service.GetGlobalLocationsPageDtoAsync(CreatePage(), CancellationToken.None);

        var titles = result.MainGrid.Locations
            .Single(r => r.Region == "AMERICAS")
            .RegionalLocations
            .Select(l => l.Title)
            .ToList();

        Assert.That(titles, Is.EqualTo(new[] { "Chicago", "Los Angeles", "New York" }));
    }

    [Test]
    public async Task GetGlobalLocationsPageDtoAsync_WhenNoLocations_ReturnsEmptyList()
    {
        var result = await _service.GetGlobalLocationsPageDtoAsync(CreatePage(), CancellationToken.None);

        Assert.That(result.MainGrid.Locations, Is.Empty);
    }

    [Test]
    public async Task GetGlobalLocationsPageDtoAsync_PassesCancellationTokenToHeroService()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        await _service.GetGlobalLocationsPageDtoAsync(CreatePage(), token);

        _heroServiceMock.Verify(
            s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), token),
            Times.Once);
    }

    [Test]
    public void GetGlobalLocationsPageDtoAsync_WhenRegionCodeUnknown_Throws()
    {
        _globalLocationCustomTableDataServiceMock
            .Setup(s => s.GetAllGlobalLocationsAsync())
            .ReturnsAsync(new List<GlobalLocation>
            {
                new() { Title = "Mars", Region = "99", Url = "/mars" },
            });

        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _service.GetGlobalLocationsPageDtoAsync(CreatePage(), CancellationToken.None));
    }
}
