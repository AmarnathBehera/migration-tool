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
    public async Task GetContentPageSidebarDto_ReturnsExpectedSidebar()
    {
        var expectedSidebar = new ContentSidebarDto
        {
            TypeName = "Article",
            RelevantContactPeopleTitle = "Authors",
            ShowCopyLinkButton = true
        };

        _newsArticleServiceMock
            .Setup(x => x.GetContentPageSidebarDto(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSidebar);

        var result = await _newsArticleServiceMock.Object.GetContentPageSidebarDto(
            new NewsArticle(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.TypeName, Is.EqualTo("Article"));
            Assert.That(result.RelevantContactPeopleTitle, Is.EqualTo("Authors"));
            Assert.That(result.ShowCopyLinkButton, Is.True);
        });
    }

    [Test]
    public async Task GetNewsPageDtoAsync_InvokedOnce()
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
    public async Task GetContentPageSidebarDto_InvokedOnce()
    {
        _newsArticleServiceMock
            .Setup(x => x.GetContentPageSidebarDto(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContentSidebarDto { RelevantContactPeopleTitle = "Authors" });

        var page = new NewsArticle();
        await _newsArticleServiceMock.Object.GetContentPageSidebarDto(page, CancellationToken.None);

        _newsArticleServiceMock.Verify(
            x => x.GetContentPageSidebarDto(page, It.IsAny<CancellationToken>()), Times.Once);
    }
}
