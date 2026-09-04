using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services.Widget;
using BDO.GWT.GLOBAL.Widgets;
using BDO.GWT.Web.Core.DTOs;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget
{
    public class ViewGlobalLocationsWidgetTests
    {
        private ViewGlobalLocationsWidget _widget;
        private DefaultHttpContext _httpContext;
        private Mock<IViewGlobalLocationsService> _mockViewGlobalLocationsService;
        private Mock<IContentService> _mockContentService;

        [SetUp]
        public void SetUp()
        {
            _mockViewGlobalLocationsService = new Mock<IViewGlobalLocationsService>();
            _mockContentService = new Mock<IContentService>();
            _widget = new ViewGlobalLocationsWidget(_mockViewGlobalLocationsService.Object);

            _httpContext = new DefaultHttpContext();
            _httpContext.RequestAborted = CancellationToken.None;

            _widget.ViewComponentContext = new ViewComponentContext
            {
                ViewContext = new ViewContext
                {
                    HttpContext = _httpContext,
                    ViewData = new ViewDataDictionary(
                        new EmptyModelMetadataProvider(),
                        new ModelStateDictionary()
                    )
                }
            };
        }

        [Test]
        public async Task InvokeAsync_ReturnsViewWithDtoFromService()
        {
            // Arrange
            var model = BuildModel(new ViewGlobalLocationsWidgetProperties
            {
                Title = "Test Title",
                Description = "Test Description",
                Theme = "dark",
                Layout = "grid",
                TopMargin = "10px",
                BottomMargin = "20px",
                ButtonText = "Click Me",
                ButtonUrl = "https://example.com",
                ButtonLinkTarget = "_blank"
            });

            var expectedDto = new ViewGlobalLocationsDto
            {
                Title = "Test Title",
                Theme = "dark",
                Button = new ButtonDto { Type = "secondary" }
            };

            _mockViewGlobalLocationsService
                .Setup(s => s.GetGlobalLocationsBannerAsync(It.IsAny<ViewGlobalLocationsData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            // Act
            var result = await _widget.InvokeAsync(model);

            // Assert
            Assert.That(result, Is.TypeOf<ViewViewComponentResult>());

            var viewResult = result as ViewViewComponentResult;
            var viewModel = viewResult?.ViewData?.Model as ViewGlobalLocationsDto;

            Assert.That(viewModel, Is.SameAs(expectedDto));
        }

        [Test]
        public async Task InvokeAsync_MapsPropertiesToServiceData()
        {
            // Arrange
            var model = BuildModel(new ViewGlobalLocationsWidgetProperties
            {
                Title = "Test Title",
                Description = "Test Description",
                Theme = "dark",
                Layout = "grid",
                TopMargin = "10px",
                BottomMargin = "20px",
                ButtonText = "Click Me",
                ButtonUrl = "https://example.com",
                ButtonLinkTarget = "_blank"
            });

            ViewGlobalLocationsData? capturedData = null;
            _mockViewGlobalLocationsService
                .Setup(s => s.GetGlobalLocationsBannerAsync(It.IsAny<ViewGlobalLocationsData>(), It.IsAny<CancellationToken>()))
                .Callback<ViewGlobalLocationsData, CancellationToken>((data, _) => capturedData = data)
                .ReturnsAsync(new ViewGlobalLocationsDto());

            // Act
            await _widget.InvokeAsync(model);

            // Assert
            Assert.That(capturedData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(capturedData!.Title, Is.EqualTo("Test Title"));
                Assert.That(capturedData.Description, Is.EqualTo("Test Description"));
                Assert.That(capturedData.Theme, Is.EqualTo("dark"));
                Assert.That(capturedData.Layout, Is.EqualTo("grid"));
                Assert.That(capturedData.TopMargin, Is.EqualTo("10px"));
                Assert.That(capturedData.BottomMargin, Is.EqualTo("20px"));
                Assert.That(capturedData.ButtonTitle, Is.EqualTo("Click Me"));
                Assert.That(capturedData.ButtonURL, Is.EqualTo("https://example.com"));
                Assert.That(capturedData.ButtonTarget, Is.EqualTo("_blank"));
            });
        }

        [TestCase("dark")]
        [TestCase("light")]
        public async Task InvokeAsync_PassesThemeToService(string theme)
        {
            var model = BuildModel(new ViewGlobalLocationsWidgetProperties { Theme = theme });

            ViewGlobalLocationsData? capturedData = null;
            _mockViewGlobalLocationsService
                .Setup(s => s.GetGlobalLocationsBannerAsync(It.IsAny<ViewGlobalLocationsData>(), It.IsAny<CancellationToken>()))
                .Callback<ViewGlobalLocationsData, CancellationToken>((data, _) => capturedData = data)
                .ReturnsAsync(new ViewGlobalLocationsDto());

            await _widget.InvokeAsync(model);

            Assert.That(capturedData?.Theme, Is.EqualTo(theme));
        }

        [Test]
        public async Task InvokeAsync_UsesRequestAbortedCancellationToken()
        {
            var model = BuildModel(new ViewGlobalLocationsWidgetProperties());

            _mockViewGlobalLocationsService
                .Setup(s => s.GetGlobalLocationsBannerAsync(It.IsAny<ViewGlobalLocationsData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ViewGlobalLocationsDto());

            await _widget.InvokeAsync(model);

            _mockViewGlobalLocationsService.Verify(
                s => s.GetGlobalLocationsBannerAsync(It.IsAny<ViewGlobalLocationsData>(), _httpContext.RequestAborted),
                Times.Once);
        }

        private static ComponentViewModel<ViewGlobalLocationsWidgetProperties> BuildModel(
            ViewGlobalLocationsWidgetProperties properties)
        {
            var model = new ComponentViewModel<ViewGlobalLocationsWidgetProperties>();
            typeof(ComponentViewModel<ViewGlobalLocationsWidgetProperties>)
                .GetProperty("Properties")
                ?.SetValue(model, properties);
            return model;
        }
    }
}