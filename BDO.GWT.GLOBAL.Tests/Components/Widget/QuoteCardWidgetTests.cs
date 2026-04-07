using BDO.GWT.Common.Core.LiveSite.Data;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.QuoteCardWidget;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget
{
    public class QuoteCardWidgetTests
    {
        private QuoteCardWidget _widget;

        [SetUp]
        public void SetUp()
        {
            _widget = new QuoteCardWidget();

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
        public void Invoke_ReturnsViewWithCorrectData()
        {
            // Arrange
            var properties = new QuoteCardWidgetProperties
            {
                Heading = "Test Heading",
                Subheading = "Test Subheading",
                Image = "/media/test.jpg",
                ImageAltText = "Alt Text",
                QuoteText = "This is a test quote",
                QuotesAuthor = "John Doe",
                AuthorsJobTitle = "CEO",
                ButtonText = "Click Here",
                ButtonUrl = "https://example.com",
                PrimaryButtonLinkTarget = "_blank",
                Size = "large",
                TopMargin = "20px",
                BottomMargin = "40px"
            };

            var model = new ComponentViewModel<QuoteCardWidgetProperties>();
            var propertiesField = typeof(ComponentViewModel<QuoteCardWidgetProperties>)
                .GetProperty("Properties");
            propertiesField?.SetValue(model, properties);

            // Act
            var result = _widget.Invoke(model);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<ViewViewComponentResult>());

            var viewResult = result as ViewViewComponentResult;
            var viewModel = viewResult?.ViewData?.Model as QuoteCardData;

            Assert.That(viewModel, Is.Not.Null);

            // Margins
            Assert.That(viewModel.TopMargin, Is.EqualTo("20px"));
            Assert.That(viewModel.BottomMargin, Is.EqualTo("40px"));

            // Quote data
            Assert.That(viewModel.Data.Heading, Is.EqualTo("Test Heading"));
            Assert.That(viewModel.Data.SubHeading, Is.EqualTo("Test Subheading"));
            Assert.That(viewModel.Data.ImageURL, Is.EqualTo("/media/test.jpg"));
            Assert.That(viewModel.Data.ImageAltText, Is.EqualTo("Alt Text"));
            Assert.That(viewModel.Data.QuoteText, Is.EqualTo("This is a test quote"));
            Assert.That(viewModel.Data.QuotesAuthor, Is.EqualTo("John Doe"));
            Assert.That(viewModel.Data.AuthorsJobTitle, Is.EqualTo("CEO"));
            Assert.That(viewModel.Data.Size, Is.EqualTo("large"));

            // Button
            Assert.That(viewModel.Data.Button.Title, Is.EqualTo("Click Here"));
            Assert.That(viewModel.Data.Button.URL, Is.EqualTo("https://example.com"));
            Assert.That(viewModel.Data.Button.Target, Is.EqualTo("_blank"));
        }

        [Test]
        public void Invoke_WithMinimalProperties_ReturnsViewWithDefaults()
        {
            // Arrange
            var properties = new QuoteCardWidgetProperties
            {
                QuoteText = "Minimal quote",
                QuotesAuthor = "Jane Doe",
                ButtonText = "Go",
                ButtonUrl = "https://test.com",
                TopMargin = "0px",
                BottomMargin = "0px"
            };

            var model = new ComponentViewModel<QuoteCardWidgetProperties>();
            var propertiesField = typeof(ComponentViewModel<QuoteCardWidgetProperties>)
                .GetProperty("Properties");
            propertiesField?.SetValue(model, properties);

            // Act
            var result = _widget.Invoke(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as QuoteCardData;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.Data.Heading, Is.Null);
            Assert.That(viewModel.Data.SubHeading, Is.Null);
            Assert.That(viewModel.Data.ImageURL, Is.Null);
            Assert.That(viewModel.Data.ImageAltText, Is.Null);
            Assert.That(viewModel.Data.Size, Is.EqualTo("standard"));
        }
        [Test]
        public void Invoke_WithNoButton_SetsButtonDefaults()
        {
            // Arrange
            var properties = new QuoteCardWidgetProperties
            {
                QuoteText = "No button quote",
                QuotesAuthor = "Author",
                TopMargin = "10px",
                BottomMargin = "10px"
            };

            var model = new ComponentViewModel<QuoteCardWidgetProperties>();
            typeof(ComponentViewModel<QuoteCardWidgetProperties>)
                .GetProperty("Properties")?.SetValue(model, properties);

            // Act
            var result = _widget.Invoke(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as QuoteCardData;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.Data.Button.Title, Is.Null);
            Assert.That(viewModel.Data.Button.URL, Is.Null);
            Assert.That(viewModel.Data.Button.Target, Is.EqualTo("_self")); // default target
        }

        [Test]
        [TestCase("small")]
        [TestCase("standard")]
        [TestCase("large")]
        public void Invoke_WithDifferentSizes_SetsSizeCorrectly(string size)
        {
            // Arrange
            var properties = new QuoteCardWidgetProperties
            {
                QuoteText = "Sized quote",
                QuotesAuthor = "Author",
                Size = size,
                TopMargin = "0px",
                BottomMargin = "0px"
            };

            var model = new ComponentViewModel<QuoteCardWidgetProperties>();
            typeof(ComponentViewModel<QuoteCardWidgetProperties>)
                .GetProperty("Properties")?.SetValue(model, properties);

            // Act
            var result = _widget.Invoke(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as QuoteCardData;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.Data.Size, Is.EqualTo(size));
        }

        [Test]
        public void Invoke_WithNullOptionalFields_DoesNotBreak()
        {
            // Arrange
            var properties = new QuoteCardWidgetProperties
            {
                QuoteText = "Quote",
                QuotesAuthor = "Author",
                Heading = null,
                Subheading = null,
                Image = null,
                ImageAltText = null,
                AuthorsJobTitle = null,
                ButtonText = null,
                ButtonUrl = null,
                TopMargin = "0px",
                BottomMargin = "0px"
            };

            var model = new ComponentViewModel<QuoteCardWidgetProperties>();
            typeof(ComponentViewModel<QuoteCardWidgetProperties>)
                .GetProperty("Properties")?.SetValue(model, properties);

            // Act
            var result = _widget.Invoke(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as QuoteCardData;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.Data.Heading, Is.Null);
            Assert.That(viewModel.Data.SubHeading, Is.Null);
            Assert.That(viewModel.Data.ImageURL, Is.Null);
            Assert.That(viewModel.Data.ImageAltText, Is.Null);
            Assert.That(viewModel.Data.AuthorsJobTitle, Is.Null);
            Assert.That(viewModel.Data.Button.Title, Is.Null);
            Assert.That(viewModel.Data.Button.URL, Is.Null);
        }
    }
}