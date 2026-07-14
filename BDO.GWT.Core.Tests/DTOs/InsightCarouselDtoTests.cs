using System.Text.Json;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using NUnit.Framework;
using System;
using System.Linq;

namespace BDO.GWT.Core.Tests.DtoTests;

public class InsightCarouselDtoTests
{
    [Test]
    public void LinkItemDto_DefaultTarget_Is_Self()
    {
        var link = new LinkItemDto
        {
            Title = "Read more",
            URL = "https://example.com"
        };

        Assert.That(link.Target, Is.EqualTo("_self"));
    }

    [Test]
    public void ContentPageCardDto_Serialization_RoundTrip_PreservesMainFields()
    {
        var card = new ContentPageCardDto
        {
            Title = "Card Title",
            Description = "Desc",
            ImageURL = "/media/img.png",
            ImageAltText = "Alt",
            IsFeatured = true,
            isComingSoon = false,
            Type = "Article",
            Date = new DateTime(2025, 12, 25),
            UpdatedOnDate = DateTime.UtcNow,
            ReadMoreLink = new LinkItemDto { Title = "Read", URL = "https://read" }
        };

        var json = JsonSerializer.Serialize(card);
        var deserialized = JsonSerializer.Deserialize<ContentPageCardDto>(json);

        Assert.Multiple(() =>
        {
            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized!.Title, Is.EqualTo(card.Title));
            Assert.That(deserialized.Description, Is.EqualTo(card.Description));
            Assert.That(deserialized.ReadMoreLink?.URL, Is.EqualTo(card.ReadMoreLink.URL));
        });
    }

    [Test]
    public void FilterItemDto_Serialization_PreservesOptions()
    {
        var filter = new FilterItemDto<OptionDto>
        {
            Title = "Business Line",
            Name = "businessLine",
            Type = "select",
            Options = new[] { new OptionDto { Value = "1", Label = "One" } },
            DefaultValue = "1"
        };

        var json = JsonSerializer.Serialize(filter);
        var deserialized = JsonSerializer.Deserialize<FilterItemDto<OptionDto>>(json);

        Assert.Multiple(() =>
        {
            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized!.Name, Is.EqualTo(filter.Name));
            Assert.That(deserialized.Options.Length, Is.EqualTo(1));
            Assert.That(deserialized.Options[0].Label, Is.EqualTo("One"));
        });
    }

    [Test]
    public void InsightCarouselDto_Serialization_RoundTrip_PreservesFields()
    {
        var carousel = new InsightCarouselDto
        {
            Title = "Insights",
            Description = "Carousel of insights",
            Filters = new[] { new FilterItemDto<OptionDto> { Name = "businessLine", Title = "BL", Type = "select", Options = Array.Empty<OptionDto>() } },
            FilteringTooltipEnabled = true,
            Items = new[] { new ContentPageCardDto { Title = "Item1", ReadMoreLink = new LinkItemDto { Title = "Read", URL = "https://r" } } },
            BackgroundImageURL = "/media/bg.png",
            BackgroundImageAltText = "BG",
            SeeAllLink = new LinkItemDto { Title = "See all", URL = "https://all" },
            TopMargin = "mt-4",
            BottomMargin = "mb-4",
            WidgetID = "widget-1",
            StateGUID = Guid.NewGuid(),
            CurrentPageGuid = Guid.NewGuid(),
            PageType = "Insight"
        };

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var json = JsonSerializer.Serialize(carousel, options);
        var deserialized = JsonSerializer.Deserialize<InsightCarouselDto>(json, options);

        Assert.Multiple(() =>
        {
            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized!.Title, Is.EqualTo(carousel.Title));
            Assert.That(deserialized.Items.Count(), Is.EqualTo(1));
            Assert.That(deserialized.Filters, Is.Not.Null);
            Assert.That(deserialized.SeeAllLink?.URL, Is.EqualTo("https://all"));
            Assert.That(deserialized.TopMargin, Is.EqualTo("mt-4"));
        });
    }

    [Test]
    public void InsightCarouselDto_WithMinimalProperties_Serializes()
    {
        var carousel = new InsightCarouselDto
        {
            FilteringTooltipEnabled = false,
            Items = Array.Empty<ContentPageCardDto>(),
            TopMargin = string.Empty,
            BottomMargin = string.Empty
        };

        var json = JsonSerializer.Serialize(carousel);
        var deserialized = JsonSerializer.Deserialize<InsightCarouselDto>(json);

        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.FilteringTooltipEnabled, Is.False);
        Assert.That(deserialized.Items, Is.Empty);
    }
}
