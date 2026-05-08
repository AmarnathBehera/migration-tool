using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Utilities;
using CMS.Base;
using CMS.ContentEngine;
using CMS.Websites;
using HotChocolate.Types.Pagination;
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
 SectionPeoplePage sectionPeople,
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
                 LanguageName = _cultureContext.CurrentLanguageTag
             },
            query =>
            {
                query.Where(w =>
                {
                    w.WhereEquals("ContentItemChannelID", _contentService.GetCurrentChannelID());
                    w.WhereEquals("WebPageItemParentID", sectionPeople.SystemFields.WebPageItemID);
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
        var peopleArray = people?.ToArray() ?? Array.Empty<Person>();

        var cardDtos = await CreatePeopleCardDtosAsync(peopleArray, cancellationToken);

        return new PaginationDto<PersonCardDto>
        {
            Data = cardDtos,
            TotalCount = peopleArray.Length
        };
    } //private static void ApplyCommonFilter(ContentItemQueryBuilder builder)
    //{
    //    builder.Parameters(p => p.Where(where =>
    //    {
    //        where.WhereEquals(nameof(Person.IsSearchExcluded), false);
    //        where.Or(or => or.WhereNull(nameof(Person.IsSearchExcluded)));
    //    }));
    //}
    private async Task<List<Action<WhereParameters>>> GetFilterParameterWhereExpressionsAsync(
     IDictionary<string, string> filterParameterValueByName)
    {
        string culture = _cultureContext.CurrentLanguageTag;

        var filters = new List<Action<WhereParameters>>();

        foreach (var (filterName, filterValue) in filterParameterValueByName)
        {
            var filterData = FilterUtility.GetFilterParameterData(filterName);

            if (filterData == null)
                continue;

            if (filterData.FilterParameterType == FilterParameterType.Page)
            {
                var page = await _contentService.GetPageByTypeAndNameAsync(filterData.EntityType, filterValue);

                if (page is null)
                {
                    continue;
                }
                //Expression<Func<Person, bool>>? whereExpression = filterName switch
                //{
                //    //FilterParameterName.BusinessLine => await BuildBusinessLineWhereExpression(page.NodeAliasPath),
                //    //FilterParameterName.ServiceArea => await BuildServiceAreaWhereExpression(page.NodeAliasPath),
                //    //FilterParameterName.Service => await BuildServiceWhereExpression(page.NodeAliasPath),
                //    //FilterParameterName.SubService => x => x.MetadataSubServices.Contains(page.SystemFields.WebPageItemID),
                //    //FilterParameterName.IndustryCategory => await BuildIndustryCategoryWhereExpression(page.NodeAliasPath),
                //    //FilterParameterName.IndustryPage => await BuildIndustryWhereExpression(page.NodeAliasPath),
                //    //FilterParameterName.IndustryService => x => x.MetadataIndustryServices.Contains(page.SystemFields.WebPageItemID),
                //    //FilterParameterName.SpecialtyCategory => await BuildSpecialtyCategoryWhereExpression(page.NodeAliasPath),
                //    //FilterParameterName.SpecialtyArea => await BuildSpecialtyAreaWhereExpression(page.NodeAliasPath),
                //    //FilterParameterName.Specialty => x => x.MetadataSpecialtiesPages.Contains(page.SystemFields.WebPageItemID),
                //    //FilterParameterName.MicrositeMasterPage => await BuildMicrositeMasterPageWhereExpression(page.NodeAliasPath),
                //    FilterParameterName.OfficeLocation => x => x.MetadataOfficeLocation.Any(o => o.SystemFields.WebPageItemID == page.SystemFields.WebPageItemID),
                //    FilterParameterName.City => x => x.MetadataCity.Any(o => o.SystemFields.WebPageItemID == page.SystemFields.WebPageItemID),
                //    _ => null,
                //};
                Action<WhereParameters>? filterAction = filterName switch
                {
                    FilterParameterName.OfficeLocation => w =>
                        w.WhereContains(nameof(Person.MetadataOfficeLocation), page.SystemFields.ContentItemGUID.ToString()),
                    FilterParameterName.City => w =>
                        w.WhereContains(nameof(Person.MetadataCity), page.SystemFields.ContentItemGUID.ToString()),
                    _ => null,
                };


                if (filterAction is not null)
                {
                    filters.Add(filterAction);
                }
            }
            else if (filterData.FilterParameterType == FilterParameterType.Taxonomy)
            {
                switch (filterName)
                {
                    case FilterParameterName.Credential:
                        {
                            var tag = await _taxonomyTagRetriever.GetTaxonomyTagByNameAsync(filterValue);
                            if (tag == null)
                                break;
                            var identifiers = new List<Guid> { tag.TagGUID };
                            if (identifiers.Any())
                            {
                                filters.Add(w =>
                                    w.WhereContainsTags(nameof(Person.PersonCredentials), identifiers));
                            }
                            break;
                        }

                    case FilterParameterName.ProfileGroup:
                        {
                            var tag = await _taxonomyTagRetriever.GetTaxonomyTagByNameAsync(filterValue);
                            if (tag == null)
                                break;
                            var identifiers = new List<Guid> { tag.TagGUID };
                            if (identifiers.Any())
                            {
                                filters.Add(w =>
                                    w.WhereContainsTags(nameof(Person.MetadataProfileGroups), identifiers));
                            }
                            break;
                        }
                }
            }
        }

        return filters;
    }
    private async Task<PersonCardDto[]> CreatePeopleCardDtosAsync(
    Person[] people,
    CancellationToken cancellationToken)
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