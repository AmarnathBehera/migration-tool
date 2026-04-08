using BDO.GWT.Core.Services.Widget;
using BDO.GWT.Web.Core.LiveSite.Data;
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
        public async Task InvokeAsync_WithNullOrEmptyProperties_UsesDefaults()
        {
            // Arrange
            var properties = new TiledLinkSectionWidgetProperties
            {
                TilePages = null
            };
            var model = new ComponentViewModel<TiledLinkSectionWidgetProperties>();
            typeof(ComponentViewModel<TiledLinkSectionWidgetProperties>)
                .GetProperty("Properties")?.SetValue(model, properties);

            // Act
            var result = await _widget.InvokeAsync(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as TiledLinkSectionData;

            // Assert defaults
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.Title, Is.Null);
            Assert.That(viewModel.Description, Is.Null);
            Assert.That(viewModel.BackgroundImageMediaFileGuid, Is.Null);
            Assert.That(viewModel.BackgroundImageAltText, Is.Null.Or.Empty);
            Assert.That(viewModel.TileNodeGuids, Is.Empty);
            Assert.That(viewModel.Theme, Is.EqualTo("light")); // default
            Assert.That(viewModel.SeeAllLinkText, Is.Null);
            Assert.That(viewModel.SeeAllLinkURLTarget, Is.EqualTo("_self")); // default
            Assert.That(viewModel.TopMargin, Is.Null);
            Assert.That(viewModel.BottomMargin, Is.Null);
        }

        [Test]
        public async Task InvokeAsync_WithAllProperties_SetsEverythingCorrectly()
        {
            // Arrange
            var properties = new TiledLinkSectionWidgetProperties
            {
                Title = "Test Title",
                Description = "Test Description",
                Theme = "dark",
                SeeAllLinkText = "See All",
                SeeAllLinkURL = "/all",
                SeeAllLinkURLTarget = "_blank",
                TopMargin = "20px",
                BottomMargin = "40px",
                TilePages = new List<ContentItemReference>()
            };

            var model = new ComponentViewModel<TiledLinkSectionWidgetProperties>();
            typeof(ComponentViewModel<TiledLinkSectionWidgetProperties>)
                .GetProperty("Properties")?.SetValue(model, properties);

            // Act
            var result = await _widget.InvokeAsync(model);
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as TiledLinkSectionData;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.Title, Is.EqualTo("Test Title"));
            Assert.That(viewModel.Description, Is.EqualTo("Test Description"));
            Assert.That(viewModel.Theme, Is.EqualTo("dark"));
            Assert.That(viewModel.SeeAllLinkText, Is.EqualTo("See All"));
            Assert.That(viewModel.SeeAllLinkURLTarget, Is.EqualTo("_blank"));
            Assert.That(viewModel.TopMargin, Is.EqualTo("20px"));
            Assert.That(viewModel.BottomMargin, Is.EqualTo("40px"));
        }
    }
}