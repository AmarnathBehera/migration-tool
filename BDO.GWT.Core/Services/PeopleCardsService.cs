using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Utilities;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using CMS.DataEngine;

namespace BDO.GWT.Core.Services;
public interface IPeopleCardsService
{
    Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(
        HashSet<string> filters,
        string? nodeAliasPath,
        CancellationToken cancellationToken);

    Task<PaginationDto<PersonCardDto>> GetFilteredItemsAsync(
        SectionPeoplePage page,
        CardsQuery cardsQuery,
        Action<ContentItemQueryBuilder>? decorateQuery,
        Func<Person, bool> isFeatured,
        CancellationToken cancellationToken);
}

public class PeopleCardsService : IPeopleCardsService
{
    private readonly ICardsService _cardsService;
    private readonly IContentService _contentService;
    private readonly ITaxonomyTagRetriever _taxonomyTagRetriever;
    private readonly ICultureContext _cultureContext;

    public PeopleCardsService(
        ICardsService cardsService,
        IContentService contentService,
        ITaxonomyTagRetriever taxonomyTagRetriever,
        ICultureContext cultureContext)
    {
        _cardsService = cardsService;
        _contentService = contentService;
        _taxonomyTagRetriever = taxonomyTagRetriever;
        _cultureContext = cultureContext;
    }

    public async Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(
        HashSet<string> filters,
        string? nodeAliasPath,
        CancellationToken cancellationToken)
    {
        return await _cardsService.CreateFiltersAsync(filters, nodeAliasPath, cancellationToken);
    }

    public async Task<PaginationDto<PersonCardDto>> GetFilteredItemsAsync(
 SectionPeoplePage page,
 CardsQuery cardsQuery,
 Action<ContentItemQueryBuilder>? decorateQuery,
 Func<Person, bool> isFeatured,
 CancellationToken cancellationToken)
    {
        var filterWhereExpressions = await GetFilterParameterWhereExpressionsAsync(cardsQuery.Filters);

        var order = cardsQuery.Order?.ToLower() ?? "asc";

        var people = await _contentService.RetrievePagesAsync<Person>(
               new RetrievePagesParameters
               {
                   LinkedItemsMaxLevel = 2,
                   LanguageName = _cultureContext.CurrentLanguageTag,
                   UseLanguageFallbacks = false 
               },
            query =>
            {
                query.Where(w =>
                {
                    w.WhereEquals("ContentItemChannelID", _contentService.GetCurrentChannelID());
                    w.WhereEquals("WebPageItemParentID", page.SystemFields.WebPageItemID);
                    foreach (var filter in filterWhereExpressions)
                    {
                        filter(w);
                    }
                });
                if (order == "desc") {
                    query.OrderBy(new OrderByColumn(nameof(Person.PersonLastName), OrderDirection.Descending));
                }
                else if (order == "asc") {
                    query.OrderBy(new OrderByColumn(nameof(Person.PersonLastName), OrderDirection.Ascending));
                }
                else {
                    query.OrderBy(nameof(Person.SystemFields.WebPageItemID));
                }
            },
            cancellationToken
        );
        var peopleArray = people?.OfType<Person>().ToArray() ?? Array.Empty<Person>();

        var cardDtos = await CreatePeopleCardDtosAsync(peopleArray);

        return new PaginationDto<PersonCardDto>
        {
            Data = cardDtos,
            TotalCount = peopleArray.Length
        };
    } 
    private async Task<List<Action<WhereParameters>>> GetFilterParameterWhereExpressionsAsync(
     IDictionary<string, string> filterParameterValueByName)
    {
        var filters = new List<Action<WhereParameters>>();

        foreach (var (filterName, filterValue) in filterParameterValueByName)
        {
            var filterData = FilterUtility.GetFilterParameterData(filterName);

            if (filterData == null)
                continue;

            var filter = filterData.FilterParameterType switch
            {
                FilterParameterType.Page => await GetPageFilterAsync(filterName, filterValue),
                FilterParameterType.Taxonomy => await GetTaxonomyFilterAsync(filterName, filterValue),
                _ => null,
            };

            if (filter is not null)
            {
                filters.Add(filter);
            }
        }

        return filters;
    }

    private async Task<Action<WhereParameters>?> GetPageFilterAsync(string filterName, string filterValue)
    {
        var page = await _contentService.GetPageByTypeAndNameAsync(
            FilterUtility.GetFilterParameterData(filterName)!.EntityType, filterValue);

        if (page is null)
            return null;

        var guid = page.SystemFields.ContentItemGUID.ToString();

        return filterName switch
        {
            FilterParameterName.OfficeLocation => w => w.WhereContains(nameof(Person.MetadataOfficeLocation), guid),
            FilterParameterName.City => w => w.WhereContains(nameof(Person.MetadataCity), guid),
            _ => null,
        };
    }

    private async Task<Action<WhereParameters>?> GetTaxonomyFilterAsync(string filterName, string filterValue)
    {
        var tag = await _taxonomyTagRetriever.GetTaxonomyTagByNameAsync(filterValue);
        if (tag == null)
            return null;

        var identifiers = new List<Guid> { tag.TagGUID };

        return filterName switch
        {
            FilterParameterName.Credential => w => w.WhereContainsTags(nameof(Person.PersonCredentials), identifiers),
            FilterParameterName.ProfileGroup => w => w.WhereContainsTags(nameof(Person.MetadataProfileGroups), identifiers),
            _ => null,
        };
    }
    private async Task<PersonCardDto[]> CreatePeopleCardDtosAsync(
    Person[] people)
    {
        var dtos = people.Select(person => new PersonCardDto
        {
            DisplayName = _contentService.GetFallbackMetadataDescription(
                person.MetadataTitle,
                person.MetadataOGTitle),

            TeaserImageUrl = person.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
            TeaserImageAltText = person.MetadataTeaserImageAltText,
            JobTitle = person.PersonJobTitle,
            PersonUrl = person.GetUrl().AbsoluteUrl
        }).ToArray();

        return (dtos);
    }
}