using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using CMS.Websites;
using System.Globalization;

namespace BDO.GWT.Web.Core.Services;

internal static class InsightCardsGridFactory
{
    public static HashSet<string> CreateFilterNames(string? configuredFilters)
    {
        return string.IsNullOrWhiteSpace(configuredFilters)
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : configuredFilters
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public static async Task<CardsGridDto> CreateAsync(
        Guid pageGuid,
        SectionInsightsPage? insightSection,
        IInsightCardService insightCardService,
        IFilteringHelper filteringHelper,
        IWebPageUrlRetriever webPageUrlRetriever,
        bool includeTitleAndDescription = false,
        CancellationToken cancellationToken = default)
    {
        var filterNames = CreateFilterNames(insightSection?.InsightListFilters);
        var filteringEnabled = insightSection?.ItemListFilteringEnabled == true;
        var createFiltersTask = insightCardService.CreateFiltersAsync(filterNames, null, cancellationToken);
        var advancedSearchPage = insightSection?.ItemListAdvancedSearchLinkUrl?.FirstOrDefault();
        var advancedSearchLink = (string?)null;

        if (advancedSearchPage is not null)
        {
            var currentLanguage = CultureInfo.CurrentUICulture.Name;
            var webPageUrl = await webPageUrlRetriever.Retrieve(
                advancedSearchPage.WebPageGuid,
                languageName: string.IsNullOrWhiteSpace(currentLanguage) ? null : currentLanguage,
                cancellationToken: cancellationToken);

            advancedSearchLink = webPageUrl.RelativePath;
        }
       
        var cardsGrid = new CardsGridDto
        {
            Filters = filteringEnabled ? await createFiltersTask : null,
            FilteringTooltipEnabled = await filteringHelper.IsFilteringTooltipEnabledAsync(filteringEnabled, cancellationToken),
            PageSize = 20,
            ApiEndpointName = $"Insights/{pageGuid}",
            AdvancedSearchPrelinkText = insightSection?.ItemListAdvancedSearchDescription,
            AdvancedSearchLinkText = insightSection?.ItemListAdvancedSearchLinkText,
            AdvancedSearchLink = advancedSearchLink
        };

        if (includeTitleAndDescription)
        {
            cardsGrid.Title = insightSection?.ItemListTitle;
            cardsGrid.Description = insightSection?.ItemListDescription;
        }

        return cardsGrid;
    }
}
