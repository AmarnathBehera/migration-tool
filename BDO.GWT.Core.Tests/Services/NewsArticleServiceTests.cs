using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class NewsArticleServiceTests
{
    private readonly Mock<INewsArticleService> _newsArticleServiceMock;

    public NewsArticleServiceTests()
    {
        _newsArticleServiceMock = new Mock<INewsArticleService>();
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_ReturnsExpectedDto()
    {
        var expectedDto = new SectionNewsPageDto
        {
            Title = "News Title",
            Description = "Description",
            Content = "Content body",
            Sidebar = new ContentSidebarDto
            {
                RelevantContactPeopleTitle = "Authors"
            },
            CallToAction = new CallToActionDto()
        };

        _newsArticleServiceMock
            .Setup(x => x.GetSectionNewsPageDtoAsync(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _newsArticleServiceMock.Object.GetSectionNewsPageDtoAsync(
            new NewsArticle(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo("News Title"));
            Assert.That(result.Description, Is.EqualTo("Description"));
            Assert.That(result.Content, Is.EqualTo("Content body"));
            Assert.That(result.Sidebar, Is.Not.Null);
            Assert.That(result.CallToAction, Is.Not.Null);
        });
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_WhenSidebarNotShown_ReturnsNullSidebar()
    {
        var dto = new SectionNewsPageDto { Sidebar = null };

        _newsArticleServiceMock
            .Setup(x => x.GetSectionNewsPageDtoAsync(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _newsArticleServiceMock.Object.GetSectionNewsPageDtoAsync(
            new NewsArticle(), CancellationToken.None);

        Assert.That(result.Sidebar, Is.Null);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_WhenCallToActionNotShown_ReturnsNullCallToAction()
    {
        var dto = new SectionNewsPageDto { CallToAction = null };

        _newsArticleServiceMock
            .Setup(x => x.GetSectionNewsPageDtoAsync(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _newsArticleServiceMock.Object.GetSectionNewsPageDtoAsync(
            new NewsArticle(), CancellationToken.None);

        Assert.That(result.CallToAction, Is.Null);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_WhenPageIsNull_ReturnsNull()
    {
        _newsArticleServiceMock
            .Setup(x => x.GetSectionNewsPageDtoAsync(null!, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SectionNewsPageDto?)null);

        var result = await _newsArticleServiceMock.Object.GetSectionNewsPageDtoAsync(
            null!, CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_InvokedOnce()
    {
        _newsArticleServiceMock
            .Setup(x => x.GetSectionNewsPageDtoAsync(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SectionNewsPageDto());

        var page = new NewsArticle();
        await _newsArticleServiceMock.Object.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        _newsArticleServiceMock.Verify(
            x => x.GetSectionNewsPageDtoAsync(page, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_WhenSidebarShown_ShowCopyLinkButtonIsSet()
    {
        var expectedDto = new SectionNewsPageDto
        {
            Sidebar = new ContentSidebarDto
            {
                ShowCopyLinkButton = true
            }
        };

        _newsArticleServiceMock
            .Setup(x => x.GetSectionNewsPageDtoAsync(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _newsArticleServiceMock.Object.GetSectionNewsPageDtoAsync(
            new NewsArticle(), CancellationToken.None);

        Assert.That(result.Sidebar!.ShowCopyLinkButton, Is.True);
    }
}
