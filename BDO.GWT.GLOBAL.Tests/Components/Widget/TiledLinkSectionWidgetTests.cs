using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services.Widget;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.TiledLinkSectionWidget;
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
    public class TiledLinkSectionWidgetTests
    {
        private Mock<IContentRetriever> _contentRetrieverMock;
        private Mock<ITileContentService> _tileContentServiceMock;
        private TiledLinkSectionWidget _widget;
        private DefaultHttpContext _httpContext;

        [SetUp]
        public void SetUp()
        {
            _contentRetrieverMock = new Mock<IContentRetriever>();
            _tileContentServiceMock = new Mock<ITileContentService>();

            _widget = new TiledLinkSectionWidget(
                _contentRetrieverMock.Object,
                _tileContentServiceMock.Object
            );

            _httpContext = new DefaultHttpContext
            {
                RequestAborted = CancellationToken.None
            };

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

        private static ComponentViewModel<TiledLinkSectionWidgetProperties> CreateModel(
            TiledLinkSectionWidgetProperties properties)
        {
            var model = new ComponentViewModel<TiledLinkSectionWidgetProperties>();
            typeof(ComponentViewModel<TiledLinkSectionWidgetProperties>)
                .GetProperty("Properties")?.SetValue(model, properties);
            return model;
        }

        [Test]
        public async Task InvokeAsync_WithEmptyProperties_UsesDefaults()
        {
            // Arrange
            var properties = new TiledLinkSectionWidgetProperties
            {
                TilePagesIndustry = null,
                TilePagesService = null
            };
            var model = CreateModel(properties);

            // Act
            var result = await _widget.InvokeAsync(model);
            var dto = (result as ViewViewComponentResult)?.ViewData?.Model as TiledLinkSectionDto;

            // Assert defaults
            Assert.That(dto, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(dto!.Title, Is.Null);
                Assert.That(dto.Description, Is.Null);
                Assert.That(dto.BackgroundImageURL, Is.Null);
                Assert.That(dto.BackgroundImageAltText, Is.Null.Or.Empty);
                Assert.That(dto.Items, Is.Empty);
                Assert.That(dto.Theme, Is.EqualTo("light")); // default
                Assert.That(dto.SeeAllLink, Is.Null);
                Assert.That(dto.TopMargin, Is.Null);
                Assert.That(dto.BottomMargin, Is.Null);
                Assert.That(dto.WidgetID, Is.Null);
            });

            _tileContentServiceMock.Verify(
                s => s.GetTilePagesAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task InvokeAsync_WithAllProperties_SetsEverythingCorrectly()
        {
            // Arrange
            var industryGuid = Guid.NewGuid();
            var tilePages = new List<TilePageDto>
            {
                new() { Title = "Tile 1" }
            };

            _tileContentServiceMock
                .Setup(s => s.GetTilePagesAsync(
                    It.Is<List<Guid>>(g => g.Contains(industryGuid)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(tilePages);

            var properties = new TiledLinkSectionWidgetProperties
            {
                Title = "Test Title",
                Description = "Test Description",
                BackgroundImageAltText = "Alt text",
                TilePageType = "Industry",
                TilePagesIndustry = new List<ContentItemReference>
                {
                    new() { Identifier = industryGuid }
                },
                Theme = "dark",
                SeeAllLinkText = "See All",
                SeeAllLinkURL = "/all",
                SeeAllLinkURLTarget = "_blank",
                TopMargin = "20px",
                BottomMargin = "40px",
                WidgetID = "tiled-link-section"
            };

            var model = CreateModel(properties);

            // Act
            var result = await _widget.InvokeAsync(model);
            var dto = (result as ViewViewComponentResult)?.ViewData?.Model as TiledLinkSectionDto;

            // Assert
            Assert.That(dto, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(dto!.Title, Is.EqualTo("Test Title"));
                Assert.That(dto.Description, Is.EqualTo("Test Description"));
                Assert.That(dto.BackgroundImageAltText, Is.EqualTo("Alt text"));
                Assert.That(dto.Theme, Is.EqualTo("dark"));
                Assert.That(dto.Items, Is.EqualTo(tilePages));
                Assert.That(dto.SeeAllLink, Is.Not.Null);
                Assert.That(dto.SeeAllLink!.Title, Is.EqualTo("See All"));
                Assert.That(dto.SeeAllLink.URL, Is.EqualTo("/all"));
                Assert.That(dto.SeeAllLink.Target, Is.EqualTo("_blank"));
                Assert.That(dto.TopMargin, Is.EqualTo("20px"));
                Assert.That(dto.BottomMargin, Is.EqualTo("40px"));
                Assert.That(dto.WidgetID, Is.EqualTo("tiled-link-section"));
            });
        }
    }
}