using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Services;

public class SectionBlogServiceTests
{
    private Mock<IHeroService> _heroServiceMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IPageUtility> _pageUtilityMock = null!;
    private SectionBlogService _service = null!;

    [SetUp]
    public void Setup()
    {
        _heroServiceMock = new Mock<IHeroService>();
        _contentServiceMock = new Mock<IContentService>();
        _pageUtilityMock = new Mock<IPageUtility>();
        _service = new SectionBlogService(
            _heroServiceMock.Object,
            _contentServiceMock.Object,
            _pageUtilityMock.Object);
    }

    [Test]
    public async Task GetSectionBlogsDtoAsync_WhenPageIsNull_ReturnsEmptyDto()
    {
        // Arrange
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBlogs>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SectionBlogs)null!);

        // Act
        var result = await _service.GetSectionBlogsDtoAsync(CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Hero, Is.Null);
        Assert.That(result.MultiLinksCardsGrid, Is.Null);
    }

    [Test]
    public async Task GetSectionBlogsDtoAsync_WhenPageIsNull_DoesNotCallHeroService()
    {
        // Arrange
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBlogs>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SectionBlogs)null!);

        // Act
        await _service.GetSectionBlogsDtoAsync(CancellationToken.None);

        // Assert
        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetSectionBlogsDtoAsync_WhenPageIsNull_DoesNotCallPageUtility()
    {
        // Arrange
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBlogs>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SectionBlogs)null!);

        // Act
        await _service.GetSectionBlogsDtoAsync(CancellationToken.None);

        // Assert
        _pageUtilityMock.Verify(
            x => x.GetMultiLinkCardsAsync2<Blog, BlogContributors, Contact>(
                It.IsAny<int>(),
                It.IsAny<System.Func<Blog, string>>(),
                It.IsAny<System.Func<Blog, string>>(),
                It.IsAny<System.Func<Blog, string>>(),
                It.IsAny<System.Func<Blog, string>>(),
                It.IsAny<System.Func<BlogContributors, string>>(),
                It.IsAny<System.Func<Contact, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetSectionBlogsDtoAsync_WhenPageExists_ReturnsHeroDto()
    {
        // Arrange
        var page = CreateMockSectionBlogsPage();
        var expectedHeroDto = new HeroDto { Title = "Test Hero" };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBlogs>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _pageUtilityMock
            .Setup(x => x.GetMultiLinkCardsAsync2<Blog, BlogContributors, Contact>(
                It.IsAny<int>(),
                It.IsAny<System.Func<Blog, string>>(),
                It.IsAny<System.Func<Blog, string>>(),
                It.IsAny<System.Func<Blog, string>>(),  
                It.IsAny<System.Func<Blog, string>>(),
                It.IsAny<System.Func<BlogContributors, string>>(),
                It.IsAny<System.Func<Contact, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MultiLinkCardDto>());

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHeroDto);

        // Act
        var result = await _service.GetSectionBlogsDtoAsync(CancellationToken.None);

        // Assert
        Assert.That(result.Hero, Is.EqualTo(expectedHeroDto));
    }

    [Test]
    public async Task GetSectionBlogsDtoAsync_WhenPageExists_ReturnsMultiLinksCardsGrid()
    {
        // Arrange
        var page = CreateMockSectionBlogsPage();
        var expectedCards = new List<MultiLinkCardDto>
        {
            new MultiLinkCardDto { Title = "Blog 1" },
            new MultiLinkCardDto { Title = "Blog 2" }
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBlogs>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _pageUtilityMock
            .Setup(x => x.GetMultiLinkCardsAsync2<Blog, BlogContributors, Contact>(
                It.IsAny<int>(),
                It.IsAny<System.Func<Blog, string>>(),
                It.IsAny<System.Func<Blog, string>>(),
                It.IsAny<System.Func<Blog, string>>(),
                It.IsAny<System.Func<Blog, string>>(),
                It.IsAny<System.Func<BlogContributors, string>>(),
                It.IsAny<System.Func<Contact, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCards.ToArray());

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        // Act
        var result = await _service.GetSectionBlogsDtoAsync(CancellationToken.None);

        // Assert
        Assert.That(result.MultiLinksCardsGrid, Is.Not.Null);
        Assert.That(result.MultiLinksCardsGrid.Cards, Is.EqualTo(expectedCards));
        Assert.That(result.MultiLinksCardsGrid.Title, Is.EqualTo("Test Page Title"));
        Assert.That(result.MultiLinksCardsGrid.Description, Is.EqualTo("Test Page Description"));
    }

    [Test]
    public async Task GetSectionBlogsDtoAsync_WhenPageExists_PassesCorrectLinkedItemsMaxLevel()
    {
        // Arrange
        var page = CreateMockSectionBlogsPage();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBlogs>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _pageUtilityMock
            .Setup(x => x.GetMultiLinkCardsAsync2<Blog, BlogContributors, Contact>(
                It.IsAny<int>(),
                It.IsAny<System.Func<Blog, string>>(),
                It.IsAny<System.Func<Blog, string>>(),
                It.IsAny<System.Func<Blog, string>>(),
                It.IsAny<System.Func<Blog, string>>(),
                It.IsAny<System.Func<BlogContributors, string>>(),
                It.IsAny<System.Func<Contact, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MultiLinkCardDto>());

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        // Act
        await _service.GetSectionBlogsDtoAsync(CancellationToken.None);

        // Assert
        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<SectionBlogs>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSectionBlogsDtoAsync_WhenPageExists_PassesHeroDataToHeroService()
    {
        // Arrange
        var page = CreateMockSectionBlogsPage();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBlogs>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _pageUtilityMock
            .Setup(x => x.GetMultiLinkCardsAsync2<Blog, BlogContributors, Contact>(
                It.IsAny<int>(),
                It.IsAny<System.Func<Blog, string>>(),
                It.IsAny<System.Func<Blog, string>>(),
                It.IsAny<System.Func<Blog, string>>(),
                It.IsAny<System.Func<Blog, string>>(),
                It.IsAny<System.Func<BlogContributors, string>>(),
                It.IsAny<System.Func<Contact, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MultiLinkCardDto>());

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.Is<HeroData>(h =>
                    h.Title == "Hero Title" &&
                    h.Description == "Hero Description"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        // Act
        await _service.GetSectionBlogsDtoAsync(CancellationToken.None);

        // Assert
        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(
                It.Is<HeroData>(h =>
                    h.Title == "Hero Title" &&
                    h.Description == "Hero Description"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static SectionBlogs CreateMockSectionBlogsPage()
    {
        var page = new Mock<SectionBlogs>();
        page.Setup(x => x.HeroTitle).Returns("Hero Title");
        page.Setup(x => x.HeroDescription).Returns("Hero Description");
        page.Setup(x => x.HeroLayout).Returns("FullWidth");
        page.Setup(x => x.HeroBackgroundImageAltText).Returns("Background Alt");
        page.Setup(x => x.HeroBackgroundImage_MobileAltText).Returns("Mobile Alt");
        page.Setup(x => x.HeroCallToActionButtonType).Returns("Primary");
        page.Setup(x => x.HeroCallToActionButtonText).Returns("Learn More");
        page.Setup(x => x.HeroBackgroundImage).Returns(new List<ContentAttachment>());
        page.Setup(x => x.HeroBackgroundImage_Mobile).Returns(new List<ContentAttachment>());
        page.Setup(x => x.HeroCallToActionButtonUrl).Returns(new List<IWebPageFieldsSource>());
        page.Setup(x => x.MetadataTitle).Returns("Test Page Title");
        page.Setup(x => x.MetadataDescription).Returns("Test Page Description");
        page.Setup(x => x.SystemFields).Returns(new WebPageFields { WebPageItemID = 123 });
        return page.Object;
    }
}