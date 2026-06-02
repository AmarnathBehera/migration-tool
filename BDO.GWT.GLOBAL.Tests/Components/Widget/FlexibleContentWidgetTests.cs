using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services.Widget;
using BDO.GWT.Web.Widgets.FlexibleContentWidget;
using CMS.ContentEngine;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget
{
    public class FlexibleContentWidgetTests
    {
        private Mock<IFlexibleContentService> _flexibleContentServiceMock;
        private FlexibleContentWidget _widget;
        private DefaultHttpContext _httpContext;

        [SetUp]
        public void SetUp()
        {
            _flexibleContentServiceMock = new Mock<IFlexibleContentService>();
            _widget = new FlexibleContentWidget(_flexibleContentServiceMock.Object);

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

        private static ComponentViewModel<FlexibleContentWidgetProperties> CreateModel(FlexibleContentWidgetProperties properties)
        {
            var model = new ComponentViewModel<FlexibleContentWidgetProperties>();
            typeof(ComponentViewModel<FlexibleContentWidgetProperties>)
                .GetProperty("Properties")?.SetValue(model, properties);
            return model;
        }

        [Test]
        public async Task InvokeAsync_WithAllProperties_PassesCorrectDataToService()
        {
            var pageGuid = Guid.NewGuid();
            var featuredGuid = Guid.NewGuid();

            var properties = new FlexibleContentWidgetProperties
            {
                Title = "Flexible Title",
                Description = "Flexible Description",
                LearnMoreButtonURLTarget = "_blank",
                TopMargin = "40px",
                BottomMargin = "20px",
                WidgetID = "flexible-content-1",
                Pages = new List<ContentItemReference>
                {
                    new ContentItemReference { Identifier = pageGuid }
                },
                FeaturedPages = new List<ContentItemReference>
                {
                    new ContentItemReference { Identifier = featuredGuid }
                }
            };

            var expectedDto = new FlexibleContentDto
            {
                Title = "Flexible Title",
                Description = "Flexible Description",
                TopMargin = "40px",
                BottomMargin = "20px",
                WidgetID = "flexible-content-1",
                LearnMoreButtonURLTarget = "_blank",
                Items = Array.Empty<FlexibleContentItemDto>()
            };

            _flexibleContentServiceMock
                .Setup(s => s.GetFlexibleContentAsync(
                    It.Is<FlexibleContentData>(d =>
                        d.Title == "Flexible Title" &&
                        d.Description == "Flexible Description" &&
                        d.TopMargin == "40px" &&
                        d.BottomMargin == "20px" &&
                        d.WidgetID == "flexible-content-1" &&
                        d.LearnMoreButtonURLTarget == "_blank" &&
                        d.FlexibleContentGuids.Contains(pageGuid) &&
                        d.FeaturedFlexibleContentGuids.Contains(featuredGuid)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _widget.InvokeAsync(CreateModel(properties));
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as FlexibleContentDto;

            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.Title, Is.EqualTo("Flexible Title"));
                Assert.That(viewModel!.Description, Is.EqualTo("Flexible Description"));
                Assert.That(viewModel!.TopMargin, Is.EqualTo("40px"));
                Assert.That(viewModel!.BottomMargin, Is.EqualTo("20px"));
                Assert.That(viewModel!.WidgetID, Is.EqualTo("flexible-content-1"));
                Assert.That(viewModel!.LearnMoreButtonURLTarget, Is.EqualTo("_blank"));
            });
        }

        [Test]
        public async Task InvokeAsync_WithNoPages_PassesEmptyGuidsToService()
        {
            var properties = new FlexibleContentWidgetProperties
            {
                Title = "Title",
                Pages = [],
                FeaturedPages = []
            };

            var expectedDto = new FlexibleContentDto
            {
                Title = "Title",
                Items = Array.Empty<FlexibleContentItemDto>()
            };

            _flexibleContentServiceMock
                .Setup(s => s.GetFlexibleContentAsync(
                    It.Is<FlexibleContentData>(d =>
                        !d.FlexibleContentGuids.Any() &&
                        !d.FeaturedFlexibleContentGuids.Any()),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _widget.InvokeAsync(CreateModel(properties));
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as FlexibleContentDto;

            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel!.Items, Is.Empty);

            _flexibleContentServiceMock.Verify(
                s => s.GetFlexibleContentAsync(
                    It.Is<FlexibleContentData>(d =>
                        !d.FlexibleContentGuids.Any() &&
                        !d.FeaturedFlexibleContentGuids.Any()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task InvokeAsync_WithDefaultProperties_UsesDefaultValues()
        {
            var properties = new FlexibleContentWidgetProperties();

            var expectedDto = new FlexibleContentDto
            {
                TopMargin = "0px",
                BottomMargin = "0px",
                LearnMoreButtonURLTarget = "_self",
                Items = Array.Empty<FlexibleContentItemDto>()
            };

            _flexibleContentServiceMock
                .Setup(s => s.GetFlexibleContentAsync(
                    It.Is<FlexibleContentData>(d =>
                        d.TopMargin == "0px" &&
                        d.BottomMargin == "0px" &&
                        d.LearnMoreButtonURLTarget == "_self"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _widget.InvokeAsync(CreateModel(properties));
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as FlexibleContentDto;

            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.TopMargin, Is.EqualTo("0px"));
                Assert.That(viewModel!.BottomMargin, Is.EqualTo("0px"));
                Assert.That(viewModel!.LearnMoreButtonURLTarget, Is.EqualTo("_self"));
            });
        }

        [Test]
        public async Task InvokeAsync_PassesItemsFromService_ToView()
        {
            var properties = new FlexibleContentWidgetProperties
            {
                Title = "Title",
                Pages = [],
                FeaturedPages = []
            };

            var items = new[]
            {
                new FlexibleContentItemDto { Title = "Item 1", IsFeatured = true, ReadMoreLink = "/item-1" },
                new FlexibleContentItemDto { Title = "Item 2", IsFeatured = false, ReadMoreLink = "/item-2" }
            };

            var expectedDto = new FlexibleContentDto
            {
                Title = "Title",
                Items = items
            };

            _flexibleContentServiceMock
                .Setup(s => s.GetFlexibleContentAsync(It.IsAny<FlexibleContentData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _widget.InvokeAsync(CreateModel(properties));
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as FlexibleContentDto;

            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel!.Items.Count(), Is.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.Items.First().Title, Is.EqualTo("Item 1"));
                Assert.That(viewModel!.Items.First().IsFeatured, Is.True);
                Assert.That(viewModel!.Items.Last().Title, Is.EqualTo("Item 2"));
                Assert.That(viewModel!.Items.Last().IsFeatured, Is.False);
            });
        }

        [Test]
        public async Task InvokeAsync_ReturnsCorrectViewName()
        {
            var properties = new FlexibleContentWidgetProperties
            {
                Pages = [],
                FeaturedPages = []
            };

            _flexibleContentServiceMock
                .Setup(s => s.GetFlexibleContentAsync(It.IsAny<FlexibleContentData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FlexibleContentDto { Items = Array.Empty<FlexibleContentItemDto>() });

            var result = await _widget.InvokeAsync(CreateModel(properties));
            var viewResult = result as ViewViewComponentResult;

            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.ViewName, Is.EqualTo("~/Components/Widget/FlexibleContentWidget/_FlexibleContentWidget.cshtml"));
        }

        [Test]
        public async Task InvokeAsync_PassesCancellationTokenFromHttpContext_ToService()
        {
            using var cts = new CancellationTokenSource();
            _httpContext.RequestAborted = cts.Token;

            var properties = new FlexibleContentWidgetProperties
            {
                Pages = [],
                FeaturedPages = []
            };

            _flexibleContentServiceMock
                .Setup(s => s.GetFlexibleContentAsync(It.IsAny<FlexibleContentData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FlexibleContentDto { Items = Array.Empty<FlexibleContentItemDto>() });

            await _widget.InvokeAsync(CreateModel(properties));

            _flexibleContentServiceMock.Verify(
                s => s.GetFlexibleContentAsync(
                    It.IsAny<FlexibleContentData>(),
                    It.Is<CancellationToken>(t => t == cts.Token)),
                Times.Once);
        }

        [Test]
        public async Task InvokeAsync_WithMultiplePages_PassesAllGuidsToService()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var featuredGuid = Guid.NewGuid();

            var properties = new FlexibleContentWidgetProperties
            {
                Pages = new List<ContentItemReference>
                {
                    new ContentItemReference { Identifier = guid1 },
                    new ContentItemReference { Identifier = guid2 }
                },
                FeaturedPages = new List<ContentItemReference>
                {
                    new ContentItemReference { Identifier = featuredGuid }
                }
            };

            _flexibleContentServiceMock
                .Setup(s => s.GetFlexibleContentAsync(It.IsAny<FlexibleContentData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FlexibleContentDto { Items = Array.Empty<FlexibleContentItemDto>() });

            await _widget.InvokeAsync(CreateModel(properties));

            _flexibleContentServiceMock.Verify(
                s => s.GetFlexibleContentAsync(
                    It.Is<FlexibleContentData>(d =>
                        d.FlexibleContentGuids.Contains(guid1) &&
                        d.FlexibleContentGuids.Contains(guid2) &&
                        d.FeaturedFlexibleContentGuids.Contains(featuredGuid)),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
