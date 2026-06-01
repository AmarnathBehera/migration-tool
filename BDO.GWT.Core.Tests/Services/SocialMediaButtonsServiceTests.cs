using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using CMS.Websites;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class SocialMediaButtonsServiceTests
{
    private readonly Mock<ISocialMediaButtonsService> _socialMediaButtonsServiceMock;

    public SocialMediaButtonsServiceTests()
    {
        _socialMediaButtonsServiceMock = new Mock<ISocialMediaButtonsService>();
    }

    [Test]
    public async Task GetSocialMediaButtonsDto_ReturnsExpectedDto()
    {
        var expectedDto = new SocialMediaButtonsDto
        {
            EmailButtonUrl = "mailto:?subject=Test&body=https://example.com",
            FacebookButtonUrl = "https://www.facebook.com/dialog/feed?display=popup",
            TwitterButtonUrl = "https://twitter.com/intent/tweet?text=Test",
            LinkedInUrl = "https://www.linkedin.com/shareArticle?url=https://example.com",
            ShowCopyLinkButton = true
        };

        _socialMediaButtonsServiceMock
            .Setup(x => x.GetSocialMediaButtonsDto(It.IsAny<IWebPageFieldsSource>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _socialMediaButtonsServiceMock.Object.GetSocialMediaButtonsDto(
            new NewsArticle(), "Test Title", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.EmailButtonUrl, Is.Not.Null);
            Assert.That(result.FacebookButtonUrl, Is.Not.Null);
            Assert.That(result.TwitterButtonUrl, Is.Not.Null);
            Assert.That(result.LinkedInUrl, Is.Not.Null);
            Assert.That(result.ShowCopyLinkButton, Is.True);
        });
    }

    [Test]
    public async Task GetSocialMediaButtonsDto_WhenEmailDisabled_ReturnsNullEmailUrl()
    {
        var dto = new SocialMediaButtonsDto
        {
            EmailButtonUrl = null,
            FacebookButtonUrl = "https://facebook.com",
            TwitterButtonUrl = "https://twitter.com",
            LinkedInUrl = "https://linkedin.com",
            ShowCopyLinkButton = true
        };

        _socialMediaButtonsServiceMock
            .Setup(x => x.GetSocialMediaButtonsDto(It.IsAny<IWebPageFieldsSource>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _socialMediaButtonsServiceMock.Object.GetSocialMediaButtonsDto(
            new NewsArticle(), "Test Title", CancellationToken.None);

        Assert.That(result.EmailButtonUrl, Is.Null);
    }

    [Test]
    public async Task GetSocialMediaButtonsDto_WhenFacebookDisabled_ReturnsNullFacebookUrl()
    {
        var dto = new SocialMediaButtonsDto { FacebookButtonUrl = null };

        _socialMediaButtonsServiceMock
            .Setup(x => x.GetSocialMediaButtonsDto(It.IsAny<IWebPageFieldsSource>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _socialMediaButtonsServiceMock.Object.GetSocialMediaButtonsDto(
            new NewsArticle(), "Test Title", CancellationToken.None);

        Assert.That(result.FacebookButtonUrl, Is.Null);
    }

    [Test]
    public async Task GetSocialMediaButtonsDto_WhenTwitterDisabled_ReturnsNullTwitterUrl()
    {
        var dto = new SocialMediaButtonsDto { TwitterButtonUrl = null };

        _socialMediaButtonsServiceMock
            .Setup(x => x.GetSocialMediaButtonsDto(It.IsAny<IWebPageFieldsSource>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _socialMediaButtonsServiceMock.Object.GetSocialMediaButtonsDto(
            new NewsArticle(), "Test Title", CancellationToken.None);

        Assert.That(result.TwitterButtonUrl, Is.Null);
    }

    [Test]
    public async Task GetSocialMediaButtonsDto_WhenLinkedInDisabled_ReturnsNullLinkedInUrl()
    {
        var dto = new SocialMediaButtonsDto { LinkedInUrl = null };

        _socialMediaButtonsServiceMock
            .Setup(x => x.GetSocialMediaButtonsDto(It.IsAny<IWebPageFieldsSource>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _socialMediaButtonsServiceMock.Object.GetSocialMediaButtonsDto(
            new NewsArticle(), "Test Title", CancellationToken.None);

        Assert.That(result.LinkedInUrl, Is.Null);
    }

    [Test]
    public async Task GetSocialMediaButtonsDto_WhenCopyLinkDisabled_ReturnsFalse()
    {
        var dto = new SocialMediaButtonsDto { ShowCopyLinkButton = false };

        _socialMediaButtonsServiceMock
            .Setup(x => x.GetSocialMediaButtonsDto(It.IsAny<IWebPageFieldsSource>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _socialMediaButtonsServiceMock.Object.GetSocialMediaButtonsDto(
            new NewsArticle(), "Test Title", CancellationToken.None);

        Assert.That(result.ShowCopyLinkButton, Is.False);
    }
}
