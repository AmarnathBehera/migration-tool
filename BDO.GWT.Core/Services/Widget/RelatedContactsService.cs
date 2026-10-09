using BDO;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services;

public interface IRelatedContactsService
{
    Task<PeopleGridDto> GetRelatedContactsDtoAsync<T>(T currentPage, RelatedContactsData data, CancellationToken cancellationToken)
    where T : IWebPageFieldsSource;
}

public class RelatedContactsService : IRelatedContactsService
{
    private readonly IContentService _contentService;
    private readonly ICultureContext _cultureContext;
    private readonly IPersonPageService _personPageService;

    public RelatedContactsService(
    IContentService contentService,
    ICultureContext cultureContext,
    IPersonPageService personPageService)
    {
        _contentService = contentService;
        _cultureContext = cultureContext;
        _personPageService = personPageService;
    }

    public async Task<PeopleGridDto> GetRelatedContactsDtoAsync<T>(T currentPage, RelatedContactsData data, CancellationToken cancellationToken)
        where T : IWebPageFieldsSource
    {
        var currentPageTypeName = currentPage.GetType().Name;
        var guid = currentPage.SystemFields.ContentItemGUID.ToString();
        Action<WhereParameters>? filter;

            filter = currentPageTypeName switch
            {
                nameof(LocationCity) => await BuildLocationCityFilterAsync(currentPage.SystemFields.WebPageItemID, cancellationToken),
                nameof(BusinessLine) => x => x.WhereContains(nameof(Person.MetadataBusinessLines), guid),
                nameof(ServiceArea) => x => x.WhereContains(nameof(Person.MetadataServiceAreasPerson), guid),
                nameof(Service) => x => x.WhereContains(nameof(Person.MetadataServicesPerson), guid),
                nameof(Sub_Service) => x => x.WhereContains(nameof(Person.MetadataSubServices), guid),
                nameof(IndustryCategory) => x => x.WhereContains(nameof(Person.MetadataIndustryCategoriesPerson), guid),
                nameof(Industry) => x => x.WhereContains(nameof(Person.MetadataIndustriesPerson), guid),
                nameof(IndustryService) => x => x.WhereContains(nameof(Person.MetadataIndustryServices), guid),
                nameof(SpecialtiesCategory) => x => x.WhereContains(nameof(Person.MetadataSpecialtiesCategories), guid),
                nameof(SpecialtiesArea) => x => x.WhereContains(nameof(Person.MetadataSpecialtiesAreas), guid),
                nameof(SpecialtiesPage) => x => x.WhereContains(nameof(Person.MetadataSpecialtiesPages), guid),
                nameof(LocationOffice) => x => x.WhereContains(nameof(Person.MetadataOfficeLocation), guid),
                nameof(MicrositeMasterPage) => x => x.WhereContains(nameof(Person.MicrositeMasterPagesPerson), guid),
                nameof(MicrositePage) => x => x.WhereContains(nameof(Person.MicrositePagesPerson), guid),
                _ => null,
            };

        var people = await RetrievePeopleAsync(filter, cancellationToken);
        var cards = BuildCards(people, data);

        return new PeopleGridDto
        {
            Title = data.Title,
            Description = data.Description,
            Items = cards,
            TopMargin = data.TopMargin,
            SeeAllLink = data is { SeeAllLinkText: not null, SeeAllLinkUrl: not null }
                ? new LinkItemDto
                {
                    Title = data.SeeAllLinkText,
                    URL = data.SeeAllLinkUrl,
                    Target = data.SeeAllLinkURLTarget ?? "_self",
                }
                : null,
            BottomMargin = data.BottomMargin,
            WidgetID = data.WidgetID,
        };
    }

    private async Task<Person[]> RetrievePeopleAsync(Action<WhereParameters>? filter, CancellationToken cancellationToken)
    {
        var sectionPersonPages = await _contentService.GetPagesAsync<SectionPeoplePage>(
            new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 0,
                LanguageName = _cultureContext.CurrentLanguageTag,
                UseLanguageFallbacks = false,
            },
            query => { },
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var sectionIds = (sectionPersonPages ?? Enumerable.Empty<SectionPeoplePage>())
            .Select(s => s.SystemFields.WebPageItemID)
            .ToArray();

        if (sectionIds.Length == 0)
        {
            return Array.Empty<Person>();
        }

        var people = await _contentService.GetPagesAsync<Person>(
            new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 2,
                LanguageName = _cultureContext.CurrentLanguageTag,
                UseLanguageFallbacks = false,
            },
            query =>
            {
                query.OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder));
                query.Where(w =>
                {
                    w.WhereIn("WebPageItemParentID", sectionIds);

                    if (filter is not null)
                    {
                        filter(w);
                    }
                });
            },
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        return (people ?? Enumerable.Empty<Person>()).ToArray();
    }

    private List<PersonCardDto> BuildCards(Person[] people, RelatedContactsData data)
    {
        var orderedPeople = OrderPeopleByCriteria(people, data.OrderCriteria, data.IsDescendingOrder).ToArray();

        return orderedPeople.Select(person => new PersonCardDto
        {
            DisplayName = _personPageService.GetPersonDisplayNameAsync(person),
            TeaserImageUrl = person.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset.Url,
            TeaserImageAltText = person.MetadataTeaserImageAltText,
            JobTitle = person.PersonJobTitle,
            PersonUrl = person.GetUrl().RelativePath,
            OfficeNames = data.ShowOffice ? RetrieveLocationOfficeNames(person) : null,
        }).ToList();
    }

    private async Task<IEnumerable<Guid>> GetChildLocationOfficeGuidsAsync(int parentWebPageItemId, CancellationToken cancellationToken)
    {
        var locationOffices = await _contentService.GetChildPagesByParentGuidAsync<LocationOffice>(parentWebPageItemId, cancellationToken);
        return locationOffices.Select(x => x.SystemFields.ContentItemGUID);
    }

    private static IEnumerable<string> RetrieveLocationOfficeNames(Person person)
    {
        if (person.MetadataOfficeLocation is null)
        {
            return Enumerable.Empty<string>();
        }

        return person.MetadataOfficeLocation.Select(o => o.MetadataTitle);
    }

    private static Action<WhereParameters> BuildOrContainsFilter(string fieldName, List<Guid> guids)
    {
        return x =>
        {
            var clause = x.WhereContains(fieldName, guids[0].ToString());
            for (var i = 1; i < guids.Count; i++)
            {
                clause = clause.Or().WhereContains(fieldName, guids[i].ToString());
            }
        };
    }

    private static IEnumerable<Person> OrderPeopleByCriteria(Person[] people, string? orderCriteria, bool isDescending)
    {
        Func<Person, string?> keySelector = orderCriteria switch
        {
            PeopleOrderCriteria.FirstName => p => p.PersonFirstName,
            PeopleOrderCriteria.JobTitle => p => p.PersonJobTitle,
            _ => p => p.PersonLastName,
        };

        return isDescending
            ? people.OrderByDescending(keySelector)
            : people.OrderBy(keySelector);
    }

    private async Task<Action<WhereParameters>> BuildLocationCityFilterAsync(
    int parentWebPageItemId,
    CancellationToken cancellationToken)
    {
        var officeGuids = (await GetChildLocationOfficeGuidsAsync(parentWebPageItemId, cancellationToken)).ToList();

        if (officeGuids.Count == 0)
        {
            return x => x.WhereEquals("WebPageItemID", -1);
        }

        return BuildOrContainsFilter(nameof(Person.MetadataOfficeLocation), officeGuids);
    }
}

