using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Utilities;
using CMS.Websites.Routing;

namespace BDO.GWT.Core.Services;

public interface ICardsService
{
    Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(HashSet<string> filterParameterNames, string? nodeAliasPath, CancellationToken cancellationToken);

}

public class CardsService : ICardsService
{
    private readonly IContentService _contentService;
    private readonly IWebsiteChannelContext _websiteChannelContext;

    public CardsService(
        IContentService contentService,
        IWebsiteChannelContext websiteChannelContext
        )
    {
        _contentService = contentService;
        _websiteChannelContext = websiteChannelContext;
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
                var displayName =
                    GetStringProperty(x, type, "MetadataTitle")
                    ?? GetStringProperty(x, type, "PageTitle")
                    ?? GetStringProperty(x, type, "Name")
                    ?? GetStringProperty(x, type, "MetadataOGTitle")
                    ?? GetContentItemName(x, type);

                return new OptionDto
                {
                    Value = displayName,
                    Label = displayName
                };
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .OrderBy(x => x.Label)
            .ToArray();

        return options;
    }

    private static string? GetStringProperty(object instance, Type type, string propertyName)
    {
        var value = type.GetProperty(propertyName)?.GetValue(instance) as string;
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string? GetContentItemName(object instance, Type type)
    {
        var systemFields = type.GetProperty("SystemFields")?.GetValue(instance);
        if (systemFields is null)
        {
            return null;
        }

        return systemFields.GetType()
            .GetProperty("ContentItemName")
            ?.GetValue(systemFields) as string;
    }

}
