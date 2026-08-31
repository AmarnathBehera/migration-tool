using BDO.GWT.Core.Data;
using BDO.GWT.Core.Services.Widget;
using BDO.GWT.GLOBAL.Widgets;
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

        [SetUp]
        public void SetUp()
        {
            _mockViewGlobalLocationsService = new Mock<IViewGlobalLocationsService>();
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
        public async Task InvokeAsync_ReturnsViewWithCorrectData()
        {
            // Arrange
            var model = new ComponentViewModel<ViewGlobalLocationsWidgetProperties>();
            var propertiesField = typeof(ComponentViewModel<ViewGlobalLocationsWidgetProperties>)
                .GetProperty("Properties");
            propertiesField?.SetValue(model, new ViewGlobalLocationsWidgetProperties
            {
                Title = "Test Title",
                Description = "Test Description",
                Theme = "dark",
                Layout = "grid",
                TopMargin = "10px",
                BottomMargin = "20px",
                ButtonText = "Click Me",
                ButtonUrl = "https://example.com"
            });

            // Act
            var result = await _widget.InvokeAsync(model);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<ViewViewComponentResult>());

            var viewResult = result as ViewViewComponentResult;
            var viewModel = viewResult?.ViewData?.Model as ViewGlobalLocationsData;

            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.Title, Is.EqualTo(model.Properties.Title));
            Assert.That(viewModel.Description, Is.EqualTo(model.Properties.Description));
            Assert.That(viewModel.Theme, Is.EqualTo(model.Properties.Theme));
            Assert.That(viewModel.Layout, Is.EqualTo(model.Properties.Layout));
            Assert.That(viewModel.TopMargin, Is.EqualTo(model.Properties.TopMargin));
            Assert.That(viewModel.BottomMargin, Is.EqualTo(model.Properties.BottomMargin));

            Assert.That(viewModel.Button, Is.Not.Null);
            Assert.That(viewModel.Button.Title, Is.EqualTo(model.Properties.ButtonText));
            Assert.That(viewModel.Button.URL, Is.EqualTo(model.Properties.ButtonUrl));
            Assert.That(viewModel.Button.Target, Is.EqualTo(model.Properties.ButtonUrl));
            Assert.That(viewModel.Button.Type, Is.EqualTo("secondary"));

            Assert.That(viewModel.BackgroundImage, Is.EqualTo("~/images/globalviewimage.jpg"));
        }
        [Test]
        public async Task InvokeAsync_WithLightTheme_ReturnsViewModelWithLightTheme()
        {
            var model = new ComponentViewModel<ViewGlobalLocationsWidgetProperties>();
            var propertiesField = typeof(ComponentViewModel<ViewGlobalLocationsWidgetProperties>)
                .GetProperty("Properties");
            propertiesField?.SetValue(model, new ViewGlobalLocationsWidgetProperties
            {
                Title = "Light Theme",
                Theme = "light"
            });

            var result = await _widget.InvokeAsync(model);
            var viewResult = result as ViewViewComponentResult;
            var viewModel = viewResult?.ViewData?.Model as ViewGlobalLocationsData;

            Assert.That(viewModel?.Theme, Is.EqualTo("light"));
        }
        [Test]
        public async Task InvokeAsync_WithoutButtonProperties_SetsButtonFieldsToNullOrDefaults()
        {
            var model = new ComponentViewModel<ViewGlobalLocationsWidgetProperties>();
            var propertiesField = typeof(ComponentViewModel<ViewGlobalLocationsWidgetProperties>)
                .GetProperty("Properties");
            propertiesField?.SetValue(model, new ViewGlobalLocationsWidgetProperties
            {
                Title = "No Button Test"
            });

            var result = await _widget.InvokeAsync(model);
            var viewResult = result as ViewViewComponentResult;
            var viewModel = viewResult?.ViewData?.Model as ViewGlobalLocationsData;

            Assert.That(viewModel?.Button, Is.Not.Null);
            Assert.That(viewModel.Button.Title, Is.Null);
            Assert.That(viewModel.Button.URL, Is.Null);
            Assert.That(viewModel.Button.Target, Is.Null);
            Assert.That(viewModel.Button.Type, Is.EqualTo("secondary"));
        }
        [Test]
        public async Task InvokeAsync_WithCustomMargins_ReturnsCorrectMargins()
        {
            var model = new ComponentViewModel<ViewGlobalLocationsWidgetProperties>();
            var propertiesField = typeof(ComponentViewModel<ViewGlobalLocationsWidgetProperties>)
                .GetProperty("Properties");
            propertiesField?.SetValue(model, new ViewGlobalLocationsWidgetProperties
            {
                TopMargin = "40px",
                BottomMargin = "60px"
            });

            var result = await _widget.InvokeAsync(model);
            var viewResult = result as ViewViewComponentResult;
            var viewModel = viewResult?.ViewData?.Model as ViewGlobalLocationsData;

            Assert.That(viewModel?.TopMargin, Is.EqualTo("40px"));
            Assert.That(viewModel.BottomMargin, Is.EqualTo("60px"));
        }
        [Test]
        public async Task InvokeAsync_DefaultBackgroundImage_IsSetCorrectly()
        {
            var model = new ComponentViewModel<ViewGlobalLocationsWidgetProperties>();
            var propertiesField = typeof(ComponentViewModel<ViewGlobalLocationsWidgetProperties>)
                .GetProperty("Properties");
            propertiesField?.SetValue(model, new ViewGlobalLocationsWidgetProperties());

            var result = await _widget.InvokeAsync(model);
            var viewResult = result as ViewViewComponentResult;
            var viewModel = viewResult?.ViewData?.Model as ViewGlobalLocationsData;

            Assert.That(viewModel?.BackgroundImage, Is.EqualTo("~/images/globalviewimage.jpg"));
        }
        [TestCase("dark")]
        [TestCase("light")]
        public async Task InvokeAsync_ThemeTest(string theme)
        {
            var model = new ComponentViewModel<ViewGlobalLocationsWidgetProperties>();
            var propertiesField = typeof(ComponentViewModel<ViewGlobalLocationsWidgetProperties>)
                .GetProperty("Properties");
            propertiesField?.SetValue(model, new ViewGlobalLocationsWidgetProperties
            {
                Theme = theme
            });

            var result = await _widget.InvokeAsync(model);
            var viewResult = result as ViewViewComponentResult;
            var viewModel = viewResult?.ViewData?.Model as ViewGlobalLocationsData;

            Assert.That(viewModel?.Theme, Is.EqualTo(theme));
        }
    }
}