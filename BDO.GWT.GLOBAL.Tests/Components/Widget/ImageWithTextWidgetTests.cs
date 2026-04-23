using BDO.GWT.Common.Core.LiveSite.Data;
using BDO.GWT.Core.Entities;
using BDO.GWT.GLOBAL.Components.Widget.ImageWithTextWidget;
using CMS.ContentEngine;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget
{
    public class ImageWithTextWidgetTests
    {
        private Mock<IContentRetriever> _contentRetrieverMock;
        private ImageWithTextWidget _widget;
        private Mock<IContentRetrieverWrapper> _mockWrapper;


        [SetUp]
        public void Setup()
        {
            _contentRetrieverMock = new Mock<IContentRetriever>();
            _mockWrapper = new Mock<IContentRetrieverWrapper>();

            _widget = new ImageWithTextWidget(_contentRetrieverMock.Object, _mockWrapper.Object);

            _widget.ViewComponentContext = new ViewComponentContext
            {
                ViewContext = new ViewContext
                {
                    HttpContext = new DefaultHttpContext(),
                    ViewData = new ViewDataDictionary(
                        new EmptyModelMetadataProvider(),
                        new ModelStateDictionary())
                }
            };
        }

        private static ComponentViewModel<ImageWithTextWidgetProperties> GetModel(ImageWithTextWidgetProperties props)
        {
            var model = new ComponentViewModel<ImageWithTextWidgetProperties>();

            typeof(ComponentViewModel<ImageWithTextWidgetProperties>)
                .GetProperty("Properties")
                ?.SetValue(model, props);

            return model;
        }

        [Test]
        public async Task InvokeAsync_ReturnsView_WithBasicProperties()
        {
            var props = new ImageWithTextWidgetProperties
            {
                Title = "Test Title",
                Description = "Test Description",
                ButtonText = "Click Me"
            };

            var model = GetModel(props);

            var result = await _widget.InvokeAsync(model);

            Assert.That(result, Is.TypeOf<ViewViewComponentResult>());

            var viewResult = result as ViewViewComponentResult;
            var viewModel = viewResult?.ViewData?.Model as ImageWithTextData;

            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.Title, Is.EqualTo("Test Title"));
                Assert.That(viewModel!.Description, Is.EqualTo("Test Description"));
                Assert.That(viewModel!.ButtonTitle, Is.EqualTo("Click Me"));
            });
        }

        [Test]
        public async Task InvokeAsync_WithContentAttachment_ButtonUrl()
        {
            var guid = Guid.NewGuid();

            var attachment = new ContentAttachment
            {
                ContentAttachmentAsset = new ContentItemAsset
                {
                    Url = "/button.pdf"
                }
            };

            _mockWrapper
                .Setup(x => x.RetrieveButtonUrlAsync(
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<string[]>()))
                .ReturnsAsync(new List<IContentItemFieldsSource>
                {
            attachment
                });

            var props = new ImageWithTextWidgetProperties
            {
                Title = "Title",
                Description = "Desc",
                ButtonURL = new List<ContentItemReference>
        {
            new ContentItemReference { Identifier = guid }
        }
            };

            var model = GetModel(props);

            var result = await _widget.InvokeAsync(model);

            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as ImageWithTextData;

            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.ButtonURL, Is.EqualTo("/button.pdf"));
        }

        [Test]
        public async Task InvokeAsync_WithNoButton_ReturnsNull()
        {
            var props = new ImageWithTextWidgetProperties
            {
                Title = "Title",
                Description = "Description"
            };

            var model = GetModel(props);

            var result = await _widget.InvokeAsync(model);

            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as ImageWithTextData;

            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.ButtonURL, Is.Null);
        }

        [Test]
        public async Task InvokeAsync_WithEmbedMediaType()
        {
            var props = new ImageWithTextWidgetProperties
            {
                Title = "Embed Title",
                Description = "Embed Description",
                MediaType = "embed",
                EmbedCode = "<iframe></iframe>"
            };

            var model = GetModel(props);

            var result = await _widget.InvokeAsync(model);

            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as ImageWithTextData;

            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.MediaType, Is.EqualTo("embed"));
                Assert.That(viewModel!.EmbedCode, Is.EqualTo("<iframe></iframe>"));
            });
        }

        [Test]
        public async Task InvokeAsync_DefaultValues()
        {
            var props = new ImageWithTextWidgetProperties
            {
                Title = "Title",
                Description = "Description"
            };

            var model = GetModel(props);

            var result = await _widget.InvokeAsync(model);

            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as ImageWithTextData;

            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.Theme, Is.EqualTo("light"));
                Assert.That(viewModel!.TopMargin, Is.EqualTo("0px"));
                Assert.That(viewModel!.BottomMargin, Is.EqualTo("0px"));
            });
        }
    }
}