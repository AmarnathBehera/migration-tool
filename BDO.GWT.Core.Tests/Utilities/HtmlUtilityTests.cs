using BDO.GWT.Core.Utilities;
using NUnit.Framework;

namespace BDO.GWT.Core.Tests.Utilities;

public class HtmlUtilityTests
{
    [Test]
    public void RemoveTildeFromUrlTypeHtmlAttributes_ReturnsNull_WhenInputIsNull()
    {
        var result = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(null);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void RemoveTildeFromUrlTypeHtmlAttributes_ReturnsUnchanged_WhenNoTilde()
    {
        var html = """<a href="/about">About</a>""";

        var result = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(html);

        Assert.That(result, Is.EqualTo(html));
    }

    [Test]
    public void RemoveTildeFromUrlTypeHtmlAttributes_RemovesTilde_FromHrefAttribute()
    {
        var html = """<a href="~/about">About</a>""";

        var result = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(html);

        Assert.That(result, Is.EqualTo("""<a href="/about">About</a>"""));
    }

    [Test]
    public void RemoveTildeFromUrlTypeHtmlAttributes_RemovesTilde_FromSrcAttribute()
    {
        var html = """<img src="~/images/logo.png" />""";

        var result = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(html);

        Assert.That(result, Is.EqualTo("""<img src="/images/logo.png" />"""));
    }

    [Test]
    public void RemoveTildeFromUrlTypeHtmlAttributes_RemovesTilde_FromSrcsetAttribute()
    {
        var html = """<source srcset="~/images/hero.webp" />""";

        var result = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(html);

        Assert.That(result, Is.EqualTo("""<source srcset="/images/hero.webp" />"""));
    }

    [Test]
    public void RemoveTildeFromUrlTypeHtmlAttributes_RemovesTilde_FromPosterAttribute()
    {
        var html = """<video poster="~/media/video.jpg"></video>""";

        var result = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(html);

        Assert.That(result, Is.EqualTo("""<video poster="/media/video.jpg"></video>"""));
    }

    [Test]
    public void RemoveTildeFromUrlTypeHtmlAttributes_DoesNotRemoveTilde_FromNonUrlAttributes()
    {
        var html = """<span title="~/not-a-url">Text</span>""";

        var result = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(html);

        Assert.That(result, Is.EqualTo(html));
    }

    [Test]
    public void RemoveTildeFromUrlTypeHtmlAttributes_HandlesMultipleTildes()
    {
        var html = """<a href="~/page"><img src="~/img.png" /></a>""";

        var result = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(html);

        Assert.That(result, Is.EqualTo("""<a href="/page"><img src="/img.png" /></a>"""));
    }

    [Test]
    public void RemoveTildeFromUrlTypeHtmlAttributes_ReturnsUnchanged_WhenTildeAtStart()
    {
        var html = "~/some/path";

        var result = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(html);

        Assert.That(result, Is.EqualTo(html));
    }

    [Test]
    public void RemoveTildeFromUrlTypeHtmlAttributes_ReturnsEmptyString_WhenInputIsEmpty()
    {
        var result = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(string.Empty);

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void RemoveTildeFromUrlTypeHtmlAttributes_HandlesSingleQuotedAttributes()
    {
        var html = """<a href='~/about'>About</a>""";

        var result = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(html);

        Assert.That(result, Is.EqualTo("""<a href='/about'>About</a>"""));
    }

    [Test]
    public void RemoveTildeFromUrlTypeHtmlAttributes_RemovesTilde_FromCssUrlFunction()
    {
        var html = """background: url(~/images/bg.png)""";

        var result = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(html);

        Assert.That(result, Is.EqualTo("""background: url(/images/bg.png)"""));
    }
}