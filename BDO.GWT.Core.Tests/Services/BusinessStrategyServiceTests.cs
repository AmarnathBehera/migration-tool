using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Moq;
using NUnit.Framework;

namespace BDO.GWT.Core.Tests.Services;

[TestFixture]
public class BusinessStrategyServiceTests
{
    private Mock<IHeroService> _heroServiceMock = null!;
    private BusinessStrategyService _service = null!;

    [SetUp]
    public void Setup()
    {
        _heroServiceMock = new Mock<IHeroService>();
        _service = new BusinessStrategyService(_heroServiceMock.Object);
    }

    #region Basic Flow Tests

    [Test]
    public async Task GetBusinessStrategyDtoAsync_WhenPageExists_CallsHeroService()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();
        var expectedHeroDto = new HeroDto { Title = "Test Hero" };

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHeroDto);

        // Act
        var result = await _service.GetBusinessStrategyDtoAsync(businessStrategy, CancellationToken.None);

        // Assert
        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetBusinessStrategyDtoAsync_WhenPageExists_ReturnsBusinessStrategyDto()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();
        var expectedHeroDto = new HeroDto { Title = "Business Strategy Hero" };

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHeroDto);

        // Act
        var result = await _service.GetBusinessStrategyDtoAsync(businessStrategy, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<BusinessStrategyDto>());
            Assert.That(result.Hero, Is.EqualTo(expectedHeroDto));
        });
    }

    #endregion

    #region Hero Image Tests

    [Test]
    public async Task GetBusinessStrategyDtoAsync_WithoutHeroBackgroundImage_PassesNullImageUrlToHeroService()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        // Act
        await _service.GetBusinessStrategyDtoAsync(businessStrategy, CancellationToken.None);

        // Assert
        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(It.Is<HeroData>(h =>
                h.BackgroundImage == null &&
                h.MobileBackgroundImage == null),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Test]
    public async Task GetBusinessStrategyDtoAsync_PassesImageAltTextToHeroService()
    {
        // Arrange
        const string altText = "Background Image Alt Text";
        const string mobileAltText = "Mobile Background Alt Text";
        
        var businessStrategy = new BusinessStrategy
        {
            HeroTitle = "Title",
            HeroDescription = "Description",
            HeroLayout = "default",
            HeroBackgroundImage = null,
            HeroBackgroundImage_Mobile = null,
            HeroCallToActionButtonUrl = null,
            HeroCallToActionButtonText = null,
            HeroCallToActionButtonType = null,
            HeroBackgroundImageAltText = altText,
            HeroBackgroundImage_MobileAltText = mobileAltText,
        };

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        // Act
        await _service.GetBusinessStrategyDtoAsync(businessStrategy, CancellationToken.None);

        // Assert
        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(It.Is<HeroData>(h =>
                h.BackgroundImageAltText == altText &&
                h.MobileBackgroundImageAltText == mobileAltText),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Hero Data Tests

    [Test]
    public async Task GetBusinessStrategyDtoAsync_WithHeroData_PassesHeroDataToService()
    {
        // Arrange
        const string heroTitle = "Test Business Strategy";
        const string heroDescription = "Test Description";
        const string heroLayout = "test-layout";

        var businessStrategy = CreateMockBusinessStrategyWithHeroData(
            title: heroTitle,
            description: heroDescription,
            layout: heroLayout);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        // Act
        await _service.GetBusinessStrategyDtoAsync(businessStrategy, CancellationToken.None);

        // Assert
        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(It.Is<HeroData>(h =>
                h.Title == heroTitle &&
                h.Description == heroDescription &&
                h.Layout == heroLayout),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetBusinessStrategyDtoAsync_ReturnsHeroDto()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();
        var expectedHeroDto = new HeroDto 
        { 
            Title = "Business Strategy", 
            Description = "Strategy Description" 
        };

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHeroDto);

        // Act
        var result = await _service.GetBusinessStrategyDtoAsync(businessStrategy, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Hero, Is.Not.Null);
            Assert.That(result.Hero.Title, Is.EqualTo("Business Strategy"));
            Assert.That(result.Hero.Description, Is.EqualTo("Strategy Description"));
        });
    }

    #endregion

    #region Call To Action Tests

    [Test]
    public async Task GetBusinessStrategyDtoAsync_WithCallToActionButton_PassesCtaDataToHeroService()
    {
        // Arrange
        const string ctaButtonText = "Learn More";
        const string ctaButtonType = "primary";

        var businessStrategy = CreateMockBusinessStrategyWithCta(
            buttonText: ctaButtonText,
            buttonType: ctaButtonType);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        // Act
        await _service.GetBusinessStrategyDtoAsync(businessStrategy, CancellationToken.None);

        // Assert
        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(It.Is<HeroData>(h =>
                h.CallToActionButtonText == ctaButtonText &&
                h.CallToActionButtonType == ctaButtonType),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetBusinessStrategyDtoAsync_WithCallToActionUrl_ParsesCtaUrlCorrectly()
    {
        // Arrange
        const string ctaUrl = "/learn-more";
        var businessStrategy = CreateMockBusinessStrategyWithCtaUrl(ctaUrl);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        // Act
        await _service.GetBusinessStrategyDtoAsync(businessStrategy, CancellationToken.None);

        // Assert
        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(It.Is<HeroData>(h =>
                h.CallToActionButtonUrl != null),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetBusinessStrategyDtoAsync_WithoutCallToActionUrl_CallToActionUrlIsNull()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        // Act
        await _service.GetBusinessStrategyDtoAsync(businessStrategy, CancellationToken.None);

        // Assert
        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(It.Is<HeroData>(h =>
                h.CallToActionButtonUrl == null),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Async and Cancellation Tests

    [Test]
    public async Task GetBusinessStrategyDtoAsync_RespectsAsyncCancellation()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();
        var cancellationToken = new CancellationToken(canceled: true);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _service.GetBusinessStrategyDtoAsync(businessStrategy, cancellationToken));
    }

    [Test]
    public async Task GetBusinessStrategyDtoAsync_PassesCancellationTokenToHeroService()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();
        var cancellationToken = new CancellationToken();

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        // Act
        await _service.GetBusinessStrategyDtoAsync(businessStrategy, cancellationToken);

        // Assert
        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), cancellationToken),
            Times.Once);
    }

    [Test]
    public async Task GetBusinessStrategyDtoAsync_WhenServiceThrowsException_PropagatesException()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();
        var exceptionMessage = "Hero service error";

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.GetBusinessStrategyDtoAsync(businessStrategy, CancellationToken.None));
        Assert.That(exception!.Message, Is.EqualTo(exceptionMessage));
    }

    #endregion


    #region Helper Methods

    private static BusinessStrategy CreateMockBusinessStrategy()
    {
        return new BusinessStrategy
        {
            HeroTitle = "Business Strategy Title",
            HeroDescription = "Business Strategy Description",
            HeroLayout = "default",
            HeroBackgroundImage = null,
            HeroBackgroundImage_Mobile = null,
            HeroCallToActionButtonUrl = null,
            HeroCallToActionButtonText = null,
            HeroCallToActionButtonType = null,
            HeroBackgroundImageAltText = null,
            HeroBackgroundImage_MobileAltText = null,
        };
    }

    private static BusinessStrategy CreateMockBusinessStrategyWithHeroData(
        string title,
        string description,
        string layout)
    {
        return new BusinessStrategy
        {
            HeroTitle = title,
            HeroDescription = description,
            HeroLayout = layout,
            HeroBackgroundImage = null,
            HeroBackgroundImage_Mobile = null,
            HeroCallToActionButtonUrl = null,
            HeroCallToActionButtonText = null,
            HeroCallToActionButtonType = null,
            HeroBackgroundImageAltText = null,
            HeroBackgroundImage_MobileAltText = null,
        };
    }

    private static BusinessStrategy CreateMockBusinessStrategyWithCta(
        string buttonText,
        string buttonType)
    {
        return new BusinessStrategy
        {
            HeroTitle = "Business Strategy Title",
            HeroDescription = "Business Strategy Description",
            HeroLayout = "default",
            HeroBackgroundImage = null,
            HeroBackgroundImage_Mobile = null,
            HeroCallToActionButtonUrl = null,
            HeroCallToActionButtonText = buttonText,
            HeroCallToActionButtonType = buttonType,
            HeroBackgroundImageAltText = null,
            HeroBackgroundImage_MobileAltText = null,
        };
    }

    private static BusinessStrategy CreateMockBusinessStrategyWithImages(
        string? imageUrl = null,
        string? mobileImageUrl = null)
    {
        return new BusinessStrategy
        {
            HeroTitle = "Business Strategy Title",
            HeroDescription = "Business Strategy Description",
            HeroLayout = "default",
            HeroBackgroundImage = imageUrl != null ? CreateMockAttachmentList(imageUrl) : null,
            HeroBackgroundImage_Mobile = mobileImageUrl != null ? CreateMockAttachmentList(mobileImageUrl) : null,
            HeroCallToActionButtonUrl = null,
            HeroCallToActionButtonText = null,
            HeroCallToActionButtonType = null,
            HeroBackgroundImageAltText = null,
            HeroBackgroundImage_MobileAltText = null,
        };
    }

    private static BusinessStrategy CreateMockBusinessStrategyWithCtaUrl(string ctaUrl)
    {
        return new BusinessStrategy
        {
            HeroTitle = "Business Strategy Title",
            HeroDescription = "Business Strategy Description",
            HeroLayout = "default",
            HeroBackgroundImage = null,
            HeroBackgroundImage_Mobile = null,
            HeroCallToActionButtonUrl = CreateMockWebPageReferenceList(ctaUrl),
            HeroCallToActionButtonText = "Learn More",
            HeroCallToActionButtonType = "primary",
            HeroBackgroundImageAltText = null,
            HeroBackgroundImage_MobileAltText = null,
        };
    }

    private static List<IWebPageFieldsSource> CreateMockWebPageReferenceList(string urlPath)
    {
        // Create a mock that simulates a web page reference with SystemFields containing WebPageUrlPath
        var mockFields = new WebPageFields 
        { 
            WebPageItemID = 1,
            WebPageItemName = "MockPage",
            WebPageItemParentID = 0,
            WebPageUrlPath = urlPath
        };
        
        var mockReference = new Mock<IWebPageFieldsSource>();
        mockReference.Setup(x => x.SystemFields).Returns(mockFields);
        
        return new List<IWebPageFieldsSource> { mockReference.Object };
    }

    private static List<ContentAttachment> CreateMockAttachmentList(string url)
    {
        return new List<ContentAttachment>();
    }

    private static WebPageFields CreateMockWebPageFields(int webPageItemId, string name, int parentId)
    {
        return new WebPageFields
        {
            WebPageItemID = webPageItemId,
            WebPageItemName = name,
            WebPageItemParentID = parentId,
        };
    }

    #endregion
}
