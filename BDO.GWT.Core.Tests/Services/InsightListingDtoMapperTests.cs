using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class InsightListingDtoMapperTests
{
    [Test]
    public async Task MapToInsightCardListingDtoAsync_MapsExpectedFields()
    {
        var heroServiceMock = new Mock<IHeroService>();
        var expectedHero = new HeroDto
        {
            Layout = "layout",
            Title = "hero"
        };

        heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<BDO.GWT.Core.Data.HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHero);

        var expectedGrid = new CardsGridDto
        {
            FilteringTooltipEnabled = false,
            ApiEndpointName = "Insights/test"
        };

        var guid = Guid.NewGuid();
        var page = new FakeInsightListingPage
        {
            MetadataTitle = "Metadata title",
            MetadataDescription = "Metadata description",
            MetadataCanonical = "/insights/test",
            MetadataTeaserImage = Array.Empty<ContentAttachment>(),
            MetadataOGImage = Array.Empty<ContentAttachment>(),
            MetadataOGTwitterImage = Array.Empty<ContentAttachment>(),
            HeroBackgroundImage = Array.Empty<ContentAttachment>(),
            HeroBackgroundImage_Mobile = Array.Empty<ContentAttachment>(),
            HeroCallToActionButtonUrl = Array.Empty<IWebPageFieldsSource>(),
            SystemFields = new WebPageFields
            {
                ContentItemID = 1,
                ContentItemGUID = guid,
                WebPageItemGUID = guid,
                WebPageItemID = 10,
                WebPageItemTreePath = "/Insights/Test"
            }
        };

        var result = await InsightListingDtoMapper.MapToInsightCardListingDtoAsync(
            page,
            heroServiceMock.Object,
            _ => Task.FromResult(expectedGrid),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo("Metadata title"));
            Assert.That(result.Hero, Is.SameAs(expectedHero));
            Assert.That(result.CardsGrid, Is.SameAs(expectedGrid));
            Assert.That(result.BaseContent.ContentItemID, Is.EqualTo(1));
            Assert.That(result.BaseWebPage.WebPageItemID, Is.EqualTo(10));
            Assert.That(result.MetaData.MetaCanonical, Is.EqualTo("/insights/test"));
        });
    }

    private sealed class FakeInsightListingPage : IInsightListingPage
    {
        public WebPageFields? SystemFields { get; init; }
        public string? MetadataTitle { get; init; }
        public string? MetadataDescription { get; init; }
        public IEnumerable<ContentAttachment> MetadataTeaserImage { get; init; } = Array.Empty<ContentAttachment>();
        public string? MetadataTeaserImageAltText { get; init; }
        public string? MetadataCanonical { get; init; }
        public bool MetadataNofollow { get; init; }
        public bool MetadataNoindex { get; init; }
        public bool IncludeInSitemap { get; init; }
        public string? MetadataOGTitle { get; init; }
        public string? MetadataOGDescription { get; init; }
        public IEnumerable<ContentAttachment> MetadataOGImage { get; init; } = Array.Empty<ContentAttachment>();
        public IEnumerable<ContentAttachment> MetadataOGTwitterImage { get; init; } = Array.Empty<ContentAttachment>();
        public bool MetadataOGTwitterImageSizeCheckbox { get; init; }
        public string? HeroLayout { get; init; }
        public string? HeroTitle { get; init; }
        public string? HeroDescription { get; init; }
        public IEnumerable<ContentAttachment> HeroBackgroundImage { get; init; } = Array.Empty<ContentAttachment>();
        public string? HeroBackgroundImageAltText { get; init; }
        public IEnumerable<ContentAttachment> HeroBackgroundImage_Mobile { get; init; } = Array.Empty<ContentAttachment>();
        public string? HeroBackgroundImage_MobileAltText { get; init; }
        public string? HeroCallToActionButtonType { get; init; }
        public string? HeroCallToActionButtonText { get; init; }
        public IEnumerable<IWebPageFieldsSource> HeroCallToActionButtonUrl { get; init; } = Array.Empty<IWebPageFieldsSource>();
    }
}
