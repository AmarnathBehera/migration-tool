using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Controllers;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

[TestFixture]
public class BlogsControllerTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IBlogsPageService> _blogsPageServiceMock = null!;
    private BlogsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _blogsPageServiceMock = new Mock<IBlogsPageService>();
        _controller = new BlogsController(
            _contentServiceMock.Object,
            _blogsPageServiceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    #region Index

    [Test]
    public async Task Index_WhenCurrentPageIsNull_ReturnsNotFound()
    {
        // Arrange
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<Blog>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Blog?)null);

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Index_WhenCurrentPageIsNull_DoesNotCallBlogsPageService()
    {
        // Arrange
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<Blog>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Blog?)null);

        // Act
        await _controller.Index(CancellationToken.None);

        // Assert
        _blogsPageServiceMock.Verify(
            x => x.GetBlogsPageDtoAsync(It.IsAny<Blog>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Index_WhenCurrentPageExists_ReturnsViewResult()
    {
        // Arrange
        var blog = CreateMockBlog();
        var expectedDto = new BlogPageDto();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<Blog>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(blog);

        _blogsPageServiceMock
            .Setup(x => x.GetBlogsPageDtoAsync(blog, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Index_WhenCurrentPageExists_ReturnsViewWithCorrectModel()
    {
        // Arrange
        var blog = CreateMockBlog();
        var expectedDto = new BlogPageDto { Title = "Test Blog Page" };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<Blog>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(blog);

        _blogsPageServiceMock
            .Setup(x => x.GetBlogsPageDtoAsync(blog, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        var viewResult = result as ViewResult;
        Assert.Multiple(() =>
        {
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.EqualTo(expectedDto));
        });
    }

    [Test]
    public async Task Index_PassesLinkedItemsMaxLevel4_ToContentService()
    {
        // Arrange
        var blog = CreateMockBlog();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<Blog>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(blog);

        _blogsPageServiceMock
            .Setup(x => x.GetBlogsPageDtoAsync(blog, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogPageDto());

        // Act
        await _controller.Index(CancellationToken.None);

        // Assert
        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<Blog>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Index_WhenCurrentPageExists_CallsGetBlogsPageDtoAsync()
    {
        // Arrange
        var blog = CreateMockBlog();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<Blog>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(blog);

        _blogsPageServiceMock
            .Setup(x => x.GetBlogsPageDtoAsync(blog, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogPageDto());

        // Act
        await _controller.Index(CancellationToken.None);

        // Assert
        _blogsPageServiceMock.Verify(
            x => x.GetBlogsPageDtoAsync(blog, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Index_PassesCorrectCancellationToken()
    {
        // Arrange
        var blog = CreateMockBlog();
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<Blog>(
                It.IsAny<RetrieveCurrentPageParameters>(), token))
            .ReturnsAsync(blog);

        _blogsPageServiceMock
            .Setup(x => x.GetBlogsPageDtoAsync(blog, token))
            .ReturnsAsync(new BlogPageDto());

        // Act
        await _controller.Index(token);

        // Assert
        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<Blog>(
                It.IsAny<RetrieveCurrentPageParameters>(), token),
            Times.Once);
        _blogsPageServiceMock.Verify(
            x => x.GetBlogsPageDtoAsync(blog, token),
            Times.Once);
    }

    [Test]
    public async Task Index_WhenServiceReturnsNull_ReturnsViewWithNullModel()
    {
        // Arrange
        var blog = CreateMockBlog();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<Blog>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(blog);

        _blogsPageServiceMock
            .Setup(x => x.GetBlogsPageDtoAsync(blog, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BlogPageDto?)null);

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        var viewResult = result as ViewResult;
        Assert.Multiple(() =>
        {
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.Null);
        });
    }

    #endregion

    private static Blog CreateMockBlog()
    {
        return new Blog
        {
            SystemFields = new WebPageFields
            {
                WebPageItemID = 1,
                ContentItemGUID = Guid.NewGuid()
            },
            HeroLayout = "Default",
            HeroTitle = "Test Hero Title",
            HeroDescription = "Test Description",
            HeroBackgroundImageAltText = "alt-bg",
            HeroBackgroundImage_MobileAltText = "alt-mobile",
            HeroCallToActionButtonType = "primary",
            HeroCallToActionButtonText = "CTA Text",
            HeroBackgroundImage = Array.Empty<ContentAttachment>(),
            HeroBackgroundImage_Mobile = Array.Empty<ContentAttachment>(),
            HeroCallToActionButtonUrl = Array.Empty<IWebPageFieldsSource>(),
            ItemListAdvancedSearchLinkUrl = Array.Empty<WebPageRelatedItem>(),
            BlogPostListFeaturedItems = Array.Empty<WebPageRelatedItem>(),
            ItemListFilteringEnabled = false,
            BlogPostListFilters = null,
            ItemListTitle = "List Title",
            ItemListDescription = "List Desc",
            ItemListAdvancedSearchDescription = "Search Desc",
            ItemListAdvancedSearchLinkText = "Search Link"
        };
    }
}