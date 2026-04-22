using BDO.GWT.Web.Core.LiveSite.Data;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ButtonWidget;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NUnit.Framework;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget
{
    public class ButtonWidgetTests
    {
        private ButtonWidget _widget = null!;

        [SetUp]
        public void SetUp()
        {
            _widget = new ButtonWidget();

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

        // ── View name ────────────────────────────────────────────────────────

        [Test]
        public void Invoke_ReturnsCorrectViewName()
        {
            var model = BuildComponentViewModel(new ButtonWidgetProperties());

            var result = _widget.Invoke(model) as ViewViewComponentResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ViewName, Is.EqualTo("~/Components/Widget/ButtonWidget/_ButtonWidget.cshtml"));
        }

        // ── Full property mapping ─────────────────────────────────────────────

        [Test]
        public void Invoke_MapsAllPropertiesCorrectly()
        {
            var properties = new ButtonWidgetProperties
            {
                Text = "Click Here",
                Link = "/contact",
                Target = "_blank",
                Type = "secondary",
                Theme = "dark",
                ButtonSide = "center",
                TopMargin = "20px",
                BottomMargin = "40px"
            };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as ButtonData;

            Assert.Multiple(() =>
            {
                Assert.That(model, Is.Not.Null);
                Assert.That(model!.Text, Is.EqualTo("Click Here"));
                Assert.That(model.Link, Is.EqualTo("/contact"));
                Assert.That(model.Target, Is.EqualTo("_blank"));
                Assert.That(model.Type, Is.EqualTo("secondary"));
                Assert.That(model.Theme, Is.EqualTo("dark"));
                Assert.That(model.ButtonSide, Is.EqualTo("center"));
                Assert.That(model.TopMargin, Is.EqualTo("20px"));
                Assert.That(model.BottomMargin, Is.EqualTo("40px"));
            });
        }

        // ── Default property values ──────────────────────────────────────────

        [Test]
        public void Invoke_WithDefaultProperties_MapsDefaultValues()
        {
            var properties = new ButtonWidgetProperties();

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as ButtonData;

            Assert.Multiple(() =>
            {
                Assert.That(model, Is.Not.Null);
                Assert.That(model!.Target, Is.EqualTo("_self"));
                Assert.That(model.Type, Is.EqualTo("primary"));
                Assert.That(model.Theme, Is.EqualTo("light"));
                Assert.That(model.ButtonSide, Is.EqualTo("left"));
                Assert.That(model.TopMargin, Is.EqualTo("0px"));
                Assert.That(model.BottomMargin, Is.EqualTo("0px"));
            });
        }

        // ── Null / empty optional fields ─────────────────────────────────────

        [Test]
        public void Invoke_WithNullTextAndLink_MapsNullToModel()
        {
            var properties = new ButtonWidgetProperties();

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as ButtonData;

            Assert.Multiple(() =>
            {
                Assert.That(model, Is.Not.Null);
                Assert.That(model!.Text, Is.Null);
                Assert.That(model.Link, Is.Null);
            });
        }

        // ── Individual field mapping ──────────────────────────────────────────

        [Test]
        public void Invoke_MapsTextOnly_WhenOnlyTextProvided()
        {
            var properties = new ButtonWidgetProperties { Text = "Submit" };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as ButtonData;

            Assert.That(model!.Text, Is.EqualTo("Submit"));
        }

        [Test]
        public void Invoke_MapsLinkOnly_WhenOnlyLinkProvided()
        {
            var properties = new ButtonWidgetProperties { Link = "/about" };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as ButtonData;

            Assert.That(model!.Link, Is.EqualTo("/about"));
        }

        [Test]
        public void Invoke_MapsTargetSelf_WhenTargetIsSameTab()
        {
            var properties = new ButtonWidgetProperties { Target = "_self" };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as ButtonData;

            Assert.That(model!.Target, Is.EqualTo("_self"));
        }

        [Test]
        public void Invoke_MapsTargetBlank_WhenTargetIsNewTab()
        {
            var properties = new ButtonWidgetProperties { Target = "_blank" };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as ButtonData;

            Assert.That(model!.Target, Is.EqualTo("_blank"));
        }

        [Test]
        public void Invoke_MapsTertiaryType_WhenTypeIsTertiary()
        {
            var properties = new ButtonWidgetProperties { Type = "tertiary" };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as ButtonData;

            Assert.That(model!.Type, Is.EqualTo("tertiary"));
        }

        [Test]
        public void Invoke_MapsMargins_WhenMarginsAreSet()
        {
            var properties = new ButtonWidgetProperties
            {
                TopMargin = "60px",
                BottomMargin = "80px"
            };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as ButtonData;

            Assert.Multiple(() =>
            {
                Assert.That(model!.TopMargin, Is.EqualTo("60px"));
                Assert.That(model.BottomMargin, Is.EqualTo("80px"));
            });
        }

        [Test]
        public void Invoke_MapsButtonSideRight_WhenButtonSideIsRight()
        {
            var properties = new ButtonWidgetProperties { ButtonSide = "right" };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as ButtonData;

            Assert.That(model!.ButtonSide, Is.EqualTo("right"));
        }

        // ── Model type ───────────────────────────────────────────────────────

        [Test]
        public void Invoke_ModelIsButtonData()
        {
            var result = _widget.Invoke(BuildComponentViewModel(new ButtonWidgetProperties())) as ViewViewComponentResult;

            Assert.That(result?.ViewData?.Model, Is.TypeOf<ButtonData>());
        }

        // ── Helper ───────────────────────────────────────────────────────────

        private static ComponentViewModel<ButtonWidgetProperties> BuildComponentViewModel(ButtonWidgetProperties properties)
        {
            var model = new ComponentViewModel<ButtonWidgetProperties>();
            typeof(ComponentViewModel<ButtonWidgetProperties>)
                .GetProperty("Properties")
                ?.SetValue(model, properties);
            return model;
        }
    }
}