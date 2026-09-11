using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

[TestFixture]
public class BlogPostControllerTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IBlogPostPageService> _blogPostPageServiceMock = null!;
    private BlogPostController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _blogPostPageServiceMock = new Mock<IBlogPostPageService>();
        _controller = new BlogPostController(
            _blogPostPageServiceMock.Object,
            _contentServiceMock.Object);
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
            .Setup(x => x.RetrieveCurrentPageAsync<BlogPost>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BlogPost?)null);

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Index_WhenCurrentPageIsNull_DoesNotCallBlogPostPageService()
    {
        // Arrange
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BlogPost>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BlogPost?)null);

        // Act
        await _controller.Index(CancellationToken.None);

        // Assert
        _blogPostPageServiceMock.Verify(
            x => x.GetBlogPostDtoAsync(It.IsAny<BlogPost>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Index_WhenCurrentPageExists_ReturnsViewResult()
    {
        // Arrange
        var blogPost = CreateMockBlogPost();
        var expectedDto = new BlogPostDto();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BlogPost>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(blogPost);

        _blogPostPageServiceMock
            .Setup(x => x.GetBlogPostDtoAsync(blogPost, It.IsAny<CancellationToken>()))
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
        var blogPost = CreateMockBlogPost();
        var expectedDto = new BlogPostDto { Title = "Test Blog Post" };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BlogPost>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(blogPost);

        _blogPostPageServiceMock
            .Setup(x => x.GetBlogPostDtoAsync(blogPost, It.IsAny<CancellationToken>()))
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
    public async Task Index_WhenCurrentPageExists_ReturnsCorrectViewPath()
    {
        // Arrange
        var blogPost = CreateMockBlogPost();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BlogPost>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(blogPost);

        _blogPostPageServiceMock
            .Setup(x => x.GetBlogPostDtoAsync(blogPost, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogPostDto());

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("~/Views/BlogPost/Index.cshtml"));
    }

    [Test]
    public async Task Index_PassesLinkedItemsMaxLevel4_ToContentService()
    {
        // Arrange
        var blogPost = CreateMockBlogPost();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BlogPost>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(blogPost);

        _blogPostPageServiceMock
            .Setup(x => x.GetBlogPostDtoAsync(blogPost, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogPostDto());

        // Act
        await _controller.Index(CancellationToken.None);

        // Assert
        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<BlogPost>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Index_WhenCurrentPageExists_CallsGetBlogPostDtoAsync()
    {
        // Arrange
        var blogPost = CreateMockBlogPost();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BlogPost>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(blogPost);

        _blogPostPageServiceMock
            .Setup(x => x.GetBlogPostDtoAsync(blogPost, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogPostDto());

        // Act
        await _controller.Index(CancellationToken.None);

        // Assert
        _blogPostPageServiceMock.Verify(
            x => x.GetBlogPostDtoAsync(blogPost, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Index_PassesCorrectCancellationToken()
    {
        // Arrange
        var blogPost = CreateMockBlogPost();
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BlogPost>(
                It.IsAny<RetrieveCurrentPageParameters>(), token))
            .ReturnsAsync(blogPost);

        _blogPostPageServiceMock
            .Setup(x => x.GetBlogPostDtoAsync(blogPost, token))
            .ReturnsAsync(new BlogPostDto());

        // Act
        await _controller.Index(token);

        // Assert
        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<BlogPost>(
                It.IsAny<RetrieveCurrentPageParameters>(), token),
            Times.Once);
        _blogPostPageServiceMock.Verify(
            x => x.GetBlogPostDtoAsync(blogPost, token),
            Times.Once);
    }

    [Test]
    public async Task Index_WhenServiceReturnsNull_ReturnsViewWithNullModel()
    {
        // Arrange
        var blogPost = CreateMockBlogPost();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BlogPost>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(blogPost);

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        _blogPostPageServiceMock
            .Setup(x => x.GetBlogPostDtoAsync(blogPost, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as BlogPostDto);
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

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

    private static BlogPost CreateMockBlogPost()
    {
        return new BlogPost
        {
            SystemFields = new WebPageFields
            {
                WebPageItemID = 1,
                ContentItemGUID = Guid.NewGuid()
            }
        };
    }
}