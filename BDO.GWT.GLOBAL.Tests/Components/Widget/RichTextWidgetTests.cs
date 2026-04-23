using BDO.GWT.Web.Widgets.RichTextWidget;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget
{
    public class RichTextWidgetTests
    {
        private RichTextWidget _widget;

        [SetUp]
        public void SetUp()
        {
            _widget = new RichTextWidget();

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

        private static ComponentViewModel<RichTextWidgetProperties> CreateModel(RichTextWidgetProperties properties)
        {
            var model = new ComponentViewModel<RichTextWidgetProperties>();
            typeof(ComponentViewModel<RichTextWidgetProperties>)
                .GetProperty("Properties")?.SetValue(model, properties);
            return model;
        }

        [Test]
        public void Invoke_WithAllProperties_ReturnsViewWithModel()
        {
            // Arrange
            var properties = new RichTextWidgetProperties
            {
                WidgetID = "rich-text-1",
                TopMargin = "20px",
                BottomMargin = "40px"
            };
            var model = CreateModel(properties);

            // Act
            var result = _widget.Invoke(model);
            var viewResult = result as ViewViewComponentResult;
            var viewModel = viewResult?.ViewData?.Model as ComponentViewModel<RichTextWidgetProperties>;

            // Assert
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.Properties.WidgetID, Is.EqualTo("rich-text-1"));
                Assert.That(viewModel!.Properties.TopMargin, Is.EqualTo("20px"));
                Assert.That(viewModel!.Properties.BottomMargin, Is.EqualTo("40px"));
            });
        }

        [Test]
        public void Invoke_WithMinimalProperties_ReturnsViewWithDefaults()
        {
            // Arrange
            var properties = new RichTextWidgetProperties();
            var model = CreateModel(properties);

            // Act
            var result = _widget.Invoke(model);
            var viewResult = result as ViewViewComponentResult;
            var viewModel = viewResult?.ViewData?.Model as ComponentViewModel<RichTextWidgetProperties>;

            // Assert
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.Properties.WidgetID, Is.Null);
                Assert.That(viewModel!.Properties.TopMargin, Is.Null);
                Assert.That(viewModel!.Properties.BottomMargin, Is.Null);
            });
        }

        [Test]
        public void Invoke_ReturnsCorrectViewPath()
        {
            // Arrange
            var model = CreateModel(new RichTextWidgetProperties());

            // Act
            var result = _widget.Invoke(model);
            var viewResult = result as ViewViewComponentResult;

            // Assert
            Assert.That(viewResult, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewResult!.ViewName, Is.EqualTo("~/Components/Widget/RichTextWidget/_RichTextWidget.cshtml"));
            });
        }

        [Test]
        public void Invoke_ReturnsSameModelInstance()
        {
            // Arrange
            var properties = new RichTextWidgetProperties
            {
                WidgetID = "same-instance-id",
                TopMargin = "60px",
                BottomMargin = "0px"
            };
            var model = CreateModel(properties);

            // Act
            var result = _widget.Invoke(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model;

            // Assert
            Assert.That(viewModel, Is.SameAs(model));
        }
    }
}
