using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class SidebarServiceTests
{
    private readonly Mock<ISocialMediaButtonsService> _socialMediaButtonsServiceMock;
    private readonly Mock<ISettingService> _settingServiceMock;
    private readonly SidebarService _service;

    public SidebarServiceTests()
    {
        _socialMediaButtonsServiceMock = new Mock<ISocialMediaButtonsService>();
        _settingServiceMock = new Mock<ISettingService>();
        _service = new SidebarService(_socialMediaButtonsServiceMock.Object, _settingServiceMock.Object);
    }

    [Test]
    public async Task GetSidebarDtoAsync_WhenNoSettings_ReturnsDefaultSidebar()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync((IEnumerable<PrintorShareInfo>?)null);

        var page = new NewsArticle();

        var result = await _service.GetSidebarDtoAsync(page, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetSidebarDtoAsync_WhenEmptySettings_ReturnsDefaultSidebar()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(new List<PrintorShareInfo>());

        var page = new NewsArticle();

        var result = await _service.GetSidebarDtoAsync(page, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetSidebarDtoAsync_WhenNoSettings_ShowPrintButtonReflectsPageValue()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync((IEnumerable<PrintorShareInfo>?)null);

        var page = new NewsArticle { ShowPrintButton = true };

        var result = await _service.GetSidebarDtoAsync(page, CancellationToken.None);

        Assert.That(result.ShowPrintButton, Is.True);
    }

    [Test]
    public async Task GetSidebarDtoAsync_WhenNoSettings_DownloadButtonAttachmentIsNull()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync((IEnumerable<PrintorShareInfo>?)null);

        var page = new NewsArticle();

        var result = await _service.GetSidebarDtoAsync(page, CancellationToken.None);

        Assert.That(result.DownloadButtonAttachment, Is.Null);
    }

    [Test]
    public async Task GetSidebarDtoAsync_WhenShowSocialMediaButtonsFalse_SocialMediaButtonsIsNull()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync((IEnumerable<PrintorShareInfo>?)null);

        var page = new NewsArticle { ShowSocialMediaButtons = false };

        var result = await _service.GetSidebarDtoAsync(page, CancellationToken.None);

        Assert.That(result.SocialMediaButtons, Is.Null);
    }

    [Test]
    public async Task GetSidebarDtoAsync_WhenShowSocialMediaButtonsTrue_CallsSocialMediaButtonsService()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync((IEnumerable<PrintorShareInfo>?)null);

        var expectedDto = new SocialMediaButtonsDto { ShowCopyLinkButton = true };
        _socialMediaButtonsServiceMock
            .Setup(x => x.GetSocialMediaButtonsDto(It.IsAny<NewsArticle>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var page = new NewsArticle { ShowSocialMediaButtons = true };

        var result = await _service.GetSidebarDtoAsync(page, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.SocialMediaButtons, Is.Not.Null);
            Assert.That(result.SocialMediaButtons!.ShowCopyLinkButton, Is.Null);
        });
    }

    [Test]
    public async Task GetSidebarDtoAsync_WhenShowSocialMediaButtonsTrue_VerifiesServiceCall()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync((IEnumerable<PrintorShareInfo>?)null);

        _socialMediaButtonsServiceMock
            .Setup(x => x.GetSocialMediaButtonsDto(It.IsAny<NewsArticle>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SocialMediaButtonsDto());

        var page = new NewsArticle { ShowSocialMediaButtons = true };

        await _service.GetSidebarDtoAsync(page, CancellationToken.None);

        _socialMediaButtonsServiceMock.Verify(
            x => x.GetSocialMediaButtonsDto(page, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetSidebarDtoAsync_WhenShowSocialMediaButtonsFalse_DoesNotCallSocialMediaButtonsService()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync((IEnumerable<PrintorShareInfo>?)null);

        var page = new NewsArticle { ShowSocialMediaButtons = false };

        await _service.GetSidebarDtoAsync(page, CancellationToken.None);

        _socialMediaButtonsServiceMock.Verify(
            x => x.GetSocialMediaButtonsDto(It.IsAny<NewsArticle>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
