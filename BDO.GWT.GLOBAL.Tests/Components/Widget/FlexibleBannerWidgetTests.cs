using BDO.GWT.Core.Data;
using BDO.GWT.Web.Widgets.FlexibleBannerWidget;
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
    public class FlexibleBannerWidgetTests
    {
        private Mock<IContentRetriever> _contentRetrieverMock;
        private FlexibleBannerWidget _widget;
        private DefaultHttpContext _httpContext;

        [SetUp]
        public void SetUp()
        {
            _contentRetrieverMock = new Mock<IContentRetriever>();
            _widget = new FlexibleBannerWidget(_contentRetrieverMock.Object);

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

        private static ComponentViewModel<FlexibleBannerWidgetProperties> CreateModel(FlexibleBannerWidgetProperties properties)
        {
            var model = new ComponentViewModel<FlexibleBannerWidgetProperties>();
            typeof(ComponentViewModel<FlexibleBannerWidgetProperties>)
                .GetProperty("Properties")?.SetValue(model, properties);
            return model;
        }

        [Test]
        public async Task InvokeAsync_WithAllProperties_MapsFieldsCorrectly()
        {
            var properties = new FlexibleBannerWidgetProperties
            {
                Layout = "col-2",
                Description = "<p>Banner description</p>",
                BackgroundImageAltText = "Alt text",
                Theme = "dark",
                ButtonSide = "left",
                TopMargin = "40px",
                BottomMargin = "20px",
                PrimaryButtonText = "Primary",
                PrimaryButtonUrl = "/primary",
                PrimaryButtonType = "secondary",
                PrimaryButtonLinkTarget = "_blank",
                SecondaryButtonText = "Secondary",
                SecondaryButtonUrl = "/secondary",
                SecondaryButtonType = "tertiary",
                SecondaryButtonLinkTarget = "_self",
                WidgetID = "flexible-banner-1"
            };

            var result = await _widget.InvokeAsync(CreateModel(properties));
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as FlexibleBannerData;

            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.Layout, Is.EqualTo("col-2"));
                Assert.That(viewModel!.Description, Is.EqualTo("<p>Banner description</p>"));
                Assert.That(viewModel!.BackgroundImageAltText, Is.EqualTo("Alt text"));
                Assert.That(viewModel!.Theme, Is.EqualTo("dark"));
                Assert.That(viewModel!.ButtonSide, Is.EqualTo("left"));
                Assert.That(viewModel!.TopMargin, Is.EqualTo("40px"));
                Assert.That(viewModel!.BottomMargin, Is.EqualTo("20px"));
                Assert.That(viewModel!.WidgetID, Is.EqualTo("flexible-banner-1"));

                Assert.That(viewModel!.PrimaryButton, Is.Not.Null);
                Assert.That(viewModel!.PrimaryButton.Title, Is.EqualTo("Primary"));
                Assert.That(viewModel!.PrimaryButton.URL, Is.EqualTo("/primary"));
                Assert.That(viewModel!.PrimaryButton.Type, Is.EqualTo("secondary"));
                Assert.That(viewModel!.PrimaryButton.Target, Is.EqualTo("_blank"));

                Assert.That(viewModel!.SecondaryButton, Is.Not.Null);
                Assert.That(viewModel!.SecondaryButton!.Title, Is.EqualTo("Secondary"));
                Assert.That(viewModel!.SecondaryButton.URL, Is.EqualTo("/secondary"));
                Assert.That(viewModel!.SecondaryButton.Type, Is.EqualTo("tertiary"));
                Assert.That(viewModel!.SecondaryButton.Target, Is.EqualTo("_self"));
            });
        }

        [Test]
        public async Task InvokeAsync_WithDefaultProperties_MapsDefaultValues()
        {
            var properties = new FlexibleBannerWidgetProperties
            {
                PrimaryButtonText = "Primary",
                PrimaryButtonUrl = "/primary"
            };

            var result = await _widget.InvokeAsync(CreateModel(properties));
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as FlexibleBannerData;

            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.Layout, Is.EqualTo("col-1"));
                Assert.That(viewModel!.Theme, Is.EqualTo("light"));
                Assert.That(viewModel!.ButtonSide, Is.EqualTo("right"));
                Assert.That(viewModel!.PrimaryButton.Type, Is.EqualTo("primary"));
                Assert.That(viewModel!.PrimaryButton.Target, Is.EqualTo("_self"));
            });
        }

        [Test]
        public async Task InvokeAsync_WithNoBackgroundImage_BackgroundImageIsNull()
        {
            var properties = new FlexibleBannerWidgetProperties
            {
                PrimaryButtonText = "Primary",
                PrimaryButtonUrl = "/primary"
            };

            var result = await _widget.InvokeAsync(CreateModel(properties));
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as FlexibleBannerData;

            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel!.BackgroundImage, Is.Null);
        }

        [Test]
        public async Task InvokeAsync_WithEmptySecondaryButtonText_SecondaryButtonIsNull()
        {
            var properties = new FlexibleBannerWidgetProperties
            {
                PrimaryButtonText = "Primary",
                PrimaryButtonUrl = "/primary",
                SecondaryButtonText = string.Empty,
                SecondaryButtonUrl = "/secondary"
            };

            var result = await _widget.InvokeAsync(CreateModel(properties));
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as FlexibleBannerData;

            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel!.SecondaryButton, Is.Null);
        }

        [Test]
        public async Task InvokeAsync_WithEmptySecondaryButtonUrl_SecondaryButtonIsNull()
        {
            var properties = new FlexibleBannerWidgetProperties
            {
                PrimaryButtonText = "Primary",
                PrimaryButtonUrl = "/primary",
                SecondaryButtonText = "Secondary",
                SecondaryButtonUrl = string.Empty
            };

            var result = await _widget.InvokeAsync(CreateModel(properties));
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as FlexibleBannerData;

            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel!.SecondaryButton, Is.Null);
        }

        [Test]
        public async Task InvokeAsync_WithNullSecondaryButton_SecondaryButtonIsNull()
        {
            var properties = new FlexibleBannerWidgetProperties
            {
                PrimaryButtonText = "Primary",
                PrimaryButtonUrl = "/primary",
                SecondaryButtonText = null,
                SecondaryButtonUrl = null
            };

            var result = await _widget.InvokeAsync(CreateModel(properties));
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as FlexibleBannerData;

            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel!.SecondaryButton, Is.Null);
        }

        [Test]
        public async Task InvokeAsync_WithBothSecondaryButtonFields_SecondaryButtonIsCreated()
        {
            var properties = new FlexibleBannerWidgetProperties
            {
                PrimaryButtonText = "Primary",
                PrimaryButtonUrl = "/primary",
                SecondaryButtonText = "Secondary",
                SecondaryButtonUrl = "/secondary",
                SecondaryButtonType = "primary",
                SecondaryButtonLinkTarget = "_blank"
            };

            var result = await _widget.InvokeAsync(CreateModel(properties));
            var viewModel = (result as ViewViewComponentResult)?.ViewData?.Model as FlexibleBannerData;

            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel!.SecondaryButton, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.SecondaryButton!.Title, Is.EqualTo("Secondary"));
                Assert.That(viewModel!.SecondaryButton.URL, Is.EqualTo("/secondary"));
                Assert.That(viewModel!.SecondaryButton.Type, Is.EqualTo("primary"));
                Assert.That(viewModel!.SecondaryButton.Target, Is.EqualTo("_blank"));
            });
        }

        [Test]
        public async Task InvokeAsync_ReturnsCorrectViewName()
        {
            var properties = new FlexibleBannerWidgetProperties
            {
                PrimaryButtonText = "Primary",
                PrimaryButtonUrl = "/primary"
            };

            var result = await _widget.InvokeAsync(CreateModel(properties));
            var viewResult = result as ViewViewComponentResult;

            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.ViewName, Is.EqualTo("~/Components/Widget/FlexibleBanner/_FlexibleBanner.cshtml"));
        }

        [Test]
        public async Task InvokeAsync_ReturnsViewViewComponentResult()
        {
            var properties = new FlexibleBannerWidgetProperties
            {
                PrimaryButtonText = "Primary",
                PrimaryButtonUrl = "/primary"
            };

            var result = await _widget.InvokeAsync(CreateModel(properties));

            Assert.That(result, Is.TypeOf<ViewViewComponentResult>());
        }
    }
}
