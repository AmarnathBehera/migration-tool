using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Helper;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Utilities;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services;

public interface ICardsService
{
    Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(HashSet<string> filterParameterNames, string? nodeAliasPath, CancellationToken cancellationToken);

}

public class CardsService : ICardsService
{
    private readonly IContentService _contentService;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly ITaxonomyTagRetriever _taxonomyTagRetriever;
    private readonly ICultureContext _cultureContext;

    public CardsService(
        IContentService contentService,
        IWebsiteChannelContext websiteChannelContext,
        ITaxonomyTagRetriever taxonomyTagRetriever,
        ICultureContext cultureContext
        )
    {
        _contentService = contentService;
        _websiteChannelContext = websiteChannelContext;
        _taxonomyTagRetriever = taxonomyTagRetriever;
        _cultureContext = cultureContext;
    }

    public async Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(HashSet<string> filterParameterNames, string? nodeAliasPath, CancellationToken cancellationToken)
    {
        var filterItems = new List<FilterItemDto<OptionDto>>();

        foreach (var filterParameterName in filterParameterNames)
        {
            var filterParameterData = FilterUtility.GetFilterParameterData(filterParameterName);

            if (filterParameterData is null)
            {
                continue;
            }

            var options = filterParameterData.FilterParameterType switch
            {
                FilterParameterType.Page => await BuildOptionsForPageFilterAsync(filterParameterData, cancellationToken),
                // FilterParameterType.Entity => await BuildOptionsForEntityFilterAsync(filterParameterData, cancellationToken),
                 FilterParameterType.Field => await BuildOptionsForFieldFilterAsync(filterParameterData, cancellationToken),
                FilterParameterType.Taxonomy => await BuildOptionsForTaxonomyFilterAsync(filterParameterData, cancellationToken),
                _ => throw new NotSupportedException($"Filter parameter type {filterParameterData.FilterParameterType} is not supported."),
            };

            var filterItem = FilterDtoUtility.CreateFilterItemDto(FilterItemType.Select, filterParameterName, filterParameterData.LiveSiteDisplayName, null, options);

            filterItems.Add(filterItem);
        }

        return filterItems.ToArray();
    }
    private async Task<OptionDto[]> BuildOptionsForPageFilterAsync(FilterParameterData filterParameterData, CancellationToken cancellationToken)
    {
        var entityType = filterParameterData.EntityType;

        var items = await _contentService.GetItemsByTypesAsync(
            new[] { entityType },
            _websiteChannelContext.WebsiteChannelID, 
            cancellationToken);

        var options = items
      .Select(x =>
      {
          var type = x.GetType();

          var metadataTitle =
              type.GetProperty("MetadataTitle")?.GetValue(x) as string;

          var metadataOgTitle =
              type.GetProperty("MetadataOGTitle")?.GetValue(x) as string;

          var systemFields =
              type.GetProperty("SystemFields")?.GetValue(x);

          var contentItemName =
              systemFields?.GetType()
                  .GetProperty("ContentItemName")
                  ?.GetValue(systemFields) as string;

          var nameBI = type.GetProperty("Name")?.GetValue(x) as string;

          string name = string.Empty;

          

          name = metadataTitle ?? metadataOgTitle ?? contentItemName;

          if (type.Name.ToString() == "Businessissues")
          {
              name = nameBI;
          }

          return new OptionDto
          {
              Value = name,
              Label = name
          };
      })
      .Where(x => !string.IsNullOrWhiteSpace(x.Value))
      .OrderBy(x => x.Label)
      .ToArray();

        return options;

    }
    private async Task<OptionDto[]> BuildOptionsForTaxonomyFilterAsync(FilterParameterData filterParameterData, CancellationToken cancellationToken)
    {
        var tags = await _taxonomyTagRetriever.GetTaxonomyTagsAsync(
            filterParameterData.AdminDisplayName, 
            _cultureContext.CurrentLanguageTag, 
            null, 
            cancellationToken);

        var options = tags
            .Select(tag => new OptionDto
            {
                Value = tag.Name ,
                Label = tag.Name
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .OrderBy(x => x.Label)
            .ToArray();

        return options;
    }

    private async Task<OptionDto[]> BuildOptionsForFieldFilterAsync(FilterParameterData filterParameterData, CancellationToken cancellationToken)
    {
        var entityType = filterParameterData.EntityType;
        var options = Array.Empty<OptionDto>();

        var items = await _contentService.GetItemsByTypesAsync(
            new[] { entityType },
            _websiteChannelContext.WebsiteChannelID,
            cancellationToken);

        if(items is null || !items.Any())
        {
            return options;
        }
       
        switch (filterParameterData.EntityType)
        {
            case { } type when type == typeof(BlogPost):
                options = items
                    .Select(x => x.GetType().GetProperty(nameof(BlogPost.MetadataKeywordsBlog))?.GetValue(x) as string)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .SelectMany(x => KeywordsHelper.ParseKeywords(x!))
                    .Distinct()
                    .Select(x => FilterDtoUtility.CreateOptionDto(x, x))
                    .ToArray();
                break;
        }

        return options;
    }
}
