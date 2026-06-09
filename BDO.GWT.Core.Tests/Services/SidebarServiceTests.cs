using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.LiveSite.DTOs;
using CMS.ContentEngine;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class SidebarServiceTests
{
    private readonly Mock<ISocialMediaButtonsService> _socialMediaButtonsServiceMock;
    private readonly Mock<ISettingService> _settingServiceMock;
    private readonly Mock<IContentService> _contentServiceMock;
    private readonly SidebarService _service;

    public SidebarServiceTests()
    {
        _socialMediaButtonsServiceMock = new Mock<ISocialMediaButtonsService>();
        _settingServiceMock = new Mock<ISettingService>();
        _contentServiceMock = new Mock<IContentService>();
        _service = new SidebarService(_socialMediaButtonsServiceMock.Object, _settingServiceMock.Object, _contentServiceMock.Object);
    }

    [Test]
    public async Task GetNewsSidebarDtoAsync_WhenNoSettings_ReturnsDefaultSidebar()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync((IEnumerable<PrintorShareInfo>?)null);

        var page = new NewsArticle();

        var result = await _service.GetSidebarDtoAsync(page, "/news", CancellationToken.None);

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetNewsSidebarDtoAsync_WhenEmptySettings_ReturnsDefaultSidebar()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(new List<PrintorShareInfo>());

        var page = new NewsArticle();

        var result = await _service.GetSidebarDtoAsync(page, "/news", CancellationToken.None);

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetNewsSidebarDtoAsync_WhenNoSettings_ShowPrintButtonReflectsPageValue()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync((IEnumerable<PrintorShareInfo>?)null);

        var page = new NewsArticle { ShowPrintButton = true };

        var result = await _service.GetSidebarDtoAsync(page, "/news", CancellationToken.None);

        Assert.That(result.ShowPrintButton, Is.True);
    }

    [Test]
    public async Task GetNewsSidebarDtoAsync_WhenNoSettings_DownloadButtonAttachmentIsNull()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync((IEnumerable<PrintorShareInfo>?)null);

        var page = new NewsArticle();

        var result = await _service.GetSidebarDtoAsync(page, "/news", CancellationToken.None);

        Assert.That(result.DownloadButtonAttachment, Is.Null);
    }

    [Test]
    public async Task GetNewsSidebarDtoAsync_WhenShowSocialMediaButtonsFalse_SocialMediaButtonsIsNull()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync((IEnumerable<PrintorShareInfo>?)null);

        var page = new NewsArticle { ShowSocialMediaButtons = false };

        var result = await _service.GetSidebarDtoAsync(page, "/news", CancellationToken.None);

        Assert.That(result.SocialMediaButtons, Is.Null);
    }

    [Test]
    public async Task GetNewsSidebarDtoAsync_WhenShowSocialMediaButtonsTrue_CallsSocialMediaButtonsService()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync((IEnumerable<PrintorShareInfo>?)null);

        var expectedDto = new SocialMediaButtonsDto { ShowCopyLinkButton = true };
        _socialMediaButtonsServiceMock
            .Setup(x => x.GetSocialMediaButtonsDtoNB(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var page = new NewsArticle { ShowSocialMediaButtons = true };

        var result = await _service.GetSidebarDtoAsync(page, "/news", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.SocialMediaButtons, Is.Not.Null);
            Assert.That(result.SocialMediaButtons!.ShowCopyLinkButton, Is.Null);
        });
    }

    [Test]
    public async Task GetNewsSidebarDtoAsync_WhenShowSocialMediaButtonsTrue_VerifiesServiceCall()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync((IEnumerable<PrintorShareInfo>?)null);

        _socialMediaButtonsServiceMock
            .Setup(x => x.GetSocialMediaButtonsDtoNB(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SocialMediaButtonsDto());

        var page = new NewsArticle { ShowSocialMediaButtons = true };

        await _service.GetSidebarDtoAsync(page, "/news", CancellationToken.None);

        _socialMediaButtonsServiceMock.Verify(
            x => x.GetSocialMediaButtonsDtoNB("/news", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetNewsSidebarDtoAsync_WhenShowSocialMediaButtonsFalse_DoesNotCallSocialMediaButtonsService()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync((IEnumerable<PrintorShareInfo>?)null);

        var page = new NewsArticle { ShowSocialMediaButtons = false };

        await _service.GetSidebarDtoAsync(page, "/news", CancellationToken.None);

        _socialMediaButtonsServiceMock.Verify(
            x => x.GetSocialMediaButtonsDtoNB(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetSidebarDtoAsync_WhenDisablePrintTrue_ShowPrintButtonIsFalse()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(new List<PrintorShareInfo> { new PrintorShareInfo { DisablePrint = true } });

        var page = new NewsArticle { ShowPrintButton = true };

        var result = await _service.GetSidebarDtoAsync(page, "/news", CancellationToken.None);

        Assert.That(result.ShowPrintButton, Is.False);
    }

    [Test]
    public async Task GetSidebarDtoAsync_WhenDisableCopyLinkButtonTrue_ShowCopyLinkButtonIsFalse()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(new List<PrintorShareInfo> { new PrintorShareInfo { DisableCopyLinkButton = true } });

        var page = new NewsArticle();

        var result = await _service.GetSidebarDtoAsync(page, "/news", CancellationToken.None);

        Assert.That(result.ShowCopyLinkButton, Is.False);
    }

    [Test]
    public async Task GetSidebarDtoAsync_WhenDisableCopyLinkButtonFalse_ShowCopyLinkButtonIsTrue()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(new List<PrintorShareInfo> { new PrintorShareInfo { DisableCopyLinkButton = false } });

        var page = new NewsArticle();

        var result = await _service.GetSidebarDtoAsync(page, "/news", CancellationToken.None);

        Assert.That(result.ShowCopyLinkButton, Is.True);
    }

    [Test]
    public async Task GetContentPageSidebarDetails_SetsPublishAndUpdatedDates()
    {
        var sidebarDto = new ContentSidebarDto();
        var publishedDate = new DateTime(2025, 1, 15);
        var updatedDate = new DateTime(2025, 6, 1);

        var result = await _service.GetContentPageSidebarDetails(sidebarDto, CancellationToken.None,
            relevantPersonGuidList: null, publishedDate: publishedDate, updatedDate: updatedDate);

        Assert.Multiple(() =>
        {
            Assert.That(result.PublishDate, Is.EqualTo(publishedDate));
            Assert.That(result.UpdatedDate, Is.EqualTo(updatedDate));
        });
    }

    [Test]
    public async Task GetContentPageSidebarDetails_SetsRelevantContactPeopleTitle()
    {
        var sidebarDto = new ContentSidebarDto();

        var result = await _service.GetContentPageSidebarDetails(sidebarDto, CancellationToken.None);

        Assert.That(result.RelevantContactPeopleTitle, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task GetContentPageSidebarDetails_WhenNoPersonGuids_DoesNotCallContentService()
    {
        var sidebarDto = new ContentSidebarDto();

        await _service.GetContentPageSidebarDetails(sidebarDto, CancellationToken.None, relevantPersonGuidList: null);

        _contentServiceMock.Verify(
            x => x.GetPagesAsync<Person>(It.IsAny<RetrievePagesParameters>(), It.IsAny<Func<ContentItemQueryBuilder, ContentItemQueryBuilder>>(),
                It.IsAny<RetrievalCacheSettings>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetContentPageSidebarDetails_WhenEmptyPersonGuids_DoesNotCallContentService()
    {
        var sidebarDto = new ContentSidebarDto();

        await _service.GetContentPageSidebarDetails(sidebarDto, CancellationToken.None, relevantPersonGuidList: new List<Guid>());

        _contentServiceMock.Verify(
            x => x.GetPagesAsync<Person>(It.IsAny<RetrievePagesParameters>(), It.IsAny<Func<ContentItemQueryBuilder, ContentItemQueryBuilder>>(),
                It.IsAny<RetrievalCacheSettings>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
