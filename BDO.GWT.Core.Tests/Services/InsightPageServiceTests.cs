using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class InsightPageServiceTests
{
    private Mock<ISettingService> _settingServiceMock = null!;
    private InsightPageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _settingServiceMock = new Mock<ISettingService>();

        _service = new InsightPageService(
            Mock.Of<IContentRetriever>(),
            Mock.Of<IInfoProvider<WebsiteChannelInfo>>(),
            Mock.Of<IContentService>(),
            Mock.Of<IInfoProvider<ChannelInfo>>(),
            Mock.Of<IWebsiteChannelContext>(),
            _settingServiceMock.Object,
            Mock.Of<ISocialMediaButtonsService>());
    }

    [Test]
    public async Task GetDisplayNameAsync_ReturnsComposedNameWithPrefix_WhenSettingsUnavailable()
    {
        _settingServiceMock
            .Setup(x => x.GetUsePersonPageTitleInfo())
            .ReturnsAsync((IEnumerable<OurPeopleInfo>?)null);

        var externalPerson = new ExternalPerson
        {
            MetadataTitle = "Display From Title",
            FirstName = "Ada",
            SurnamePrefixes = "van",
            LastName = "Lovelace"
        };

        var result = await _service.GetDisplayNameAsync(externalPerson);

        Assert.That(result, Is.EqualTo("Ada van Lovelace"));
    }

    [Test]
    public async Task GetDisplayNameAsync_ReturnsComposedNameWithoutPrefix_WhenSettingsUnavailable()
    {
        _settingServiceMock
            .Setup(x => x.GetUsePersonPageTitleInfo())
            .ReturnsAsync((IEnumerable<OurPeopleInfo>?)null);

        var externalPerson = new ExternalPerson
        {
            FirstName = "Ada",
            SurnamePrefixes = string.Empty,
            LastName = "Lovelace"
        };

        var result = await _service.GetDisplayNameAsync(externalPerson);

        Assert.That(result, Is.EqualTo("Ada Lovelace"));
    }
}
