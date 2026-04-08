using BDO.GWT.Common.Core.LiveSite.Data;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ImageWithTextWidget;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget
{
    public class ImageWithTextWidgetTests
    {
        private ImageWithTextWidget _widget;

        [SetUp]
        public void SetUp()
        {
            _widget = new ImageWithTextWidget();
            _widget.ViewComponentContext = new ViewComponentContext
            {
                ViewContext = new ViewContext
                {
                    HttpContext = new DefaultHttpContext(),
                    ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                }
            };
        }

        [Test]
        public void Invoke_ReturnsViewWithAllPropertiesSet()
        {
            // Arrange
            var properties = new ImageWithTextWidgetProperties
            {
                Title = "Test Title",
                Description = "Test Description",
                ButtonText = "Click Me",
                ButtonURL = "https://example.com",
                PrimaryButtonLinkTarget = "_blank",
                ButtonType = "primary",
                MediaType = "image",
                EmbedCode = "<iframe></iframe>",
                ImageURL = "/media/test.jpg",
                ImageAltText = "Alt Text",
                MediaPosition = "right",
                BackgroundImageURL = "/media/bg.jpg",
                BackgroundImageAltText = "BG Alt",
                Theme = "dark",
                TopMargin = "20px",
                BottomMargin = "40px"
            };

            var model = new ComponentViewModel<ImageWithTextWidgetProperties>();
            var propertiesField = typeof(ComponentViewModel<ImageWithTextWidgetProperties>)
                .GetProperty("Properties");
            propertiesField?.SetValue(model, properties);

            // Act
            var result = _widget.Invoke(model);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<ViewViewComponentResult>());

            var viewResult = result as ViewViewComponentResult;
            var viewModel = viewResult?.ViewData?.Model as ImageWithTextData;

            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.Title, Is.EqualTo(properties.Title));
            Assert.That(viewModel.Description, Is.EqualTo(properties.Description));
            Assert.That(viewModel.ButtonTitle, Is.EqualTo(properties.ButtonText));
            Assert.That(viewModel.ButtonURL, Is.EqualTo(properties.ButtonURL));
            Assert.That(viewModel.PrimaryButtonLinkTarget, Is.EqualTo(properties.PrimaryButtonLinkTarget));
            Assert.That(viewModel.ButtonType, Is.EqualTo(properties.ButtonType));
            Assert.That(viewModel.MediaType, Is.EqualTo(properties.MediaType));
            Assert.That(viewModel.EmbedCode, Is.EqualTo(properties.EmbedCode));
            Assert.That(viewModel.ImageURL, Is.EqualTo(properties.ImageURL));
            Assert.That(viewModel.ImageAltText, Is.EqualTo(properties.ImageAltText));
            Assert.That(viewModel.MediaPosition, Is.EqualTo(properties.MediaPosition));
            Assert.That(viewModel.BackgroundImageURL, Is.EqualTo(properties.BackgroundImageURL));
            Assert.That(viewModel.BackgroundImageAltText, Is.EqualTo(properties.BackgroundImageAltText));
            Assert.That(viewModel.Theme, Is.EqualTo(properties.Theme));
            Assert.That(viewModel.TopMargin, Is.EqualTo(properties.TopMargin));
            Assert.That(viewModel.BottomMargin, Is.EqualTo(properties.BottomMargin));
        }

        [Test]
        public void Invoke_WithMinimalProperties_ReturnsViewWithDefaults()
        {
            // Arrange
            var properties = new ImageWithTextWidgetProperties
            {
                Title = "Minimal Title",
                Description = "Minimal Description"
            };

            var model = new ComponentViewModel<ImageWithTextWidgetProperties>();
            var propertiesField = typeof(ComponentViewModel<ImageWithTextWidgetProperties>)
                .GetProperty("Properties");
            propertiesField?.SetValue(model, properties);

            // Act
            var result = _widget.Invoke(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as ImageWithTextData;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.Title, Is.EqualTo("Minimal Title"));
            Assert.That(viewModel.Description, Is.EqualTo("Minimal Description"));
            Assert.That(viewModel.ButtonTitle, Is.Null);
            Assert.That(viewModel.ButtonURL, Is.Null);
            Assert.That(viewModel.PrimaryButtonLinkTarget, Is.EqualTo("_self")); // default
            Assert.That(viewModel.ButtonType, Is.Null);
            Assert.That(viewModel.MediaType, Is.EqualTo("image")); // default
            Assert.That(viewModel.EmbedCode, Is.EqualTo(""));
            Assert.That(viewModel.ImageURL, Is.EqualTo(""));
            Assert.That(viewModel.ImageAltText, Is.EqualTo(""));
            Assert.That(viewModel.MediaPosition, Is.EqualTo("left")); // default
            Assert.That(viewModel.BackgroundImageURL, Is.Null);
            Assert.That(viewModel.BackgroundImageAltText, Is.Null);
            Assert.That(viewModel.Theme, Is.EqualTo("light")); // default
            Assert.That(viewModel.TopMargin, Is.EqualTo("0px")); // default
            Assert.That(viewModel.BottomMargin, Is.EqualTo("0px")); // default
        }
        [Test]
        public void Invoke_WithEmbedMediaType_UsesEmbedCodeInsteadOfImageURL()
        {
            // Arrange
            var properties = new ImageWithTextWidgetProperties
            {
                Title = "Embed Title",
                Description = "Embed Description",
                MediaType = "embed",
                EmbedCode = "<iframe src='test'></iframe>",
                ImageURL = "/media/should-not-use.jpg"
            };

            var model = new ComponentViewModel<ImageWithTextWidgetProperties>();
            typeof(ComponentViewModel<ImageWithTextWidgetProperties>)
                .GetProperty("Properties")?.SetValue(model, properties);

            // Act
            var result = _widget.Invoke(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as ImageWithTextData;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.MediaType, Is.EqualTo("embed"));
            Assert.That(viewModel.EmbedCode, Is.EqualTo("<iframe src='test'></iframe>"));
            Assert.That(viewModel.ImageURL, Is.EqualTo("/media/should-not-use.jpg")); // still set, but frontend should ignore it
        }

        [Test]
        public void Invoke_WithNullBackgroundImageAltText_SetsNull()
        {
            // Arrange
            var properties = new ImageWithTextWidgetProperties
            {
                Title = "Title",
                Description = "Description",
                BackgroundImageURL = "/media/bg.jpg",
                BackgroundImageAltText = null
            };

            var model = new ComponentViewModel<ImageWithTextWidgetProperties>();
            typeof(ComponentViewModel<ImageWithTextWidgetProperties>)
                .GetProperty("Properties")?.SetValue(model, properties);

            // Act
            var result = _widget.Invoke(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as ImageWithTextData;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.BackgroundImageURL, Is.EqualTo("/media/bg.jpg"));
            Assert.That(viewModel.BackgroundImageAltText, Is.Null);
        }

        [Test]
        [TestCase("light")]
        [TestCase("dark")]
        public void Invoke_WithTheme_SetsThemeCorrectly(string theme)
        {
            // Arrange
            var properties = new ImageWithTextWidgetProperties
            {
                Title = "Title",
                Description = "Description",
                Theme = theme
            };

            var model = new ComponentViewModel<ImageWithTextWidgetProperties>();
            typeof(ComponentViewModel<ImageWithTextWidgetProperties>)
                .GetProperty("Properties")?.SetValue(model, properties);

            // Act
            var result = _widget.Invoke(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as ImageWithTextData;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.Theme, Is.EqualTo(theme));
        }

        [Test]
        public void Invoke_WithNoButton_SetsButtonFieldsNull()
        {
            // Arrange
            var properties = new ImageWithTextWidgetProperties
            {
                Title = "Title",
                Description = "Description",
                ButtonText = null,
                ButtonURL = null,
                PrimaryButtonLinkTarget = "_self"
            };

            var model = new ComponentViewModel<ImageWithTextWidgetProperties>();
            typeof(ComponentViewModel<ImageWithTextWidgetProperties>)
                .GetProperty("Properties")?.SetValue(model, properties);

            // Act
            var result = _widget.Invoke(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as ImageWithTextData;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.ButtonTitle, Is.Null);
            Assert.That(viewModel.ButtonURL, Is.Null);
            Assert.That(viewModel.PrimaryButtonLinkTarget, Is.EqualTo("_self"));
        }
    }
}