using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Helper;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Services.Widget;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services.Widget;

public class ViewGlobalLocationsServiceTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IWebPageUrlResolver> _webPageUrlResolverMock = null!;
    private Mock<ILocalizationHelper> _localizationHelperMock = null!;
    private Mock<ICultureContext> _cultureContextMock = null!;
    private Mock<ISettingService> _settingServiceMock = null!;

    private ViewGlobalLocationsService _service = null!;

    private const string CultureTag = "en-US";
    private const string TitleTranslation = "Global locations title";
    private const string DescriptionTranslation = "Global locations description";
    private const string ButtonTranslation = "View global locations";

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _webPageUrlResolverMock = new Mock<IWebPageUrlResolver>();
        _localizationHelperMock = new Mock<ILocalizationHelper>();
        _cultureContextMock = new Mock<ICultureContext>();
        _settingServiceMock = new Mock<ISettingService>();

        _cultureContextMock.Setup(c => c.CurrentLanguageTag).Returns(CultureTag);

        SetupEmptyPagesAsync<GlobalLocations>();

        _localizationHelperMock
            .Setup(x => x.GetTranslationAsync(CultureTag, ResourceStringKey.LiveSite.ViewGlobalLocations.Title, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TitleTranslation);
        _localizationHelperMock
            .Setup(x => x.GetTranslationAsync(CultureTag, ResourceStringKey.LiveSite.ViewGlobalLocations.Description, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DescriptionTranslation);
        _localizationHelperMock
            .Setup(x => x.GetTranslationAsync(CultureTag, ResourceStringKey.LiveSite.ViewGlobalLocations.Button, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ButtonTranslation);

        _settingServiceMock.Setup(x => x.GetGlobalSettingAsync()).ReturnsAsync((GlobalsettingskeyInfo?)null);

        _service = new ViewGlobalLocationsService(
            _contentServiceMock.Object,
            _webPageUrlResolverMock.Object,
            _localizationHelperMock.Object,
            _cultureContextMock.Object,
            _settingServiceMock.Object);
    }

    // -- Helpers --------------------------------------------------------------

    private void SetupEmptyPagesAsync<TPage>() where TPage : class, IWebPageFieldsSource
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<TPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<TPage>());
    }

    private void SetupPagesAsync(params GlobalLocations[] pages)
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<GlobalLocations>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pages);
    }

    private static ViewGlobalLocationsData CreateData() => new()
    {
        Theme = "dark",
        Layout = "left",
        TopMargin = "40px",
        BottomMargin = "20px",
        ButtonTarget = "_self",
        WidgetID = "view-global-locations-1",
    };

    // -- Scalar mapping

    [Test]
    public async Task GetGlobalLocationsBannerAsync_MapsScalarDataFieldsToDto()
    {
        var data = CreateData();

        var result = await _service.GetGlobalLocationsBannerAsync(data, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Theme, Is.EqualTo("dark"));
            Assert.That(result.Layout, Is.EqualTo("left"));
            Assert.That(result.TopMargin, Is.EqualTo("40px"));
            Assert.That(result.BottomMargin, Is.EqualTo("20px"));
            Assert.That(result.WidgetID, Is.EqualTo("view-global-locations-1"));
        });
    }

    [Test]
    public async Task GetGlobalLocationsBannerAsync_UsesLocalizedTitleDescriptionAndButton()
    {
        var data = CreateData();

        var result = await _service.GetGlobalLocationsBannerAsync(data, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo(TitleTranslation));
            Assert.That(result.Description, Is.EqualTo(DescriptionTranslation));
            Assert.That(result.Button, Is.Not.Null);
            Assert.That(result.Button.Title, Is.EqualTo(ButtonTranslation));
        });
    }

    [Test]
    public async Task GetGlobalLocationsBannerAsync_SetsButtonTargetAndSecondaryType()
    {
        var data = CreateData();
        data.ButtonTarget = "_blank";

        var result = await _service.GetGlobalLocationsBannerAsync(data, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Button.Target, Is.EqualTo("_blank"));
            Assert.That(result.Button.Type, Is.EqualTo("secondary"));
        });
    }

    // -- Button URL resolution ------------------------------------------------

    [Test]
    public async Task GetGlobalLocationsBannerAsync_UsesDataButtonUrl_WhenProvided()
    {
        var data = CreateData();
        data.ButtonURL = "/custom-locations";

        var result = await _service.GetGlobalLocationsBannerAsync(data, CancellationToken.None);

        Assert.That(result.Button.URL, Is.EqualTo("/custom-locations"));
        _webPageUrlResolverMock.Verify(x => x.GetRelativePath(It.IsAny<IWebPageFieldsSource>()), Times.Never);
    }

    [Test]
    public async Task GetGlobalLocationsBannerAsync_FallsBackToResolvedPageUrl_WhenButtonUrlEmpty()
    {
        var data = CreateData();
        data.ButtonURL = "   ";

        var globalLocations = new GlobalLocations();
        SetupPagesAsync(globalLocations);
        _webPageUrlResolverMock
            .Setup(x => x.GetRelativePath(globalLocations))
            .Returns("/global-locations");

        var result = await _service.GetGlobalLocationsBannerAsync(data, CancellationToken.None);

        Assert.That(result.Button.URL, Is.EqualTo("/global-locations"));
    }

    [Test]
    public async Task GetGlobalLocationsBannerAsync_ButtonUrlEmpty_WhenNoGlobalLocationsPageFound()
    {
        var data = CreateData();
        data.ButtonURL = null;

        var result = await _service.GetGlobalLocationsBannerAsync(data, CancellationToken.None);

        Assert.That(result.Button.URL, Is.Empty);
        _webPageUrlResolverMock.Verify(x => x.GetRelativePath(It.IsAny<IWebPageFieldsSource>()), Times.Never);
    }

    // -- Background image -----------------------------------------------------

    [Test]
    public async Task GetGlobalLocationsBannerAsync_ReturnsEmptyBackgroundImage_WhenNoGlobalSetting()
    {
        var data = CreateData();

        var result = await _service.GetGlobalLocationsBannerAsync(data, CancellationToken.None);

        Assert.That(result.BackgroundImage, Is.Empty);
    }
}
