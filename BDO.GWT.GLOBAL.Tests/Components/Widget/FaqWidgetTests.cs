using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services.Widget;
using BDO.GWT.GLOBAL.Components.Widget.FaqWidget;
using BDO.GWT.Web.Widgets.AccordionWidget;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget
{
    public class FaqWidgetTests
    {
        private Mock<IFaqService> _faqServiceMock;
        private FaqWidget _widget;
        private DefaultHttpContext _httpContext;

        [SetUp]
        public void SetUp()
        {
            _faqServiceMock = new Mock<IFaqService>();

            _widget = new FaqWidget(_faqServiceMock.Object);

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

        private static ComponentViewModel<FaqWidgetProperties> CreateModel(FaqWidgetProperties properties)
        {
            var model = new ComponentViewModel<FaqWidgetProperties>();
            typeof(ComponentViewModel<FaqWidgetProperties>)
                .GetProperty("Properties")?.SetValue(model, properties);
            return model;
        }

        [Test]
        public async Task InvokeAsync_WithAllProperties_PassesCorrectDataToService()
        {
            var pageGuid = Guid.NewGuid();
            var properties = new FaqWidgetProperties
            {
                Title = "FAQ Title",
                Layout = "accordion",
                HoverColour = "red",
                TopMargin = "40px",
                BottomMargin = "20px",
                WidgetID = "faq-widget-1",
                Pages = [new CMS.ContentEngine.ContentItemReference { Identifier = pageGuid }]
            };
            var expectedDto = new FaqDto
            {
                Title = "FAQ Title",
                Layout = "accordion",
                HoverColour = "red",
                TopMargin = "40px",
                BottomMargin = "20px",
                WidgetID = "faq-widget-1",
                Topics = []
            };

            _faqServiceMock
                .Setup(s => s.GetFaqAsync(
                    It.Is<BDO.GWT.Core.Data.FaqData>(d =>
                        d.Title == "FAQ Title" &&
                        d.Layout == "accordion" &&
                        d.HoverColour == "red" &&
                        d.TopMargin == "40px" &&
                        d.BottomMargin == "20px" &&
                        d.WidgetID == "faq-widget-1" &&
                        d.PageGuids.Contains(pageGuid)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            _faqServiceMock
                .Setup(s => s.GetFaqSchemaOrgAsync(expectedDto))
                .Returns(new BDO.GWT.Core.DTOs.SchemaMarkups.FaqSchemaMarkupDto());

            var model = CreateModel(properties);

            var result = await _widget.InvokeAsync(model);
            var viewResult = result as ViewViewComponentResult;
            var viewModel = viewResult?.ViewData?.Model as FaqDto;

            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.Title, Is.EqualTo("FAQ Title"));
                Assert.That(viewModel!.Layout, Is.EqualTo("accordion"));
                Assert.That(viewModel!.HoverColour, Is.EqualTo("red"));
                Assert.That(viewModel!.TopMargin, Is.EqualTo("40px"));
                Assert.That(viewModel!.BottomMargin, Is.EqualTo("20px"));
                Assert.That(viewModel!.WidgetID, Is.EqualTo("faq-widget-1"));
            });
        }

        [Test]
        public async Task InvokeAsync_WithNoPages_PassesEmptyGuidsToService()
        {
            var properties = new FaqWidgetProperties
            {
                Title = "FAQ Title",
                Layout = "cards",
                Pages = []
            };
            var expectedDto = new FaqDto { Title = "FAQ Title", Layout = "cards", Topics = [] };

            _faqServiceMock
                .Setup(s => s.GetFaqAsync(
                    It.Is<BDO.GWT.Core.Data.FaqData>(d => !d.PageGuids.Any()),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            _faqServiceMock
                .Setup(s => s.GetFaqSchemaOrgAsync(It.IsAny<FaqDto>()))
                .Returns(new BDO.GWT.Core.DTOs.SchemaMarkups.FaqSchemaMarkupDto());

            var model = CreateModel(properties);

            var result = await _widget.InvokeAsync(model);
            var viewResult = result as ViewViewComponentResult;
            var viewModel = viewResult?.ViewData?.Model as FaqDto;

            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel!.Topics, Is.Empty);

            _faqServiceMock.Verify(
                s => s.GetFaqAsync(
                    It.Is<BDO.GWT.Core.Data.FaqData>(d => !d.PageGuids.Any()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task InvokeAsync_WithNullPages_PassesEmptyGuidsToService()
        {
            var properties = new FaqWidgetProperties
            {
                Title = "Test",
                Layout = "list",
                Pages = null
            };
            var expectedDto = new FaqDto { Title = "Test", Layout = "list", Topics = [] };

            _faqServiceMock
                .Setup(s => s.GetFaqAsync(
                    It.Is<BDO.GWT.Core.Data.FaqData>(d => !d.PageGuids.Any()),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            _faqServiceMock
                .Setup(s => s.GetFaqSchemaOrgAsync(It.IsAny<FaqDto>()))
                .Returns(new BDO.GWT.Core.DTOs.SchemaMarkups.FaqSchemaMarkupDto());

            var model = CreateModel(properties);

            var result = await _widget.InvokeAsync(model);
            var viewResult = result as ViewViewComponentResult;

            Assert.That(viewResult, Is.Not.Null);
        }

        [Test]
        public async Task InvokeAsync_ReturnsViewWithCorrectViewPath()
        {
            var properties = new FaqWidgetProperties { Layout = "cards" };
            var expectedDto = new FaqDto { Layout = "cards", Topics = [] };

            _faqServiceMock
                .Setup(s => s.GetFaqAsync(It.IsAny<BDO.GWT.Core.Data.FaqData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            _faqServiceMock
                .Setup(s => s.GetFaqSchemaOrgAsync(It.IsAny<FaqDto>()))
                .Returns(new BDO.GWT.Core.DTOs.SchemaMarkups.FaqSchemaMarkupDto());

            var model = CreateModel(properties);

            var result = await _widget.InvokeAsync(model);
            var viewResult = result as ViewViewComponentResult;

            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.ViewName, Is.EqualTo("~/Components/Widget/FaqWidget/_FaqWidget.cshtml"));
        }

        [Test]
        public async Task InvokeAsync_DoesNotCallGetFaqSchemaOrgAsync()
        {
            var properties = new FaqWidgetProperties { Layout = "cards" };

            _faqServiceMock
                .Setup(s => s.GetFaqAsync(It.IsAny<BDO.GWT.Core.Data.FaqData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FaqDto { Layout = "cards", Topics = [] });

            var model = CreateModel(properties);

            await _widget.InvokeAsync(model);

            _faqServiceMock.Verify(s => s.GetFaqSchemaOrgAsync(It.IsAny<FaqDto>()), Times.Never);
        }

        [Test]
        public async Task InvokeAsync_DefaultLayout_IsCards()
        {
            var properties = new FaqWidgetProperties();
            var expectedDto = new FaqDto { Layout = "cards", Topics = [] };

            _faqServiceMock
                .Setup(s => s.GetFaqAsync(
                    It.Is<BDO.GWT.Core.Data.FaqData>(d => d.Layout == "cards"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            _faqServiceMock
                .Setup(s => s.GetFaqSchemaOrgAsync(It.IsAny<FaqDto>()))
                .Returns(new BDO.GWT.Core.DTOs.SchemaMarkups.FaqSchemaMarkupDto());

            var model = CreateModel(properties);

            await _widget.InvokeAsync(model);

            _faqServiceMock.Verify(
                s => s.GetFaqAsync(
                    It.Is<BDO.GWT.Core.Data.FaqData>(d => d.Layout == "cards"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
