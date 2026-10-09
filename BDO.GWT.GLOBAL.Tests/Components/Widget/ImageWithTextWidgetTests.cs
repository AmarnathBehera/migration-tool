using BDO.GWT.Core.Data.ImageWithTextData;
using BDO.GWT.Core;
using BDO.GWT.Web.Widgets.ImageWithTextWidget;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
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
        private Mock<IContentService> _contentServiceMock;
        private Mock<IContentQueryExecutor> _queryExecutorMock;
        private Mock<IWebsiteChannelContext> _channelContextMock;
        private Mock<IWebPageUrlRetriever> _webPageUrlRetrieverMock;


        [SetUp]
        public void Setup()
        {
            _contentRetrieverMock = new Mock<IContentRetriever>();
            _contentServiceMock = new Mock<IContentService>();
            _queryExecutorMock = new Mock<IContentQueryExecutor>();
            _channelContextMock = new Mock<IWebsiteChannelContext>();
            _webPageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();

            _channelContextMock.SetupGet(x => x.WebsiteChannelName).Returns("BDOGlobal");
            _channelContextMock.SetupGet(x => x.IsPreview).Returns(false);
            _contentServiceMock.Setup(x => x.GetCurrentLanguage()).Returns("en-US");
            _queryExecutorMock
                .Setup(x => x.GetResult<int>(
                    It.IsAny<ContentItemQueryBuilder>(),
                    It.IsAny<Func<IContentQueryDataContainer, int>>(),
                    It.IsAny<ContentQueryExecutionOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            _widget = new ImageWithTextWidget(
                _contentRetrieverMock.Object,
                _contentServiceMock.Object,
                _queryExecutorMock.Object,
                _channelContextMock.Object,
                _webPageUrlRetrieverMock.Object);

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

            _contentServiceMock
                .Setup(x => x.RetrieveButtonUrlAsync(
                    It.Is<IEnumerable<Guid>>(g => g.Contains(guid)),
                    It.Is<string[]>(types => types.SequenceEqual(new[] { ContentAttachment.CONTENT_TYPE_NAME }))))
                .ReturnsAsync([attachment]);

            var props = new ImageWithTextWidgetProperties
            {
                Title = "Title",
                Description = "Desc",
                ButtonURL =
                [
                    new ContentItemReference { Identifier = guid }
                ]
            };

            var model = GetModel(props);
            var result = await _widget.InvokeAsync(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as ImageWithTextData;

            _contentServiceMock.Verify(x => x.RetrieveButtonUrlAsync(
                It.Is<IEnumerable<Guid>>(g => g.Contains(guid)),
                It.Is<string[]>(types => types.SequenceEqual(new[] { ContentAttachment.CONTENT_TYPE_NAME }))), Times.Once);

            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel!.ButtonURL, Is.EqualTo("/button.pdf"));
        }

        [Test]
        public async Task InvokeAsync_WithManualUrl_UsesManualUrlWithoutResolvingContent()
        {
            var props = new ImageWithTextWidgetProperties
            {
                Title = "Title",
                Description = "Desc",
                UrlSource = "manual",
                ManualUrl = "https://example.com/manual"
            };

            var model = GetModel(props);
            var result = await _widget.InvokeAsync(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as ImageWithTextData;

            _queryExecutorMock.Verify(x => x.GetResult<int>(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IContentQueryDataContainer, int>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()), Times.Never);
            _webPageUrlRetrieverMock.Verify(x => x.Retrieve(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            _contentServiceMock.Verify(x => x.RetrieveButtonUrlAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<string[]>()), Times.Never);

            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel!.ButtonURL, Is.EqualTo("https://example.com/manual"));
        }

        [Test]
        public async Task InvokeAsync_WithPageButtonUrl_DoesNotFallbackToAttachment()
        {
            var guid = Guid.NewGuid();

            _queryExecutorMock
                .Setup(x => x.GetResult<int>(
                    It.IsAny<ContentItemQueryBuilder>(),
                    It.IsAny<Func<IContentQueryDataContainer, int>>(),
                    It.IsAny<ContentQueryExecutionOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([123]);

            _webPageUrlRetrieverMock
                .Setup(x => x.Retrieve(123, "en-US"))
                .ReturnsAsync(new WebPageUrl("~/section/test-page", "https://example.com/section/test-page"));

            var props = new ImageWithTextWidgetProperties
            {
                Title = "Title",
                Description = "Desc",
                ButtonURL =
                [
                    new ContentItemReference { Identifier = guid }
                ]
            };

            var model = GetModel(props);
            var result = await _widget.InvokeAsync(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as ImageWithTextData;

            _contentServiceMock.Verify(x => x.RetrieveButtonUrlAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<string[]>()), Times.Never);

            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel!.ButtonURL, Is.EqualTo("/section/test-page"));
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

        [Test]
        public async Task InvokeAsync_WithPage_ButtonUrl()
        {
            var guid = Guid.NewGuid();

            _queryExecutorMock
                .Setup(x => x.GetResult<int>(
                    It.IsAny<ContentItemQueryBuilder>(),
                    It.IsAny<Func<IContentQueryDataContainer, int>>(),
                    It.IsAny<ContentQueryExecutionOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([123]);

            _webPageUrlRetrieverMock
                .Setup(x => x.Retrieve(123, "en-US"))
                .ReturnsAsync(new WebPageUrl("~/section/test-page", "https://example.com/section/test-page"));

            var props = new ImageWithTextWidgetProperties
            {
                Title = "Title",
                Description = "Desc",
                ButtonURL =
                [
                    new ContentItemReference { Identifier = guid }
                ]
            };

            var model = GetModel(props);
            var result = await _widget.InvokeAsync(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as ImageWithTextData;

            _contentServiceMock.Verify(x => x.GetCurrentLanguage(), Times.Exactly(2));
            _queryExecutorMock.Verify(x => x.GetResult<int>(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IContentQueryDataContainer, int>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
            _webPageUrlRetrieverMock.Verify(x => x.Retrieve(123, "en-US"), Times.Once);
            _contentServiceMock.Verify(x => x.RetrieveButtonUrlAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<string[]>()), Times.Never);

            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel!.ButtonURL, Is.EqualTo("/section/test-page"));
        }
    }
}