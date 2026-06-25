using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helpers;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services;

public interface IKeyContactsService
{
    Task<PeopleGridDto> GetKeyContactsAsync(KeyContactsData data, CancellationToken cancellationToken = default);
}

public class KeyContactsService : IKeyContactsService
{
    private readonly IContentService _contentService;
    private readonly ICultureContext _cultureContext;
    private readonly IPersonPageService _personpageService;
    private readonly IExternalPeopleHelper _externalPeopleHelper;

    public KeyContactsService(
        IContentService contentService,
        ICultureContext cultureContext,
        IPersonPageService personpageService,
        IExternalPeopleHelper externalPeopleHelper)
    {
        _contentService = contentService;
        _cultureContext = cultureContext;
        _personpageService = personpageService;
        _externalPeopleHelper = externalPeopleHelper;
    }

    public async Task<PeopleGridDto> GetKeyContactsAsync(KeyContactsData data, CancellationToken cancellationToken = default)
    {
        var peopleParameters = new RetrievePagesParameters
        {
            LinkedItemsMaxLevel = 2,
            LanguageName = _cultureContext.CurrentLanguageTag,
            UseLanguageFallbacks = false
        };
        var contributorParameters = new RetrievePagesParameters
        {
            LinkedItemsMaxLevel = 2,
            LanguageName = _cultureContext.CurrentLanguageTag,
            UseLanguageFallbacks = false
        };

        var peopleTask = _contentService.GetPagesAsync<Person>(
            peopleParameters,
            query => query.Where(w => w.WhereIn("ContentItemGUID", data.OrderedPeopleNodeGuids.Select(x => x.Identifier))),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var externalContributorsTask = _contentService.GetPagesAsync<ExternalPerson>(
            contributorParameters,
            query => query.Where(w => w.WhereIn("ContentItemGUID", data.OrderedContributorsNodeGuids.Select(x => x.Identifier))),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var internalContributorsTask = _contentService.GetPagesAsync<InternalPerson>(
            contributorParameters,
            query => query.Where(w => w.WhereIn("ContentItemGUID", data.OrderedContributorsNodeGuids.Select(x => x.Identifier))),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        await Task.WhenAll(peopleTask, externalContributorsTask, internalContributorsTask);

        var people = await peopleTask;
        var externalContributors = await externalContributorsTask;
        var internalContributors = await internalContributorsTask;

        var peopleCards = await Task.WhenAll(people.Select(async person => new KeyContactCardDto
        {
            DisplayName = _personpageService.GetPersonDisplayNameAsync(person),
            FirstName = person.PersonFirstName,
            LastName = person.PersonLastName,
            ContentItemGuid = person.SystemFields.ContentItemGUID,
            TeaserImageUrl = person.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset.Url,
            TeaserImageAltText = person.MetadataTeaserImageAltText,
            JobTitle = person.PersonJobTitle,
            OfficeNames = data.ShowOffice ?await RetrieveLocationOfficeNamesAsync(person, cancellationToken) : null,
            PersonUrl = person.GetUrl().RelativePath,
        }));

        var internalCards = await Task.WhenAll(internalContributors.Select(async contributorPage => new KeyContactCardDto
        {
            DisplayName = await _externalPeopleHelper.GetDisplayNameAsyncInternalPeople(contributorPage),
            FirstName = contributorPage.FirstName,
            LastName = contributorPage.LastName,
            ContentItemGuid = contributorPage.SystemFields.ContentItemGUID,
            TeaserImageUrl = contributorPage.ImageFile?.FirstOrDefault()?.ContentAttachmentAsset.Url,
            TeaserImageAltText = contributorPage.ImageFileAltText,
            JobTitle = contributorPage.JobTitle,
            PersonUrl = contributorPage.Url
        }));

        var externalCards = await Task.WhenAll(externalContributors.Select(async contributorPage => new KeyContactCardDto
        {
            DisplayName = await _externalPeopleHelper.GetDisplayNameAsync(contributorPage),
            FirstName = contributorPage.FirstName,
            LastName = contributorPage.LastName,
            ContentItemGuid = contributorPage.SystemFields.ContentItemGUID,
            TeaserImageUrl = contributorPage.ImageFile?.FirstOrDefault()?.ContentAttachmentAsset.Url,
            TeaserImageAltText = contributorPage.ImageFileAltText,
            JobTitle = contributorPage.JobTitle,
            PersonUrl = contributorPage.Url
        }));
        var cards = peopleCards.Concat(internalCards).ToList();

        IEnumerable<KeyContactCardDto> orderedCards;

        if (data.IsOrderOverriden)
        {
            orderedCards = OrderPersonCardByCriteria(cards, data.OrderCriteria, data.IsDescendingOrder);
        }
        else
        {
            var orderByContentItemGuid = data.OrderedPeopleNodeGuids
                .Concat(data.OrderedContributorsNodeGuids)
                .Select((guid, index) => (guid.Identifier, index))
                .ToDictionary(pair => pair.Identifier, pair => pair.index);

            orderedCards = cards.OrderBy(x => orderByContentItemGuid.GetValueOrDefault(x.ContentItemGuid, int.MaxValue));
        }

        var dto = new PeopleGridDto
        {
            Title = data.Title,
            Description = data.Description,
            Items = orderedCards.Select(MapToPersonCard).ToArray(),
            ExternalItems = externalCards.Select(MapToPersonCard).ToArray(),
            SeeAllLink = data is { SeeAllLinkText: not null, SeeAllLinkUrl: not null }
                ? new LinkItemDto
                {
                    Title = data.SeeAllLinkText,
                    URL = data.SeeAllLinkUrl,
                    Target = data.SeeAllLinkURLTarget ?? "_self",
                }
                : null,
            TopMargin = data.TopMargin,
            BottomMargin = data.BottomMargin,
            WidgetID = data.WidgetID,
        };

        return dto;
    }
    private static PersonCardDto MapToPersonCard(KeyContactCardDto card) => new()
    {
        DisplayName = card.DisplayName,
        TeaserImageUrl = card.TeaserImageUrl,
        TeaserImageAltText = card.TeaserImageAltText,
        JobTitle = card.JobTitle,
        PersonUrl = card.PersonUrl,
        OfficeNames = card.OfficeNames,
    };

    private static IEnumerable<KeyContactCardDto> OrderPersonCardByCriteria(IEnumerable<KeyContactCardDto> cards, string? orderCriteria, bool isDescendingOrder)
    {
        if (orderCriteria is null)
        {
            return cards;
        }

        Func<KeyContactCardDto, string?> orderBy = orderCriteria switch
        {
            PeopleOrderCriteria.FirstName => x => x.FirstName,
            PeopleOrderCriteria.LastName => x => x.LastName,
            PeopleOrderCriteria.JobTitle => x => x.JobTitle,
            _ => throw new NotSupportedException($"Ordering by {orderCriteria} is not supported."),
        };

        return (isDescendingOrder ? cards.OrderByDescending(orderBy) : cards.OrderBy(orderBy)).ToArray();
    }
    public static async Task<IEnumerable<string>> RetrieveLocationOfficeNamesAsync(Person person, CancellationToken cancellationToken)
    {
        var offices = person.MetadataOfficeLocation;

        return offices.Select(o => o.MetadataTitle);
    }
}