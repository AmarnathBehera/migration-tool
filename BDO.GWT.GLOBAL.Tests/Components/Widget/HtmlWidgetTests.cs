using BDO.GWT.Core.DTOs;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.HtmlWidget;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NUnit.Framework;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget
{
    public class HtmlWidgetTests
    {
        private HtmlWidget _widget = null!;

        [SetUp]
        public void SetUp()
        {
            _widget = new HtmlWidget();

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

        // ── View name ──────────────────────────────────────────────────────────────

        [Test]
        public void Invoke_ReturnsCorrectViewName()
        {
            var model = BuildComponentViewModel(new HtmlWidgetProperties());

            var result = _widget.Invoke(model) as ViewViewComponentResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ViewName, Is.EqualTo("~/Components/Widget/HtmlWidget/_HtmlWidget.cshtml"));
        }

        // ── Full property mapping ───────────────────────────────────────────────────

        [Test]
        public void Invoke_MapsHtmlCodeCorrectly()
        {
            var htmlCode = "<div><p>Hello World</p></div>";
            var properties = new HtmlWidgetProperties
            {
                HtmlCode = htmlCode
            };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.Multiple(() =>
            {
                Assert.That(model, Is.Not.Null);
                Assert.That(model!.HtmlCode, Is.EqualTo(htmlCode));
            });
        }

        // ── Null / empty optional fields ────────────────────────────────────────────

        [Test]
        public void Invoke_WithNullHtmlCode_MapsNullToModel()
        {
            var properties = new HtmlWidgetProperties { HtmlCode = null! };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.Multiple(() =>
            {
                Assert.That(model, Is.Not.Null);
                Assert.That(model!.HtmlCode, Is.Null);
            });
        }

        [Test]
        public void Invoke_WithEmptyHtmlCode_MapsEmptyStringToModel()
        {
            var properties = new HtmlWidgetProperties { HtmlCode = string.Empty };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.Multiple(() =>
            {
                Assert.That(model, Is.Not.Null);
                Assert.That(model!.HtmlCode, Is.EqualTo(string.Empty));
            });
        }

        [Test]
        public void Invoke_WithWhitespaceOnlyHtmlCode_MapsWhitespaceToModel()
        {
            var htmlCode = "   \n\t  ";
            var properties = new HtmlWidgetProperties { HtmlCode = htmlCode };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.That(model!.HtmlCode, Is.EqualTo(htmlCode));
        }

        // ── Common HTML scenarios ───────────────────────────────────────────────────

        [Test]
        public void Invoke_WithSimpleHtmlContent_MapsCorrectly()
        {
            var htmlCode = "<h1>Title</h1><p>Content</p>";
            var properties = new HtmlWidgetProperties { HtmlCode = htmlCode };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.That(model!.HtmlCode, Is.EqualTo(htmlCode));
        }

        [Test]
        public void Invoke_WithNestedHtmlContent_MapsCorrectly()
        {
            var htmlCode = "<div class=\"container\"><div class=\"row\"><div class=\"col\"><span>Text</span></div></div></div>";
            var properties = new HtmlWidgetProperties { HtmlCode = htmlCode };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.That(model!.HtmlCode, Is.EqualTo(htmlCode));
        }

        [Test]
        public void Invoke_WithHtmlAttributes_MapsCorrectly()
        {
            var htmlCode = "<a href=\"https://example.com\" target=\"_blank\" rel=\"noopener\">Link</a>";
            var properties = new HtmlWidgetProperties { HtmlCode = htmlCode };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.That(model!.HtmlCode, Is.EqualTo(htmlCode));
        }

        [Test]
        public void Invoke_WithSelfClosingTags_MapsCorrectly()
        {
            var htmlCode = "<img src=\"/image.jpg\" alt=\"Image\" /><br /><hr />";
            var properties = new HtmlWidgetProperties { HtmlCode = htmlCode };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.That(model!.HtmlCode, Is.EqualTo(htmlCode));
        }

        // ── Special characters and escaping ─────────────────────────────────────────

        [Test]
        public void Invoke_WithSpecialCharacters_MapsCorrectly()
        {
            var htmlCode = "<p>Special chars: &amp; &lt; &gt; &quot; &apos;</p>";
            var properties = new HtmlWidgetProperties { HtmlCode = htmlCode };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.That(model!.HtmlCode, Is.EqualTo(htmlCode));
        }

        [Test]
        public void Invoke_WithUnicodeCharacters_MapsCorrectly()
        {
            var htmlCode = "<p>Unicode: ñ é ü © ® ™ 中文</p>";
            var properties = new HtmlWidgetProperties { HtmlCode = htmlCode };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.That(model!.HtmlCode, Is.EqualTo(htmlCode));
        }

        [Test]
        public void Invoke_WithQuotesAndApostrophes_MapsCorrectly()
        {
            var htmlCode = "<div data-tooltip=\"It's a quote\">Content</div>";
            var properties = new HtmlWidgetProperties { HtmlCode = htmlCode };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.That(model!.HtmlCode, Is.EqualTo(htmlCode));
        }

        [Test]
        public void Invoke_WithNewlinesAndTabs_MapsCorrectly()
        {
            var htmlCode = "<div>\n\t<p>\n\t\tContent\n\t</p>\n</div>";
            var properties = new HtmlWidgetProperties { HtmlCode = htmlCode };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.That(model!.HtmlCode, Is.EqualTo(htmlCode));
        }

        // ── Script and dangerous content ────────────────────────────────────────────

        [Test]
        public void Invoke_WithScriptTag_MapsWithoutSanitization()
        {
            var htmlCode = "<script>console.log('test');</script><p>Content</p>";
            var properties = new HtmlWidgetProperties { HtmlCode = htmlCode };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.That(model!.HtmlCode, Is.EqualTo(htmlCode));
        }

        [Test]
        public void Invoke_WithEventHandlers_MapsWithoutFiltering()
        {
            var htmlCode = "<div onclick=\"alert('click')\">Click me</div>";
            var properties = new HtmlWidgetProperties { HtmlCode = htmlCode };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.That(model!.HtmlCode, Is.EqualTo(htmlCode));
        }

        [Test]
        public void Invoke_WithStyleTag_MapsWithoutRemoval()
        {
            var htmlCode = "<style>body { color: red; }</style><p>Styled</p>";
            var properties = new HtmlWidgetProperties { HtmlCode = htmlCode };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.That(model!.HtmlCode, Is.EqualTo(htmlCode));
        }

        // ── Edge cases ──────────────────────────────────────────────────────────────

        [Test]
        public void Invoke_WithVeryLongHtmlCode_MapsCorrectly()
        {
            var longContent = string.Concat(Enumerable.Range(1, 1000).Select(i => $"<p>Line {i}</p>"));
            var properties = new HtmlWidgetProperties { HtmlCode = longContent };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.That(model!.HtmlCode, Is.EqualTo(longContent));
            Assert.That(model.HtmlCode.Length, Is.GreaterThan(5000));
        }

        [Test]
        public void Invoke_WithIFrameContent_MapsCorrectly()
        {
            var htmlCode = "<iframe src=\"https://example.com\" width=\"100%\" height=\"400\"></iframe>";
            var properties = new HtmlWidgetProperties { HtmlCode = htmlCode };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.That(model!.HtmlCode, Is.EqualTo(htmlCode));
        }

        [Test]
        public void Invoke_WithTableContent_MapsCorrectly()
        {
            var htmlCode = "<table><tr><td>Cell 1</td><td>Cell 2</td></tr></table>";
            var properties = new HtmlWidgetProperties { HtmlCode = htmlCode };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.That(model!.HtmlCode, Is.EqualTo(htmlCode));
        }

        [Test]
        public void Invoke_WithFormContent_MapsCorrectly()
        {
            var htmlCode = "<form><input type=\"text\" /><button type=\"submit\">Submit</button></form>";
            var properties = new HtmlWidgetProperties { HtmlCode = htmlCode };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.That(model!.HtmlCode, Is.EqualTo(htmlCode));
        }

        [Test]
        public void Invoke_WithCommentsInHtml_MapsCorrectly()
        {
            var htmlCode = "<!-- This is a comment --><p>Content after comment</p>";
            var properties = new HtmlWidgetProperties { HtmlCode = htmlCode };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.That(model!.HtmlCode, Is.EqualTo(htmlCode));
        }

        [Test]
        public void Invoke_WithCDATASection_MapsCorrectly()
        {
            var htmlCode = "<p><![CDATA[Some data]]></p>";
            var properties = new HtmlWidgetProperties { HtmlCode = htmlCode };

            var result = _widget.Invoke(BuildComponentViewModel(properties)) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as HtmlDto;

            Assert.That(model!.HtmlCode, Is.EqualTo(htmlCode));
        }

        // ── Model type ─────────────────────────────────────────────────────────────

        [Test]
        public void Invoke_ModelIsHtmlDto()
        {
            var result = _widget.Invoke(BuildComponentViewModel(new HtmlWidgetProperties())) as ViewViewComponentResult;

            Assert.That(result?.ViewData?.Model, Is.TypeOf<HtmlDto>());
        }

        [Test]
        public void Invoke_ReturnsViewViewComponentResult()
        {
            var result = _widget.Invoke(BuildComponentViewModel(new HtmlWidgetProperties()));

            Assert.That(result, Is.TypeOf<ViewViewComponentResult>());
        }

        // ── Multiple invocations ───────────────────────────────────────────────────

        [Test]
        public void Invoke_MultipleTimesWithDifferentContent_MapsEachCorrectly()
        {
            var content1 = "<p>First</p>";
            var content2 = "<p>Second</p>";

            var result1 = _widget.Invoke(BuildComponentViewModel(new HtmlWidgetProperties { HtmlCode = content1 })) as ViewViewComponentResult;
            var model1 = result1?.ViewData?.Model as HtmlDto;

            var result2 = _widget.Invoke(BuildComponentViewModel(new HtmlWidgetProperties { HtmlCode = content2 })) as ViewViewComponentResult;
            var model2 = result2?.ViewData?.Model as HtmlDto;

            Assert.Multiple(() =>
            {
                Assert.That(model1!.HtmlCode, Is.EqualTo(content1));
                Assert.That(model2!.HtmlCode, Is.EqualTo(content2));
            });
        }

        // ── Helper ─────────────────────────────────────────────────────────────────

        private static ComponentViewModel<HtmlWidgetProperties> BuildComponentViewModel(HtmlWidgetProperties properties)
        {
            var model = new ComponentViewModel<HtmlWidgetProperties>();
            typeof(ComponentViewModel<HtmlWidgetProperties>)
                .GetProperty("Properties")
                ?.SetValue(model, properties);
            return model;
        }
    }
}
