using BDO.GWT.Common.Core.LiveSite.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services.Widget;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.QuoteCardWidget;
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
    public class QuoteCardWidgetTests
    {
        private Mock<IContentRetriever> _contentRetrieverMock;
        private Mock<IButtonUrlRetriever> _buttonUrlRetrieverMock;
        private QuoteCardWidget _widget;
        private DefaultHttpContext _httpContext;

        [SetUp]
        public void SetUp()
        {
            _contentRetrieverMock = new Mock<IContentRetriever>();
            _buttonUrlRetrieverMock = new Mock<IButtonUrlRetriever>();

            _widget = new QuoteCardWidget(
                _contentRetrieverMock.Object,
                _buttonUrlRetrieverMock.Object
            );

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

        private static ComponentViewModel<QuoteCardWidgetProperties> BuildModel(QuoteCardWidgetProperties properties)
        {
            var model = new ComponentViewModel<QuoteCardWidgetProperties>();
            typeof(ComponentViewModel<QuoteCardWidgetProperties>)
                .GetProperty("Properties")?.SetValue(model, properties);
            return model;
        }

        [Test]
        public async Task InvokeAsync_WithMinimalProperties_MapsFieldsCorrectly()
        {
            var properties = new QuoteCardWidgetProperties
            {
                QuoteText = "A great quote.",
                QuotesAuthor = "John Doe"
            };

            _buttonUrlRetrieverMock
                .Setup(x => x.GetTilePagesAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LinkItemDto>());

            var result = await _widget.InvokeAsync(BuildModel(properties));
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as QuoteCardData;

            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.Data.QuoteText, Is.EqualTo("A great quote."));
                Assert.That(viewModel!.Data.QuotesAuthor, Is.EqualTo("John Doe"));
            });
        }

        [Test]
        public async Task InvokeAsync_WithAllProperties_SetsEverythingCorrectly()
        {
            var properties = new QuoteCardWidgetProperties
            {
                Heading = "Test Heading",
                Subheading = "<p>Test Subheading</p>",
                ImageAltText = "Alt text",
                QuoteText = "Quote body",
                QuotesAuthor = "Jane Smith",
                AuthorsJobTitle = "CEO",
                ButtonText = "Learn More",
                PrimaryButtonLinkTarget = "_blank",
                Size = "large",
                TopMargin = "40px",
                BottomMargin = "20px",
                ButtonUrl = new List<ContentItemReference>(),
                Image = new List<ContentItemReference>()
            };

            _buttonUrlRetrieverMock
                .Setup(x => x.GetTilePagesAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LinkItemDto>());

            var result = await _widget.InvokeAsync(BuildModel(properties));
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as QuoteCardData;

            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.Data.Heading, Is.EqualTo("Test Heading"));
                Assert.That(viewModel!.Data.SubHeading, Is.EqualTo("<p>Test Subheading</p>"));
                Assert.That(viewModel!.Data.ImageAltText, Is.EqualTo("Alt text"));
                Assert.That(viewModel!.Data.QuoteText, Is.EqualTo("Quote body"));
                Assert.That(viewModel!.Data.QuotesAuthor, Is.EqualTo("Jane Smith"));
                Assert.That(viewModel!.Data.AuthorsJobTitle, Is.EqualTo("CEO"));
                Assert.That(viewModel!.Data.Button.Title, Is.EqualTo("Learn More"));
                Assert.That(viewModel!.Data.Button.Target, Is.EqualTo("_blank"));
                Assert.That(viewModel!.Data.Size, Is.EqualTo("large"));
                Assert.That(viewModel!.TopMargin, Is.EqualTo("40px"));
                Assert.That(viewModel!.BottomMargin, Is.EqualTo("20px"));
            });
        }

        [Test]
        public async Task InvokeAsync_WithButtonUrlReference_ResolvesButtonUrl()
        {
            var guid = Guid.NewGuid();
            var expectedUrl = "/about-us";

            var properties = new QuoteCardWidgetProperties
            {
                QuoteText = "Quote",
                QuotesAuthor = "Author",
                ButtonText = "Click here",
                ButtonUrl = new List<ContentItemReference>
                {
                    new ContentItemReference { Identifier = guid }
                }
            };

            _buttonUrlRetrieverMock
                .Setup(x => x.GetTilePagesAsync(
                    It.Is<List<Guid>>(ids => ids.Contains(guid)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LinkItemDto> { new LinkItemDto { URL = expectedUrl } });

            var result = await _widget.InvokeAsync(BuildModel(properties));
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as QuoteCardData;

            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.Data.Button.URL, Is.EqualTo(expectedUrl));
            });
        }

        [Test]
        public async Task InvokeAsync_WithEmptyButtonUrl_DoesNotCallRetrieverAndButtonUrlIsNull()
        {
            var properties = new QuoteCardWidgetProperties
            {
                QuoteText = "Quote",
                QuotesAuthor = "Author",
                ButtonText = "Click",
                ButtonUrl = new List<ContentItemReference>()
            };

            var result = await _widget.InvokeAsync(BuildModel(properties));
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as QuoteCardData;

            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.Data.Button.URL, Is.Null);
            });
            _buttonUrlRetrieverMock.Verify(
                x => x.GetTilePagesAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task InvokeAsync_WithButtonUrlContainingEmptyGuid_DoesNotCallRetriever()
        {
            var properties = new QuoteCardWidgetProperties
            {
                QuoteText = "Quote",
                QuotesAuthor = "Author",
                ButtonUrl = new List<ContentItemReference>
                {
                    new ContentItemReference { Identifier = Guid.Empty }
                }
            };

            var result = await _widget.InvokeAsync(BuildModel(properties));
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as QuoteCardData;

            Assert.That(viewModel, Is.Not.Null);
            _buttonUrlRetrieverMock.Verify(
                x => x.GetTilePagesAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task InvokeAsync_WithNoImage_ImageUrlIsNull()
        {
            var properties = new QuoteCardWidgetProperties
            {
                QuoteText = "Quote",
                QuotesAuthor = "Author",
                Image = new List<ContentItemReference>()
            };

            var result = await _widget.InvokeAsync(BuildModel(properties));
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as QuoteCardData;

            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.Data.ImageURL, Is.Null);
        }

        [Test]
        public async Task InvokeAsync_ReturnsCorrectViewName()
        {
            var properties = new QuoteCardWidgetProperties
            {
                QuoteText = "Quote",
                QuotesAuthor = "Author"
            };

            var result = await _widget.InvokeAsync(BuildModel(properties));
            var viewResult = result as ViewViewComponentResult;

            Assert.That(viewResult, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewResult!.ViewName, Is.EqualTo("~/Components/Widget/QuoteCardWidget/_QuoteCardWidget.cshtml"));
            });
        }

        [Test]
        public async Task InvokeAsync_DefaultSizeAndTarget_ArePreserved()
        {
            var properties = new QuoteCardWidgetProperties
            {
                QuoteText = "Quote",
                QuotesAuthor = "Author"
            };

            var result = await _widget.InvokeAsync(BuildModel(properties));
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as QuoteCardData;

            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.Data.Size, Is.EqualTo("standard"));
                Assert.That(viewModel!.Data.Button.Target, Is.EqualTo("_self"));
            });
        }
    }
}
