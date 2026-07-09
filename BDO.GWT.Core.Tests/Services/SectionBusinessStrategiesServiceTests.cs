using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;
using NUnit.Framework;

namespace BDO.GWT.Core.Tests.Services;

[TestFixture]
public class SectionBusinessStrategiesServiceTests
{
    private Mock<IHeroService> _heroServiceMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IWebPageUrlResolver> _urlResolverMock = null!;
    private Mock<ICultureContext> _cultureContextMock = null!;
    private SectionBusinessStrategiesService _service = null!;

    [SetUp]
    public void Setup()
    {
        _heroServiceMock = new Mock<IHeroService>();
        _contentServiceMock = new Mock<IContentService>();
        _urlResolverMock = new Mock<IWebPageUrlResolver>();
        _cultureContextMock = new Mock<ICultureContext>();

        _cultureContextMock.Setup(x => x.CurrentLanguageTag).Returns("en");

        _service = new SectionBusinessStrategiesService(
            _heroServiceMock.Object,
            _contentServiceMock.Object,
            _urlResolverMock.Object,
            _cultureContextMock.Object);
    }

    #region Basic Flow Tests

    [Test]
    public async Task GetBusinessStrategiesDtoAsync_WhenPageExists_CallsHeroService()
    {
        // Arrange
        var page = CreateMockSectionBusinessStrategies();
        var expectedHeroDto = new HeroDto { Title = "Section Hero" };

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHeroDto);

        SetupContentServiceMocks();

        // Act
        var result = await _service.GetBusinessStrategiesDtoAsync(page, CancellationToken.None);

        // Assert
        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetBusinessStrategiesDtoAsync_WhenPageExists_ReturnsSectionBusinessStrategiesDto()
    {
        // Arrange
        var page = CreateMockSectionBusinessStrategies();
        var expectedHeroDto = new HeroDto { Title = "Section Hero" };

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHeroDto);

        SetupContentServiceMocks();

