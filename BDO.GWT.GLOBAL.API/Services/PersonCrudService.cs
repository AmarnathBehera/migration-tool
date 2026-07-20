using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Extensions;
using BDO.GWT.GLOBAL.API.Helpers;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IPersonCrudService
{
    Task<ActionResult<IEnumerable<PageOutput<PersonContent>>>> GetPersonPagesAsync(int channelId, string? culture, bool? onlyPublished, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<PersonContent>>> GetPersonPageAsync(int channelId, Guid pageGUID, string? culture, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<PersonContent>>> CreatePageAsync(int channelId, int nodeParentID, string? culture, PersonContent request, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<PersonContent>>> CreatePageCultureVersionAsync(int channelId, Guid pageGUID, string? fromCulture, string? toCulture, PersonContent request, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<PersonContent>>> UpdatePageAsync(int channelId, Guid pageGUID, string? culture, PersonContent request, CancellationToken cancellationToken);
    Task DeletePageAsync(int channelId, Guid pageGUID, string? culture, bool deleteAllCultures, CancellationToken cancellationToken);
}

public class PersonCrudService : IPersonCrudService
{
    private readonly IContentService _contentRetriever;
    private readonly IValidationState _validationState;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly ICultureService _cultureService;
    private readonly IPageContentValidator _pageContentValidator;
    private readonly IInfoProvider<ChannelInfo> _channelInfoProvider;
    private readonly IWebPageManager _webPageManager;
    private readonly IContentItemManager _contentItemManager;
    private readonly IMappingHelper _mappingHelper;
    private static readonly LanguageTag defaultCulture = LanguageTag.GB;
    private const string PERSON_CONTENT_TYPE = Person.CONTENT_TYPE_NAME;
    private const string WEB_PAGE_GUID = "WebPageItemGUID";

#pragma warning disable S107
    public PersonCrudService(
        IContentService contentRetriever,
        IValidationState validationState,
        IWebsiteChannelContext websiteChannelContext,
        ICultureService cultureService,
        IPageContentValidator pageContentValidator,
        IInfoProvider<ChannelInfo> channelInfoProvider,
        IWebPageManagerFactory webPageManagerFactory,
        IContentItemManagerFactory managerFactory,
        IUserInfoProvider userInfoProvider,
        IMappingHelper mappingHelper)
#pragma warning restore S107
    {
        UserInfo user = userInfoProvider.Get("administrator");

        _contentRetriever = contentRetriever;
        _validationState = validationState;
        _websiteChannelContext = websiteChannelContext;
        _cultureService = cultureService;
        _pageContentValidator = pageContentValidator;
        _channelInfoProvider = channelInfoProvider;
        _contentItemManager = managerFactory.Create(user.UserID);
        _webPageManager = webPageManagerFactory.Create(_websiteChannelContext.WebsiteChannelID == 0 ? 1 : _websiteChannelContext.WebsiteChannelID, user.UserID);
        _mappingHelper = mappingHelper;
    }

    public async Task<ActionResult<IEnumerable<PageOutput<PersonContent>>>> GetPersonPagesAsync(int channelId, string? culture, bool? onlyPublished, CancellationToken cancellationToken)
    {
        try
        {
            var currentCulture = await ValidateAndNormalizeSiteCultureAsync(channelId, culture ?? defaultCulture.Language, cancellationToken) ?? defaultCulture;

            if (string.IsNullOrEmpty(currentCulture.Locale))
            {
                throw new NotFoundException("Invalid culture specified");
            }

            var channel = await _channelInfoProvider.GetAsync(channelId);
            if (string.IsNullOrEmpty(channel?.ChannelName))
            {
                throw new NotFoundException("Channel not found");
            }

            var retrieveParameters = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 5,
                ChannelName = channel.ChannelName,
                LanguageName = currentCulture.Locale,
                IsForPreview = !(onlyPublished ?? true),
            };

            var results = await _contentRetriever.GetPagesAsync<Person>(
                retrieveParameters,
                query => { },
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var people = results?.ToList() ?? new List<Person>();

            return people.Select(person => MapPersonToPageOutput(person, currentCulture.Locale)).ToList();
        }
        catch (NotFoundException ex)
        {
            return new NotFoundObjectResult(ex.Message);
        }
        catch (UnprocessableEntityException)
        {
            return new UnprocessableEntityObjectResult("One or more validation errors occurred.");
        }
        catch (Exception ex)
        {
            return new ObjectResult(ex.Message) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

    public async Task<ActionResult<PageOutput<PersonContent>>> GetPersonPageAsync(int channelId, Guid pageGUID, string? culture, CancellationToken cancellationToken)
    {
        try
        {
            var currentCulture = await ValidateAndNormalizeSiteCultureAsync(channelId, culture ?? defaultCulture.Language, cancellationToken) ?? defaultCulture;

            if (string.IsNullOrEmpty(currentCulture.Locale))
            {
                throw new NotFoundException("Invalid culture specified");
            }

            var channel = await _channelInfoProvider.GetAsync(channelId);
            if (string.IsNullOrEmpty(channel?.ChannelName))
            {
                throw new NotFoundException("Channel not found");
            }

            var _retrieveCurrentPage = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 1,
                ChannelName = channel.ChannelName,
                LanguageName = currentCulture.Locale,
                IsForPreview = false
            };

            var result = await _contentRetriever.GetPagesAsync<Person>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var person = result?.FirstOrDefault();

            if (person is null)
            {
                return new NotFoundResult();
            }

            return MapPersonToPageOutput(person, currentCulture.Locale);
        }
        catch (NotFoundException ex)
        {
            return new NotFoundObjectResult(ex.Message);
        }
        catch (UnprocessableEntityException)
        {
            return new UnprocessableEntityObjectResult("One or more validation errors occurred.");
        }
        catch (Exception ex)
        {
            return new ObjectResult(ex.Message) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

    public async Task<ActionResult<PageOutput<PersonContent>>> CreatePageAsync(int channelId, int nodeParentID, string? culture, PersonContent request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return new BadRequestResult();
        }

        try
        {
            var currentCulture = await ValidateAndNormalizeSiteCultureAsync(channelId, culture ?? defaultCulture.Language, cancellationToken) ?? defaultCulture;

            if (string.IsNullOrEmpty(currentCulture.Locale))
            {
                throw new NotFoundException("Invalid culture specified");
            }

            var channel = await _channelInfoProvider.GetAsync(channelId);
            if (string.IsNullOrEmpty(channel?.ChannelName))
            {
                throw new NotFoundException("Channel not found");
            }

            var _retrieveCurrentPage = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 1,
                ChannelName = channel.ChannelName,
                LanguageName = currentCulture.Locale,
                IsForPreview = false
            };

            var parentPage = await _contentRetriever.GetPagesAsync<SectionPeoplePage>(_retrieveCurrentPage, query => query
                .Where(w => w.WhereEquals("WebPageItemID", nodeParentID)), RetrievalCacheSettings.CacheDisabled, cancellationToken);

            if (parentPage == null)
            {
                _validationState.AddUnprocessableEntityError(nameof(nodeParentID), ErrorMessage.ParentWithNodeIdNotFound(nodeParentID));
                throw new UnprocessableEntityException();
            }

            _pageContentValidator.ValidateAndNormalizePageContent(request, "Person");

            _validationState.ThrowIfThereAreErrors();

            // The page display name shown in the admin UI
            string pageDisplayName = request.MetadataTitle ?? "New Person Created by API";

            var itemData = await CreatePersonItemData(request, nodeParentID, currentCulture.Locale, cancellationToken);

            // Creates a page metadata object
            var contentItemParameters = new ContentItemParameters(PERSON_CONTENT_TYPE, itemData);
            var createPageParameters = new CreateWebPageParameters(pageDisplayName, currentCulture.Locale, contentItemParameters)
            {
                ParentWebPageItemID = nodeParentID
            };

            // Creates the page in the database
            int newPageId = await _webPageManager.Create(createPageParameters, cancellationToken);

            // Update the draft with new content
            if (!await _webPageManager.TryUpdateDraft(newPageId, currentCulture.Locale, new UpdateDraftData(itemData)))
            {
                throw new NotFoundException("Failed to update page draft");
            }

            // Publish the updated page
            if (!await _webPageManager.TryPublish(newPageId, currentCulture.Locale))
            {
                throw new NotFoundException("Failed to publish updated page");
            }

            // Retrieve the created page and return it
            var _retrieveNewPage = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 5,
                ChannelName = channel.ChannelName,
                LanguageName = currentCulture.Locale,
                IsForPreview = false
            };

            var createdResult = await _contentRetriever.GetPagesAsync<Person>(_retrieveNewPage,
                query => query.Where(w => w.WhereEquals("WebPageItemID", newPageId)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var createdPerson = createdResult?.FirstOrDefault();
            return createdPerson != null ? MapPersonToPageOutput(createdPerson, currentCulture.Locale) : new BadRequestResult();
        }
        catch (UnprocessableEntityException)
        {
            return new UnprocessableEntityObjectResult("One or more validation errors occurred.");
        }
        catch (NotFoundException ex)
        {
            return new NotFoundObjectResult(ex.Message);
        }
        catch (Exception ex)
        {
            return new ObjectResult(ex.Message) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

    public async Task<ActionResult<PageOutput<PersonContent>>> CreatePageCultureVersionAsync(int channelId, Guid pageGUID, string? fromCulture, string? toCulture, PersonContent request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return new BadRequestResult();
        }

        try
        {
            var fromLangCulture = await ValidateAndNormalizeSiteCultureAsync(channelId, fromCulture ?? defaultCulture.Language, cancellationToken) ?? defaultCulture;

            if (string.IsNullOrEmpty(fromLangCulture.Language))
            {
                throw new NotFoundException("Invalid from culture specified");
            }

            var toLangCulture = await ValidateAndNormalizeSiteCultureAsync(channelId, toCulture ?? defaultCulture.Language, cancellationToken) ?? defaultCulture;

            if (string.IsNullOrEmpty(toLangCulture.Language))
            {
                throw new NotFoundException("Invalid to culture specified");
            }

            var channel = await _channelInfoProvider.GetAsync(channelId);
            if (string.IsNullOrEmpty(channel?.ChannelName))
            {
                throw new NotFoundException("Channel not found");
            }

            _pageContentValidator.ValidateAndNormalizePageContent(request, "Person");

            _validationState.ThrowIfThereAreErrors();

            var _retrieveCurrentPage = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 1,
                ChannelName = channel.ChannelName,
                LanguageName = fromLangCulture.Locale,
                IsForPreview = false
            };

            // Get existing page
            var result = await _contentRetriever.GetPagesAsync<Person>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var existingPerson = result?.FirstOrDefault();
            if (existingPerson is null)
            {
                return new NotFoundResult();
            }
            // Create new culture version data
            var itemData = await CreatePersonItemData(request, existingPerson.SystemFields.WebPageItemParentID, toLangCulture.Locale, cancellationToken);

            // Create language variant for the existing page
            var createVariantParameters = new CMS.Websites.CreateLanguageVariantParameters(
                existingPerson.SystemFields.WebPageItemID,
                toLangCulture.Locale,
                existingPerson.SystemFields?.ContentItemName ?? "Person culture variant",
                itemData);

            // Creates the language variant in the database
            if (!await _webPageManager.TryCreateLanguageVariant(createVariantParameters))
            {
                throw new NotFoundException("Failed to create language variant");
            }

            // Update the draft with new content
            if (!await _webPageManager.TryUpdateDraft(existingPerson.SystemFields.WebPageItemID, toLangCulture.Locale, new UpdateDraftData(itemData)))
            {
                throw new NotFoundException("Failed to update page draft");
            }

            // Publish the updated page
            if (!await _webPageManager.TryPublish(existingPerson.SystemFields.WebPageItemID, toLangCulture.Locale))
            {
                throw new NotFoundException("Failed to publish updated page");
            }

            // Retrieve the created variant and return it
            var _retrieveNewVariant = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 5,
                ChannelName = channel.ChannelName,
                LanguageName = toLangCulture.Locale,
                IsForPreview = false
            };

            var variantResult = await _contentRetriever.GetPagesAsync<Person>(_retrieveNewVariant,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var variantPerson = variantResult?.FirstOrDefault();
            return variantPerson != null ? MapPersonToPageOutput(variantPerson, toLangCulture.Locale) : new BadRequestResult();
        }
        catch (UnprocessableEntityException)
        {
            return new UnprocessableEntityObjectResult("One or more validation errors occurred.");
        }
        catch (NotFoundException ex)
        {
            return new NotFoundObjectResult(ex.Message);
        }
        catch (Exception ex)
        {
            return new ObjectResult(ex.Message) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

    public async Task<ActionResult<PageOutput<PersonContent>>> UpdatePageAsync(int channelId, Guid pageGUID, string? culture, PersonContent request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return new BadRequestResult();
        }

        try
        {
            var currentCulture = await ValidateAndNormalizeSiteCultureAsync(channelId, culture ?? defaultCulture.Locale, cancellationToken) ?? defaultCulture;

            if (string.IsNullOrEmpty(currentCulture.Locale))
            {
                throw new NotFoundException("Invalid culture specified");
            }

            var channel = await _channelInfoProvider.GetAsync(channelId);
            if (string.IsNullOrEmpty(channel?.ChannelName))
            {
                throw new NotFoundException("Channel not found");
            }

            _pageContentValidator.ValidateAndNormalizePageContent(request, "Person");

            _validationState.ThrowIfThereAreErrors();

            var _retrieveCurrentPage = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 1,
                ChannelName = channel.ChannelName,
                LanguageName = currentCulture.Locale,
                IsForPreview = false
            };

            // Get existing page
            var result = await _contentRetriever.GetPagesAsync<Person>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var existingPerson = result?.FirstOrDefault();
            if (existingPerson is null)
            {
                return new NotFoundResult();
            }
            // Build ContentItemData for the update
            var itemData = await CreatePersonItemData(request, existingPerson.SystemFields.WebPageItemParentID, currentCulture.Locale, cancellationToken);

            // Create a draft, update it, and publish
            await _webPageManager.TryCreateDraft(existingPerson.SystemFields.WebPageItemID, currentCulture.Locale);

            // Update the draft with new content
            if (!await _webPageManager.TryUpdateDraft(existingPerson.SystemFields.WebPageItemID, currentCulture.Locale, new UpdateDraftData(itemData)))
            {
                throw new NotFoundException("Failed to update page draft");
            }

            // Publish the updated page
            if (!await _webPageManager.TryPublish(existingPerson.SystemFields.WebPageItemID, currentCulture.Locale))
            {
                throw new NotFoundException("Failed to publish updated page");
            }

            // Retrieve the updated page and return it
            var updatedResult = await _contentRetriever.GetPagesAsync<Person>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var updatedPerson = updatedResult?.FirstOrDefault();
            return updatedPerson != null ? MapPersonToPageOutput(updatedPerson, currentCulture.Locale) : new BadRequestResult();
        }
        catch (UnprocessableEntityException)
        {
            return new UnprocessableEntityObjectResult("One or more validation errors occurred.");
        }
        catch (NotFoundException ex)
        {
            return new NotFoundObjectResult(ex.Message);
        }
        catch (Exception ex)
        {
            return new ObjectResult(ex.Message) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

    public async Task DeletePageAsync(int channelId, Guid pageGUID, string? culture, bool deleteAllCultures, CancellationToken cancellationToken)
    {
        var currentCulture = await ValidateAndNormalizeSiteCultureAsync(channelId, culture ?? defaultCulture.Locale, cancellationToken) ?? defaultCulture;

        if (string.IsNullOrEmpty(currentCulture.Locale))
        {
            throw new NotFoundException("Invalid culture specified");
        }

        var channel = await _channelInfoProvider.GetAsync(channelId);
        if (string.IsNullOrEmpty(channel?.ChannelName))
        {
            throw new NotFoundException("Channel not found");
        }

        if (deleteAllCultures)
        {
            // Get all available cultures for the channel
            var allCultures = await _cultureService.GetSiteCulturesLanguageCodeAsync(channelId, cancellationToken);

            // Delete the page variant for each culture
            foreach (var cult in allCultures)
            {
                var _retrieveCurrentPage = new RetrievePagesParameters
                {
                    LinkedItemsMaxLevel = 1,
                    ChannelName = channel.ChannelName,
                    LanguageName = currentCulture.Locale,
                    IsForPreview = false
                };

                // Get existing page for this culture
                var result = await _contentRetriever.GetPagesAsync<Person>(_retrieveCurrentPage,
                    query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID)),
                    RetrievalCacheSettings.CacheDisabled,
                    cancellationToken);

                var existingPerson = result?.FirstOrDefault();
                if (existingPerson is not null)
                {
                    await _webPageManager.Delete(
                        new DeleteWebPageParameters(existingPerson.SystemFields.WebPageItemID, cult.ToString())
                        {
                            Permanently = true
                        });
                }
            }
        }
        else
        {
            // Delete only the requested culture variant
            var _retrieveCurrentPage = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 1,
                ChannelName = channel.ChannelName,
                LanguageName = currentCulture.Locale,
                IsForPreview = false
            };

            // Get existing page
            var result = await _contentRetriever.GetPagesAsync<Person>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var existingPerson = result?.FirstOrDefault();
            if (existingPerson is null)
            {
                throw new NotFoundException();
            }

            await _webPageManager.Delete(
                new DeleteWebPageParameters(existingPerson.SystemFields.WebPageItemID, currentCulture.Locale)
                {
                    Permanently = true
                });
        }
    }

    private async Task<ContentItemData> CreatePersonItemData(PersonContent content, int? webPageParentID, string? culture, CancellationToken cancellationToken)
    {
        //var PersonCredentials = await _mappingHelper.BuildReferencesAsync<Credentials>(Person.PersonCredentials, i => i.Name, cancellationToken);
        //var MetadataCity = await _mappingHelper.BuildReferencesAsync<LocationCity>(Person.MetadataCity, s => s.Name, cancellationToken);
        //var MetadataOfficeLocation = await _mappingHelper.BuildReferencesAsync<LocationOffice>(Person.MetadataOfficeLocation, d => d.Name, cancellationToken);
        //var MetadataProfileGroups = await _mappingHelper.BuildReferencesAsync<ProfileGroups>(Person.MetadataProfileGroups, r => r.Name, cancellationToken);
        //var MetadataBusinessLines = await _mappingHelper.BuildReferencesAsync<BusinessLine>(Person.MetadataBusinessLines, i => i.Name, cancellationToken);
        //var MetadataServiceAreasPerson = await _mappingHelper.BuildReferencesAsync<LocationCity>(Person., s => s.Name, cancellationToken);
        //var MetadataServicesPerson = await _mappingHelper.BuildReferencesAsync<DealCardDealType>(Person.MetadataOfficeLocation, d => d.Name, cancellationToken);
        //var MetadataSubServices = await _mappingHelper.BuildReferencesAsync<DealCardRange>(Person.MetadataProfileGroups, r => r.Name, cancellationToken);
        //var MetadataIndustryCategoriesPerson = await _mappingHelper.BuildReferencesAsync<DealCardIndustry>(Person.PersonCredentials, i => i.Name, cancellationToken);
        //var MetadataIndustriesPerson = await _mappingHelper.BuildReferencesAsync<DealCardSubIndustry>(Person.MetadataCity, s => s.Name, cancellationToken);
        //var MetadataIndustryServices = await _mappingHelper.BuildReferencesAsync<DealCardDealType>(Person.MetadataOfficeLocation, d => d.Name, cancellationToken);
        //var MetadataSpecialtiesCategories = await _mappingHelper.BuildReferencesAsync<DealCardSubIndustry>(Person.MetadataCity, s => s.Name, cancellationToken);
        //var MetadataSpecialtiesAreas = await _mappingHelper.BuildReferencesAsync<DealCardDealType>(Person.MetadataOfficeLocation, d => d.Name, cancellationToken);
        //var MicrositeMasterPagesPerson = await _mappingHelper.BuildReferencesAsync<DealCardRange>(Person.MetadataProfileGroups, r => r.Name, cancellationToken);
        //var MicrositePagesPerson = await _mappingHelper.BuildReferencesAsync<DealCardRange>(Person.MetadataProfileGroups, r => r.Name, cancellationToken);


        var teaserImageReferences = await _mappingHelper.BuildContentAttachmentImageReferences(content.MetadataTeaserImage, cancellationToken);
        var ogImageReferences = await _mappingHelper.BuildContentAttachmentImageReferences(content.MetadataOGImage, cancellationToken);
        var ogTwitterImageReferences = await _mappingHelper.BuildContentAttachmentImageReferences(content.MetadataOGTwitterImage, cancellationToken);

        return new ContentItemData(new Dictionary<string, object>
        {
            { nameof(Person.MetadataTitle), content.MetadataTitle ?? string.Empty },
            { nameof(Person.PersonFirstName), content.PersonFirstName ?? string.Empty },
            { nameof(Person.PersonSurnamePrefixes), content.PersonSurnamePrefixes ?? string.Empty },
            { nameof(Person.PersonLastName), content.PersonLastName ?? string.Empty },
            { nameof(Person.MetadataTeaserImage), teaserImageReferences },
            { nameof(Person.MetadataTeaserImageAltText), content.MetadataTeaserImageAltText ?? string.Empty },
            { nameof(Person.MetadataCanonical), content.MetadataCanonical ?? string.Empty },
            { nameof(Person.MetadataDescription), content.MetadataDescription ?? string.Empty },
            { nameof(Person.PersonJobTitle), content.PersonJobTitle ?? string.Empty },
            { nameof(Person.PersonPhone), content.PersonPhone ?? string.Empty },
            { nameof(Person.PersonMobilePhone), content.PersonMobilePhone ?? string.Empty },
            { nameof(Person.PersonDirectDialNumber), content.PersonDirectDialNumber ?? string.Empty },
            { nameof(Person.PersonEmail), content.PersonEmail ?? string.Empty },
            { nameof(Person.EnableForm), content.EnableForm },
            { nameof(Person.EnableVCard), content.EnableVCard },

            { nameof(Person.PersonCredentials), ToReferences(content.PersonCredentials) },
            { nameof(Person.MetadataCity), ToReferences(content.MetadataCity) },
            { nameof(Person.MetadataOfficeLocation), ToReferences(content.MetadataOfficeLocation) },
            { nameof(Person.MetadataProfileGroups), ToReferences(content.MetadataProfileGroups) },
            { nameof(Person.MetadataBusinessLines), ToReferences(content.MetadataBusinessLines) },
            { nameof(Person.MetadataServiceAreasPerson), ToReferences(content.MetadataServiceAreas) },
            { nameof(Person.MetadataServicesPerson), ToReferences(content.MetadataServices) },
            { nameof(Person.MetadataSubServices), ToReferences(content.MetadataSubServices) },
            { nameof(Person.MetadataIndustryCategoriesPerson), ToReferences(content.MetadataIndustryCategories) },
            { nameof(Person.MetadataIndustriesPerson), ToReferences(content.MetadataIndustries) },
            { nameof(Person.MetadataIndustryServices), ToReferences(content.MetadataIndustryServices) },
            { nameof(Person.MetadataSpecialtiesCategories), ToReferences(content.MetadataSpecialtiesCategories) },
            { nameof(Person.MetadataSpecialtiesAreas), ToReferences(content.MetadataSpecialtiesAreas) },
            { nameof(Person.MetadataSpecialtiesPages), ToReferences(content.MetadataSpecialtiesPages) },
            { nameof(Person.MicrositeMasterPagesPerson), ToReferences(content.MicrositeMasterPages) },
            { nameof(Person.MicrositePagesPerson), ToReferences(content.MicrositePages) },

            { nameof(Person.Facebook), content.Facebook ?? string.Empty },
            { nameof(Person.Twitter), content.Twitter ?? string.Empty },
            { nameof(Person.LinkedIn), content.LinkedIn ?? string.Empty },

            { nameof(Person.MetadataOGTitle), content.MetadataOGTitle ?? string.Empty },
            { nameof(Person.MetadataOGDescription), content.MetadataOGDescription ?? string.Empty },
            { nameof(Person.MetadataOGImage), ogImageReferences },
            { nameof(Person.MetadataOGTwitterImageSizeCheckbox), content.MetadataOGTwitterImageSizeCheckbox },
            { nameof(Person.MetadataOGTwitterImage), ogTwitterImageReferences },
            { nameof(Person.MetadataNofollow), content.MetadataNofollow },
            { nameof(Person.MetadataNoindex), content.MetadataNoindex },
            { nameof(Person.IncludeInSitemap), content.IncludeInSitemap },

            { nameof(Person.PageTitle), content.MetadataTitle ?? string.Empty },
            { nameof(Person.PageKeywords), string.Empty },
        });
    }

    private static IEnumerable<ContentItemReference> ToReferences(List<Guid>? guids)
    {
        return guids?.Select(guid => new ContentItemReference { Identifier = guid }) ?? [];
    }

    private static PageOutput<PersonContent> MapPersonToPageOutput(Person person, string culture)
    {
        return new PageOutput<PersonContent>
        {
            General = new PageGeneralOutput
            {
                Name = person.SystemFields?.WebPageItemName ?? string.Empty,
                Culture = culture ?? string.Empty,
                WebPageItemParentID = person.SystemFields?.WebPageItemParentID ?? 0,
                WebPageItemGuid = person.SystemFields?.WebPageItemGUID ?? Guid.Empty,
                WebPageItemID = person.SystemFields?.WebPageItemID ?? 0,
                ContentItemGuid = person.SystemFields?.ContentItemGUID ?? Guid.Empty,
                ContentItemID = person.SystemFields?.ContentItemID ?? 0,
                Path = person.SystemFields?.WebPageItemTreePath ?? string.Empty
            },
            Content = new PersonContent
            {
                MetadataTitle = person.MetadataTitle,
                PersonFirstName = person.PersonFirstName,
                PersonSurnamePrefixes = person.PersonSurnamePrefixes,
                PersonLastName = person.PersonLastName,
                MetadataTeaserImage = person.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataTeaserImageAltText = person.MetadataTeaserImageAltText,
                MetadataCanonical = person.MetadataCanonical,
                MetadataDescription = person.MetadataDescription,
                PersonJobTitle = person.PersonJobTitle,
                PersonPhone = person.PersonPhone,
                PersonMobilePhone = person.PersonMobilePhone,
                PersonDirectDialNumber = person.PersonDirectDialNumber,
                PersonEmail = person.PersonEmail,
                EnableForm = person.EnableForm,
                EnableVCard = person.EnableVCard,
                PersonCredentials = person.PersonCredentials?.Select(c => c.SystemFields?.WebPageItemGUID ?? Guid.Empty).ToList() ?? [],
                MetadataCity = person.MetadataCity?.Select(c => c.SystemFields?.WebPageItemGUID ?? Guid.Empty).ToList() ?? [],
                MetadataOfficeLocation = person.MetadataOfficeLocation?.Select(c => c.SystemFields?.WebPageItemGUID ?? Guid.Empty).ToList() ?? [],
                MetadataProfileGroups = person.MetadataProfileGroups?.Select(c => c.SystemFields?.WebPageItemGUID ?? Guid.Empty).ToList() ?? [],
                MetadataBusinessLines = person.MetadataBusinessLines?.Select(c => c.SystemFields?.WebPageItemGUID ?? Guid.Empty).ToList() ?? [],
                MetadataServiceAreas = person.MetadataServiceAreasPerson?.Select(c => c.SystemFields?.WebPageItemGUID ?? Guid.Empty).ToList() ?? [],
                MetadataServices = person.MetadataServicesPerson?.Select(c => c.SystemFields?.WebPageItemGUID ?? Guid.Empty).ToList() ?? [],
                MetadataSubServices = person.MetadataSubServices?.Select(c => c.SystemFields?.WebPageItemGUID ?? Guid.Empty).ToList() ?? [],
                MetadataIndustryCategories = person.MetadataIndustryCategoriesPerson?.Select(c => c.SystemFields?.WebPageItemGUID ?? Guid.Empty).ToList() ?? [],
                MetadataIndustries = person.MetadataIndustriesPerson?.Select(c => c.SystemFields?.WebPageItemGUID ?? Guid.Empty).ToList() ?? [],
                MetadataIndustryServices = person.MetadataIndustryServices?.Select(c => c.SystemFields?.WebPageItemGUID ?? Guid.Empty).ToList() ?? [],
                MetadataSpecialtiesCategories = person.MetadataSpecialtiesCategories?.Select(c => c.SystemFields?.WebPageItemGUID ?? Guid.Empty).ToList() ?? [],
                MetadataSpecialtiesAreas = person.MetadataSpecialtiesAreas?.Select(c => c.SystemFields?.WebPageItemGUID ?? Guid.Empty).ToList() ?? [],
                MetadataSpecialtiesPages = person.MetadataSpecialtiesPages?.Select(c => c.SystemFields?.WebPageItemGUID ?? Guid.Empty).ToList() ?? [],
                MicrositeMasterPages = person.MicrositeMasterPagesPerson?.Select(c => c.SystemFields?.WebPageItemGUID ?? Guid.Empty).ToList() ?? [],
                MicrositePages = person.MicrositePagesPerson?.Select(c => c.SystemFields?.WebPageItemGUID ?? Guid.Empty).ToList() ?? [],
                Facebook = person.Facebook,
                Twitter = person.Twitter,
                LinkedIn = person.LinkedIn,
                MetadataOGTitle = person.MetadataOGTitle,
                MetadataOGDescription = person.MetadataOGDescription,
                MetadataOGImage = person.MetadataOGImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataOGTwitterImageSizeCheckbox = person.MetadataOGTwitterImageSizeCheckbox,
                MetadataOGTwitterImage = person.MetadataOGTwitterImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataNofollow = person.MetadataNofollow,
                MetadataNoindex = person.MetadataNoindex,
                IncludeInSitemap = person.IncludeInSitemap,
                PublishFrom = person.SystemFields?.ContentItemCommonDataFirstPublishedWhen,
                PublishTo = person.SystemFields?.ContentItemCommonDataLastPublishedWhen,
            },
            Metadata = new PageMetadataOutput
            {
                PageTitle = new InheritableString { Value = person.MetadataTitle ?? string.Empty },
                PageDescription = new InheritableString { Value = person.MetadataDescription ?? string.Empty },
                PageKeywords = new InheritableString { Value = string.Empty },
            },
            Navigation = new PageNavigationOutput
            {
                ShowInMenu = false,
                ShowInSitemap = person.IncludeInSitemap,
            },
        };
    }

    private async Task<LanguageTag?> ValidateAndNormalizeSiteCultureAsync(int channelId, string languageTagString, CancellationToken cancellationToken)
    {
        var languageTag = ValidateAndNormalizeCultureAsync(languageTagString);

        if (languageTag == null)
        {
            return null;
        }

        var siteCultures = (await _cultureService.GetSiteCulturesLanguageCodeAsync(channelId, cancellationToken)).ToHashSet();

        if (!siteCultures.Contains(languageTag))
        {
            _validationState.AddError("culture", ErrorMessage.DocumentCultureNotAllowedOnSite(languageTag, _websiteChannelContext.WebsiteChannelName, siteCultures));
        }

        return languageTag;
    }

    private LanguageTag? ValidateAndNormalizeCultureAsync(string languageTagString)
    {
        if (LanguageTag.TryParse(languageTagString, out var languageTag))
        {
            return languageTag;
        }

        _validationState.AddError("culture", ErrorMessage.IncorrectLanguageTagFormat(languageTagString));

        return null;
    }
}