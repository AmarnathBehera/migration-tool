using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.DTOs;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

[TestFixture]
public class SectionBusinessStrategiesControllerTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<ISectionBusinessStrategiesService> _sectionBusinessStrategiesServiceMock = null!;
    private SectionBusinessStrategiesController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _sectionBusinessStrategiesServiceMock = new Mock<ISectionBusinessStrategiesService>();
        _controller = new SectionBusinessStrategiesController(
            _contentServiceMock.Object,
            _sectionBusinessStrategiesServiceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    #region Null Page Tests

    [Test]
    public async Task Index_WhenCurrentPageIsNull_ReturnsNotFound()
    {
        // Arrange
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SectionBusinessStrategies?)null);

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Index_WhenCurrentPageIsNull_DoesNotCallSectionBusinessStrategiesService()
    {
        // Arrange
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SectionBusinessStrategies?)null);

        // Act
        await _controller.Index(CancellationToken.None);

        // Assert
        _sectionBusinessStrategiesServiceMock.Verify(
            x => x.GetBusinessStrategiesDtoAsync(It.IsAny<SectionBusinessStrategies>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Index_WhenCurrentPageIsNull_DoesNotCallContentServiceAgain()
    {
        // Arrange
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SectionBusinessStrategies?)null);

        // Act
        await _controller.Index(CancellationToken.None);

        // Assert
        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Happy Path Tests

    [Test]
    public async Task Index_WhenCurrentPageExists_ReturnsViewResult()
    {
        // Arrange
        var sectionPage = CreateMockSectionBusinessStrategies();
        var expectedDto = new SectionBusinessStrategiesDto();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectionPage);

        _sectionBusinessStrategiesServiceMock
            .Setup(x => x.GetBusinessStrategiesDtoAsync(sectionPage, It.IsAny<CancellationToken>()))
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
        var sectionPage = CreateMockSectionBusinessStrategies();
        var expectedDto = new SectionBusinessStrategiesDto
        {
            Hero = new HeroDto { Title = "Business Strategies Hero" },
            BackgroundImageURL = "https://example.com/bg.jpg",
            BackgroundImageAltText = "Background"
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectionPage);

        _sectionBusinessStrategiesServiceMock
            .Setup(x => x.GetBusinessStrategiesDtoAsync(sectionPage, It.IsAny<CancellationToken>()))
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
        var sectionPage = CreateMockSectionBusinessStrategies();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectionPage);

        _sectionBusinessStrategiesServiceMock
            .Setup(x => x.GetBusinessStrategiesDtoAsync(sectionPage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SectionBusinessStrategiesDto());

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("~/Views/SectionBusinessStrategies/Index.cshtml"));
    }

    [Test]
    public async Task Index_WhenCurrentPageExists_CallsSectionBusinessStrategiesService()
    {
        // Arrange
        var sectionPage = CreateMockSectionBusinessStrategies();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectionPage);

        _sectionBusinessStrategiesServiceMock
            .Setup(x => x.GetBusinessStrategiesDtoAsync(sectionPage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SectionBusinessStrategiesDto());

        // Act
        await _controller.Index(CancellationToken.None);

        // Assert
        _sectionBusinessStrategiesServiceMock.Verify(
            x => x.GetBusinessStrategiesDtoAsync(sectionPage, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Service Parameters Tests

    [Test]
    public async Task Index_CallsContentServiceWithCorrectParameters()
    {
        // Arrange
        var sectionPage = CreateMockSectionBusinessStrategies();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectionPage);

        _sectionBusinessStrategiesServiceMock
            .Setup(x => x.GetBusinessStrategiesDtoAsync(sectionPage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SectionBusinessStrategiesDto());

        // Act
        await _controller.Index(CancellationToken.None);

        // Assert
        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Index_PassesCancellationTokenToContentService()
    {
        // Arrange
        var sectionPage = CreateMockSectionBusinessStrategies();
        var cancellationToken = new CancellationToken();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectionPage);

        _sectionBusinessStrategiesServiceMock
            .Setup(x => x.GetBusinessStrategiesDtoAsync(sectionPage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SectionBusinessStrategiesDto());

        // Act
        await _controller.Index(cancellationToken);

        // Assert
        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                cancellationToken),
            Times.Once);
    }

    [Test]
    public async Task Index_PassesCancellationTokenToService()
    {
        // Arrange
        var sectionPage = CreateMockSectionBusinessStrategies();
        var cancellationToken = new CancellationToken();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectionPage);

        _sectionBusinessStrategiesServiceMock
            .Setup(x => x.GetBusinessStrategiesDtoAsync(sectionPage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SectionBusinessStrategiesDto());

        // Act
        await _controller.Index(cancellationToken);

        // Assert
        _sectionBusinessStrategiesServiceMock.Verify(
            x => x.GetBusinessStrategiesDtoAsync(sectionPage, cancellationToken),
            Times.Once);
    }

    #endregion

    #region Async and Cancellation Tests

    [Test]
    public async Task Index_RespectsAsyncCancellation_WhenContentServiceCancelled()
    {
        // Arrange
        var cancellationToken = new CancellationToken(canceled: true);

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _controller.Index(cancellationToken));
    }

    [Test]
    public async Task Index_RespectsAsyncCancellation_WhenServiceCancelled()
    {
        // Arrange
        var sectionPage = CreateMockSectionBusinessStrategies();
        var cancellationToken = new CancellationToken(canceled: true);

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectionPage);

        _sectionBusinessStrategiesServiceMock
            .Setup(x => x.GetBusinessStrategiesDtoAsync(sectionPage, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _controller.Index(cancellationToken));
    }

    #endregion

    #region Exception Handling Tests

    [Test]
    public async Task Index_WhenContentServiceThrowsException_PropagatesException()
    {
        // Arrange
        var exceptionMessage = "Content service error";

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _controller.Index(CancellationToken.None));
        Assert.That(exception!.Message, Is.EqualTo(exceptionMessage));
    }

    [Test]
    public async Task Index_WhenServiceThrowsException_PropagatesException()
    {
        // Arrange
        var sectionPage = CreateMockSectionBusinessStrategies();
        var exceptionMessage = "Service error";

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectionPage);

        _sectionBusinessStrategiesServiceMock
            .Setup(x => x.GetBusinessStrategiesDtoAsync(sectionPage, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _controller.Index(CancellationToken.None));
        Assert.That(exception!.Message, Is.EqualTo(exceptionMessage));
    }

    [Test]
    public async Task Index_WhenServiceThrowsArgumentException_PropagatesArgumentException()
    {
        // Arrange
        var sectionPage = CreateMockSectionBusinessStrategies();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectionPage);

        _sectionBusinessStrategiesServiceMock
            .Setup(x => x.GetBusinessStrategiesDtoAsync(sectionPage, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid argument"));

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _controller.Index(CancellationToken.None));
    }

    #endregion

    #region View Result Tests

    [Test]
    public async Task Index_ViewResult_HasCorrectViewName()
    {
        // Arrange
        var sectionPage = CreateMockSectionBusinessStrategies();
        var expectedViewName = "~/Views/SectionBusinessStrategies/Index.cshtml";

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectionPage);

        _sectionBusinessStrategiesServiceMock
            .Setup(x => x.GetBusinessStrategiesDtoAsync(sectionPage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SectionBusinessStrategiesDto());

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.ViewName, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task Index_ViewResult_ModelIsNotNull()
    {
        // Arrange
        var sectionPage = CreateMockSectionBusinessStrategies();
        var expectedDto = new SectionBusinessStrategiesDto
        {
            Hero = new HeroDto { Title = "Hero Title" }
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectionPage);

        _sectionBusinessStrategiesServiceMock
            .Setup(x => x.GetBusinessStrategiesDtoAsync(sectionPage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.Model, Is.Not.Null);
        Assert.That(viewResult.Model, Is.InstanceOf<SectionBusinessStrategiesDto>());
    }

    [Test]
    public async Task Index_ViewResult_ModelMatchesServiceResult()
    {
        // Arrange
        var sectionPage = CreateMockSectionBusinessStrategies();
        var expectedDto = new SectionBusinessStrategiesDto
        {
            Hero = new HeroDto
            {
                Title = "Business Strategies",
                Description = "Description Text"
            },
            BackgroundImageURL = "https://example.com/bg.jpg"
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectionPage);

        _sectionBusinessStrategiesServiceMock
            .Setup(x => x.GetBusinessStrategiesDtoAsync(sectionPage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        var viewResult = (ViewResult)result;
        var model = (SectionBusinessStrategiesDto)viewResult.Model!;
        Assert.Multiple(() =>
        {
            Assert.That(model.Hero.Title, Is.EqualTo("Business Strategies"));
            Assert.That(model.Hero.Description, Is.EqualTo("Description Text"));
            Assert.That(model.BackgroundImageURL, Is.EqualTo("https://example.com/bg.jpg"));
        });
    }

    #endregion

    #region Business Strategies Menu Tests

    [Test]
    public async Task Index_WithMultipleBusinessStrategies_ReturnsCompleteDto()
    {
        // Arrange
        var sectionPage = CreateMockSectionBusinessStrategies();
        var menuItems = new List<BusinessStrategiesMenuDto>
        {
            new BusinessStrategiesMenuDto
            {
                Title = "Strategy 1",
                ImageURL = "https://example.com/img1.jpg",
                Description = "Strategy 1 Description",
                ReadMoreLink = new LinkItemDto { Title = "Read More", URL = "/strategy-1" }
            },
            new BusinessStrategiesMenuDto
            {
                Title = "Strategy 2",
                ImageURL = "https://example.com/img2.jpg",
                Description = "Strategy 2 Description",
                ReadMoreLink = new LinkItemDto { Title = "Read More", URL = "/strategy-2" }
            },
        };

        var expectedDto = new SectionBusinessStrategiesDto
        {
            Hero = new HeroDto { Title = "Strategies" },
            BusinessStrategiesMenu = menuItems,
            BackgroundImageURL = "https://example.com/menu-bg.jpg"
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectionPage);

        _sectionBusinessStrategiesServiceMock
            .Setup(x => x.GetBusinessStrategiesDtoAsync(sectionPage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SectionBusinessStrategiesDto;
        Assert.Multiple(() =>
        {
            Assert.That(model!.BusinessStrategiesMenu.Count, Is.EqualTo(2));
            Assert.That(model.BusinessStrategiesMenu[0].Title, Is.EqualTo("Strategy 1"));
            Assert.That(model.BusinessStrategiesMenu[1].Title, Is.EqualTo("Strategy 2"));
            Assert.That(model.BackgroundImageURL, Is.EqualTo("https://example.com/menu-bg.jpg"));
        });
    }

    [Test]
    public async Task Index_WithEmptyMenuItems_ReturnsEmptyList()
    {
        // Arrange
        var sectionPage = CreateMockSectionBusinessStrategies();
        var expectedDto = new SectionBusinessStrategiesDto
        {
            Hero = new HeroDto { Title = "Strategies" },
            BusinessStrategiesMenu = new List<BusinessStrategiesMenuDto>(),
            BackgroundImageURL = null
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectionPage);

        _sectionBusinessStrategiesServiceMock
            .Setup(x => x.GetBusinessStrategiesDtoAsync(sectionPage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SectionBusinessStrategiesDto;
        Assert.That(model!.BusinessStrategiesMenu, Is.Empty);
    }

    #endregion

    #region Multiple Invocations Tests

    [Test]
    public async Task Index_CalledMultipleTimes_WorksCorrectly()
    {
        // Arrange
        var sectionPage = CreateMockSectionBusinessStrategies();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectionPage);

        _sectionBusinessStrategiesServiceMock
            .Setup(x => x.GetBusinessStrategiesDtoAsync(sectionPage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SectionBusinessStrategiesDto());

        // Act
        var result1 = await _controller.Index(CancellationToken.None);
        var result2 = await _controller.Index(CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result1, Is.InstanceOf<ViewResult>());
            Assert.That(result2, Is.InstanceOf<ViewResult>());
        });

        _sectionBusinessStrategiesServiceMock.Verify(
            x => x.GetBusinessStrategiesDtoAsync(sectionPage, It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    #endregion

    #region Helper Methods

    private static SectionBusinessStrategies CreateMockSectionBusinessStrategies()
    {
        return new SectionBusinessStrategies
        {
            HeroTitle = "Business Strategies Section",
            HeroDescription = "Explore our strategies",
            HeroLayout = "default",
        };
    }

    #endregion
}