        // Act
        var result = await _service.GetBusinessStrategiesDtoAsync(page, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SectionBusinessStrategiesDto>());
            Assert.That(result.Hero, Is.EqualTo(expectedHeroDto));
        });
    }

    #endregion

    #region Business Strategies Menu Tests

    [Test]
    public async Task GetBusinessStrategiesDtoAsync_ReturnsMenuItemsWhenBusinessStrategiesExist()
    {
        // Arrange
        var page = CreateMockSectionBusinessStrategies();
        var businessStrategies = CreateMockBusinessStrategyList(3);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        SetupContentServiceMocks(businessStrategiesCount: 3);

        // Act
        var result = await _service.GetBusinessStrategiesDtoAsync(page, CancellationToken.None);

        // Assert
        Assert.That(result.BusinessStrategiesMenu, Is.Not.Empty);
        Assert.That(result.BusinessStrategiesMenu.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetBusinessStrategiesDtoAsync_ReturnsEmptyMenuListWhenNoBusinessStrategiesExist()
    {
        // Arrange
        var page = CreateMockSectionBusinessStrategies();

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        SetupContentServiceMocks(businessStrategiesCount: 0);

        // Act
        var result = await _service.GetBusinessStrategiesDtoAsync(page, CancellationToken.None);

        // Assert
        Assert.That(result.BusinessStrategiesMenu, Is.Empty);
    }


    [Test]
    public async Task GetBusinessStrategiesDtoAsync_OnlyIncludesStrategiesWithMenuShowInSectionTrue()
    {
        // Arrange
        var page = CreateMockSectionBusinessStrategies();

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        SetupContentServiceMocks(businessStrategiesCount: 5);

        // Act
        var result = await _service.GetBusinessStrategiesDtoAsync(page, CancellationToken.None);

        // Assert
        _contentServiceMock.Verify(
            x => x.GetPagesAsync<BusinessStrategy>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Background Image Tests

    [Test]
    public async Task GetBusinessStrategiesDtoAsync_WithoutMenuBackgroundImage_BackgroundImageUrlIsNull()
    {
        // Arrange
        var page = CreateMockSectionBusinessStrategies();

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        SetupContentServiceMocks();

        // Act
        var result = await _service.GetBusinessStrategiesDtoAsync(page, CancellationToken.None);

        // Assert
        Assert.That(result.BackgroundImageURL, Is.Null);
    }

  

    [Test]
    public async Task GetBusinessStrategiesDtoAsync_WithMenuBackgroundImageAltText_ReturnsAltText()
    {
        // Arrange
        const string altText = "Menu Background Image";
        const string bgImageUrl = "https://example.com/menu-bg.jpg";
        
        var page = new SectionBusinessStrategies
        {
            HeroTitle = "Section Title",
            HeroDescription = "Section Description",
            HeroLayout = "default",
            HeroBackgroundImage = null,
            HeroBackgroundImage_Mobile = null,
            HeroCallToActionButtonUrl = null,
            HeroCallToActionButtonText = null,
            HeroCallToActionButtonType = null,
            HeroBackgroundImageAltText = null,
            HeroBackgroundImage_MobileAltText = null,
            MenuBackgroundImage = CreateMockAttachmentList(bgImageUrl),
            MenuBackgroundImageAltText = altText,
            SystemFields = CreateMockWebPageFields(1, "Section", 0),
        };

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        SetupContentServiceMocks();

        // Act
        var result = await _service.GetBusinessStrategiesDtoAsync(page, CancellationToken.None);

        // Assert
        Assert.That(result.BackgroundImageAltText, Is.EqualTo(altText));
    }

    #endregion

    #region Hero Data Tests

    [Test]
    public async Task GetBusinessStrategiesDtoAsync_ReturnsHeroData()
    {
        // Arrange
        var page = CreateMockSectionBusinessStrategies();
        var expectedHeroDto = new HeroDto 
        { 
            Title = "Strategies", 
            Description = "Business Strategies Description" 
        };

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHeroDto);

        SetupContentServiceMocks();

        // Act
        var result = await _service.GetBusinessStrategiesDtoAsync(page, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Hero, Is.Not.Null);
            Assert.That(result.Hero.Title, Is.EqualTo("Strategies"));
            Assert.That(result.Hero.Description, Is.EqualTo("Business Strategies Description"));
        });
    }

    #endregion

    #region Culture Context Tests

    [Test]
    public async Task GetBusinessStrategiesDtoAsync_UsesCultureContextLanguageTag()
    {
        // Arrange
        var page = CreateMockSectionBusinessStrategies();
        const string languageTag = "en";

        _cultureContextMock.Setup(x => x.CurrentLanguageTag).Returns(languageTag);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        SetupContentServiceMocks();

        // Act
        await _service.GetBusinessStrategiesDtoAsync(page, CancellationToken.None);

        // Assert
        _contentServiceMock.Verify(
            x => x.GetPagesAsync<BusinessStrategiesCategory>(
                It.Is<RetrievePagesParameters>(p => p.LanguageName == languageTag),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Async and Cancellation Tests

    [Test]
    public async Task GetBusinessStrategiesDtoAsync_RespectsAsyncCancellation()
    {
        // Arrange
        var page = CreateMockSectionBusinessStrategies();
        var cancellationToken = new CancellationToken(canceled: true);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _service.GetBusinessStrategiesDtoAsync(page, cancellationToken));
    }

    [Test]
    public async Task GetBusinessStrategiesDtoAsync_PassesCancellationTokenToContentService()
    {
        // Arrange
        var page = CreateMockSectionBusinessStrategies();
        var cancellationToken = new CancellationToken();

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        SetupContentServiceMocks();

        // Act
        await _service.GetBusinessStrategiesDtoAsync(page, cancellationToken);

        // Assert
        _contentServiceMock.Verify(
            x => x.GetPagesAsync<BusinessStrategy>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                cancellationToken),
            Times.Once);
    }

    [Test]
    public async Task GetBusinessStrategiesDtoAsync_PassesCancellationTokenToHeroService()
    {
        // Arrange
        var page = CreateMockSectionBusinessStrategies();
        var cancellationToken = new CancellationToken();

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        SetupContentServiceMocks();

        // Act
        await _service.GetBusinessStrategiesDtoAsync(page, cancellationToken);

        // Assert
        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), cancellationToken),
            Times.Once);
    }

    [Test]
    public async Task GetBusinessStrategiesDtoAsync_WhenContentServiceThrowsException_PropagatesException()
    {
        // Arrange
        var page = CreateMockSectionBusinessStrategies();
        var exceptionMessage = "Content service error";

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<BusinessStrategiesCategory>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.GetBusinessStrategiesDtoAsync(page, CancellationToken.None));
        Assert.That(exception!.Message, Is.EqualTo(exceptionMessage));
    }

    #endregion

    #region Helper Methods

    private static SectionBusinessStrategies CreateMockSectionBusinessStrategies()
    {
        return new SectionBusinessStrategies
        {
            HeroTitle = "Business Strategies Section",
            HeroDescription = "Explore our business strategies",
            HeroLayout = "default",
            HeroBackgroundImage = null,
            HeroBackgroundImage_Mobile = null,
            HeroCallToActionButtonUrl = null,
            HeroCallToActionButtonText = null,
            HeroCallToActionButtonType = null,
            HeroBackgroundImageAltText = null,
            HeroBackgroundImage_MobileAltText = null,
            MenuBackgroundImage = null,
            MenuBackgroundImageAltText = null,
            SystemFields = CreateMockWebPageFields(1, "Section", 0),
        };
    }

    private static SectionBusinessStrategies CreateMockSectionBusinessStrategiesWithImage(string imageUrl)
    {
        return new SectionBusinessStrategies
        {
            HeroTitle = "Business Strategies Section",
            HeroDescription = "Explore our business strategies",
            HeroLayout = "default",
            HeroBackgroundImage = null,
            HeroBackgroundImage_Mobile = null,
            HeroCallToActionButtonUrl = null,
            HeroCallToActionButtonText = null,
            HeroCallToActionButtonType = null,
            HeroBackgroundImageAltText = null,
            HeroBackgroundImage_MobileAltText = null,
            MenuBackgroundImage = CreateMockAttachmentList(imageUrl),
            MenuBackgroundImageAltText = null,
            SystemFields = CreateMockWebPageFields(1, "Section", 0),
        };
    }

    private static List<BusinessStrategy> CreateMockBusinessStrategyList(int count)
    {
        var list = new List<BusinessStrategy>();
        for (int i = 0; i < count; i++)
        {
            list.Add(new BusinessStrategy
            {
                MetadataTitle = $"Strategy {i + 1}",
                MetadataDescription = $"Description {i + 1}",
                MenuShowInSection = true,
                MenuTileIcon = CreateMockAttachmentList($"https://example.com/icon-{i + 1}.png"),
                MenuReadMoreText = "Read More",
                SystemFields = CreateMockWebPageFields(i + 2, $"Strategy {i + 1}", 1),
            });
        }
        return list;
    }

    private static List<ContentAttachment> CreateMockAttachmentList(string url)
    {
        return new List<ContentAttachment>();
    }

    private static WebPageFields CreateMockWebPageFields(int id, string name, int parentId)
    {
        return new WebPageFields
        {
            WebPageItemID = id,
            WebPageItemName = name,
            WebPageItemParentID = parentId,
        };
    }

    private void SetupContentServiceMocks(int businessStrategiesCount = 1)
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<BusinessStrategiesCategory>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BusinessStrategiesCategory>());

        var strategies = CreateMockBusinessStrategyList(businessStrategiesCount);

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<BusinessStrategy>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(strategies);

        _urlResolverMock
            .Setup(x => x.GetRelativePath(It.IsAny<BusinessStrategy>()))
            .Returns<BusinessStrategy>(s => $"/strategies/{s.SystemFields.WebPageItemName.ToLower().Replace(" ", "-")}");
    }

    private void SetupContentServiceMocksWithStrategyDetails(
        string title,
        string description,
        string imageUrl,
        string readMoreText)
    {
        var strategy = new BusinessStrategy
        {
            MetadataTitle = title,
            MetadataDescription = description,
            MenuShowInSection = true,
            MenuTileIcon = CreateMockAttachmentList(imageUrl),
            MenuReadMoreText = readMoreText,
            SystemFields = CreateMockWebPageFields(2, title, 1),
        };

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<BusinessStrategiesCategory>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BusinessStrategiesCategory>());

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<BusinessStrategy>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BusinessStrategy> { strategy });

        _urlResolverMock
            .Setup(x => x.GetRelativePath(It.IsAny<BusinessStrategy>()))
            .Returns<BusinessStrategy>(s => $"/strategies/{s.SystemFields.WebPageItemName.ToLower().Replace(" ", "-")}");
    }

    #endregion
}
