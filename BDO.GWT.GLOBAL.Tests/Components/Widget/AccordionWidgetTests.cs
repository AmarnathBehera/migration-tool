using BDO.GWT.Core.Data;
using BDO.GWT.Web.Widgets.AccordionWidget;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget
{
    public class AccordionWidgetTests
    {
        private AccordionWidget _widget;

        [SetUp]
        public void SetUp()
        {
            _widget = new AccordionWidget();

            _widget.ViewComponentContext = new ViewComponentContext
            {
                ViewContext = new ViewContext
                {
                    HttpContext = new DefaultHttpContext(),
                    ViewData = new ViewDataDictionary(
                        new EmptyModelMetadataProvider(),
                        new ModelStateDictionary()
                    )
                }
            };
        }

        private static ComponentViewModel<AccordionWidgetProperties> CreateModel(AccordionWidgetProperties properties)
        {
            var model = new ComponentViewModel<AccordionWidgetProperties>();
            typeof(ComponentViewModel<AccordionWidgetProperties>)
                .GetProperty("Properties")?.SetValue(model, properties);
            return model;
        }

        [Test]
        public void Invoke_WithAllProperties_MapsAllFieldsCorrectly()
        {
            // Arrange
            var properties = new AccordionWidgetProperties
            {
                Heading = "Test Heading",
                Content = "<p>Test Content</p>",
                DeepLinkID = "test-id",
                TopMargin = "20px",
                BottomMargin = "40px",
                WidgetID = "accordion-widget-1"
            };
            var model = CreateModel(properties);

            // Act
            var result = _widget.Invoke(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as AccordionData;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.TopMargin, Is.EqualTo("20px"));
                Assert.That(viewModel!.BottomMargin, Is.EqualTo("40px"));
                Assert.That(viewModel!.WidgetID, Is.EqualTo("accordion-widget-1"));

                var item = viewModel!.AccordionItems.Single();
                Assert.That(item.Title, Is.EqualTo("Test Heading"));
                Assert.That(item.Content, Is.EqualTo("<p>Test Content</p>"));
                Assert.That(item.Id, Is.EqualTo("test-id"));
            });
        }

        [Test]
        public void Invoke_WithMinimalProperties_ReturnsViewWithDefaults()
        {
            // Arrange
            var properties = new AccordionWidgetProperties
            {
                Heading = "Minimal Heading",
                Content = "Minimal Content"
            };
            var model = CreateModel(properties);

            // Act
            var result = _widget.Invoke(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as AccordionData;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.TopMargin, Is.Null);
                Assert.That(viewModel!.BottomMargin, Is.Null);
                Assert.That(viewModel!.WidgetID, Is.Null);

                var item = viewModel!.AccordionItems.Single();
                Assert.That(item.Title, Is.EqualTo("Minimal Heading"));
                Assert.That(item.Content, Is.EqualTo("Minimal Content"));
                Assert.That(item.Id, Is.Null);
            });
        }

        [Test]
        public void Invoke_ReturnsExactlyOneAccordionItem()
        {
            // Arrange
            var properties = new AccordionWidgetProperties
            {
                Heading = "Single Item Heading",
                Content = "Single Item Content"
            };
            var model = CreateModel(properties);

            // Act
            var result = _widget.Invoke(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as AccordionData;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.AccordionItems.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Invoke_WithDeepLinkID_SetsItemId()
        {
            // Arrange
            var properties = new AccordionWidgetProperties
            {
                Heading = "Heading",
                Content = "Content",
                DeepLinkID = "my-anchor"
            };
            var model = CreateModel(properties);

            // Act
            var result = _widget.Invoke(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as AccordionData;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.AccordionItems.Single().Id, Is.EqualTo("my-anchor"));
        }

        [Test]
        public void Invoke_WithNullDeepLinkID_SetsItemIdToNull()
        {
            // Arrange
            var properties = new AccordionWidgetProperties
            {
                Heading = "Heading",
                Content = "Content",
                DeepLinkID = null
            };
            var model = CreateModel(properties);

            // Act
            var result = _widget.Invoke(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as AccordionData;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.AccordionItems.Single().Id, Is.Null);
        }

        [Test]
        public void Invoke_ReturnsViewViewComponentResult()
        {
            // Arrange
            var properties = new AccordionWidgetProperties
            {
                Heading = "Heading",
                Content = "Content"
            };
            var model = CreateModel(properties);

            // Act
            var result = _widget.Invoke(model);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<ViewViewComponentResult>());
        }

        [Test]
        [TestCase("0px")]
        [TestCase("20px")]
        [TestCase("40px")]
        [TestCase("60px")]
        [TestCase("80px")]
        public void Invoke_WithTopMargin_SetsTopMarginCorrectly(string topMargin)
        {
            // Arrange
            var properties = new AccordionWidgetProperties
            {
                Heading = "Heading",
                Content = "Content",
                TopMargin = topMargin
            };
            var model = CreateModel(properties);

            // Act
            var result = _widget.Invoke(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as AccordionData;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.TopMargin, Is.EqualTo(topMargin));
        }

        [Test]
        [TestCase("0px")]
        [TestCase("20px")]
        [TestCase("40px")]
        [TestCase("60px")]
        [TestCase("80px")]
        public void Invoke_WithBottomMargin_SetsBottomMarginCorrectly(string bottomMargin)
        {
            // Arrange
            var properties = new AccordionWidgetProperties
            {
                Heading = "Heading",
                Content = "Content",
                BottomMargin = bottomMargin
            };
            var model = CreateModel(properties);

            // Act
            var result = _widget.Invoke(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as AccordionData;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.BottomMargin, Is.EqualTo(bottomMargin));
        }

        [Test]
        public void Invoke_WithRichTextContent_SetsContentCorrectly()
        {
            // Arrange
            var richContent = "<h2>Rich Text</h2><p>Paragraph with <strong>bold</strong> text.</p>";
            var properties = new AccordionWidgetProperties
            {
                Heading = "Rich Heading",
                Content = richContent
            };
            var model = CreateModel(properties);

            // Act
            var result = _widget.Invoke(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as AccordionData;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.AccordionItems.Single().Content, Is.EqualTo(richContent));
        }

        [Test]
        public void Invoke_WithWidgetID_SetsWidgetIDCorrectly()
        {
            // Arrange
            var properties = new AccordionWidgetProperties
            {
                Heading = "Heading",
                Content = "Content",
                WidgetID = "custom-widget-id"
            };
            var model = CreateModel(properties);

            // Act
            var result = _widget.Invoke(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as AccordionData;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.WidgetID, Is.EqualTo("custom-widget-id"));
        }
    }
}
