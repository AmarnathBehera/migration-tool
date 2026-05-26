using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Helpers;

namespace BDO.GWT.Core.Tests.Helpers;

public class PageHelperTests
{
    [Test]
    public void GetImageDto_WhenImageLinkIsNull_ReturnsNull()
    {
        var result = PageHelper.GetImageDto(null, "Alt text");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetImageDto_WhenImageLinkAndAltTextProvided_ReturnsImageDto()
    {
        var result = PageHelper.GetImageDto("https://example.com/image.png", "Alt text");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Url, Is.EqualTo("https://example.com/image.png"));
            Assert.That(result.AltText, Is.EqualTo("Alt text"));
        });
    }

    [Test]
    public void GetImageDto_WhenImageLinkProvidedAndAltTextIsNull_ThrowsInvalidStateException()
    {
        Assert.Throws<InvalidStateException>(() =>
            PageHelper.GetImageDto("https://example.com/image.png", null));
    }

    [Test]
    public void GetImageDto_WhenImageLinkIsEmptyString_ReturnsImageDtoWithEmptyUrl()
    {
        var result = PageHelper.GetImageDto(string.Empty, "Alt text");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Url, Is.EqualTo(string.Empty));
            Assert.That(result.AltText, Is.EqualTo("Alt text"));
        });
    }

    [Test]
    public void GetCallToActionDto_WhenButtonTextIsNull_ReturnsNull()
    {
        var result = PageHelper.GetCallToActionDto("Description", null, "https://example.com", "top");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetCallToActionDto_WhenButtonLinkIsNull_ReturnsNull()
    {
        var result = PageHelper.GetCallToActionDto("Description", "Click here", null, "top");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetCallToActionDto_WhenButtonTextAndLinkAreNull_ReturnsNull()
    {
        var result = PageHelper.GetCallToActionDto("Description", null, null, "top");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetCallToActionDto_WhenAllValuesProvided_ReturnsCallToActionDto()
    {
        var result = PageHelper.GetCallToActionDto(
            "Description", "Click here", "https://example.com", "top");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Description, Is.EqualTo("Description"));
            Assert.That(result.ButtonTitle, Is.EqualTo("Click here"));
            Assert.That(result.ButtonUrl, Is.EqualTo("https://example.com"));
            Assert.That(result.Position, Is.EqualTo("top"));
        });
    }

    [Test]
    public void GetCallToActionDto_WhenDescriptionIsNull_ReturnsDtoWithNullDescription()
    {
        var result = PageHelper.GetCallToActionDto(
            null, "Click here", "https://example.com", "top");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Description, Is.Null);
            Assert.That(result.ButtonTitle, Is.EqualTo("Click here"));
            Assert.That(result.ButtonUrl, Is.EqualTo("https://example.com"));
        });
    }

    [Test]
    public void GetCallToActionDto_WhenPositionIsNull_ReturnsDtoWithNullPosition()
    {
        var result = PageHelper.GetCallToActionDto(
            "Description", "Click here", "https://example.com", null);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Position, Is.Null);
        });
    }
}
