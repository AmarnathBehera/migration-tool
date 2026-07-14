using BDO;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class InsightCardsGridFactoryTests
{
    [Test]
    public async Task CreateAsync_ReturnsMappedCardsGrid_WithFilters_WhenFilteringEnabled()
    {
        var insightCardServiceMock = new Mock<IInsightCardService>();
        var filteringHelperMock = new Mock<IFilteringHelper>();
        var webPageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();

        var expectedFilters = new[]
        {
            new FilterItemDto<OptionDto>
            {
                Name = "businessLine",
                Options = Array.Empty<OptionDto>()
            }
        };

        insightCardServiceMock
            .Setup(x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFilters);

        filteringHelperMock
            .Setup(x => x.IsFilteringTooltipEnabledAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var pageGuid = Guid.NewGuid();
        var insightSection = new SectionInsightsPage
        {
            InsightListFilters = "businessLine; serviceArea",
            ItemListFilteringEnabled = true,
            ItemListAdvancedSearchDescription = "Pre-link",
            ItemListAdvancedSearchLinkText = "Advanced",
            ItemListAdvancedSearchLinkUrl = Array.Empty<WebPageRelatedItem>(),
            ItemListTitle = "Section title",
            ItemListDescription = "Section description"
        };

        var result = await InsightCardsGridFactory.CreateAsync(
            pageGuid,
            insightSection,
            insightCardServiceMock.Object,
            filteringHelperMock.Object,
            webPageUrlRetrieverMock.Object,
            includeTitleAndDescription: true,
            cancellationToken: CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Filters, Is.SameAs(expectedFilters));
            Assert.That(result.FilteringTooltipEnabled, Is.True);
            Assert.That(result.ApiEndpointName, Is.EqualTo($"Insights/{pageGuid}"));
            Assert.That(result.Title, Is.EqualTo("Section title"));
            Assert.That(result.Description, Is.EqualTo("Section description"));
            Assert.That(result.AdvancedSearchPrelinkText, Is.EqualTo("Pre-link"));
            Assert.That(result.AdvancedSearchLinkText, Is.EqualTo("Advanced"));
        });

        insightCardServiceMock.Verify(x => x.CreateFiltersAsync(
            It.Is<HashSet<string>>(set =>
                set.Contains("businessLine") &&
                set.Contains("serviceArea") &&
                set.Count == 2),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreateAsync_ReturnsNullFilters_WhenFilteringDisabled()
    {
        var insightCardServiceMock = new Mock<IInsightCardService>();
        var filteringHelperMock = new Mock<IFilteringHelper>();
        var webPageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();

        insightCardServiceMock
            .Setup(x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

        filteringHelperMock
            .Setup(x => x.IsFilteringTooltipEnabledAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await InsightCardsGridFactory.CreateAsync(
            Guid.NewGuid(),
            new SectionInsightsPage
            {
                InsightListFilters = "businessLine",
                ItemListFilteringEnabled = false,
                ItemListAdvancedSearchLinkUrl = Array.Empty<WebPageRelatedItem>()
            },
            insightCardServiceMock.Object,
            filteringHelperMock.Object,
            webPageUrlRetrieverMock.Object,
            cancellationToken: CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Filters, Is.Null);
            Assert.That(result.FilteringTooltipEnabled, Is.False);
        });
    }
}
