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

public interface ISectionEventsCrudService
{
    Task<ActionResult<IEnumerable<PageOutput<SectionEventsPageContent>>>> GetPagesAsync(int channelId, string? culture, bool? onlyPublished, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<SectionEventsPageContent>>> GetPageAsync(int channelId, Guid WebPageItemGuid, string? culture, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<SectionEventsPageContent>>> CreatePageAsync(int channelId, int WebPageParentID, string? culture, SectionEventsPageContent request, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<SectionEventsPageContent>>> CreatePageCultureVersionAsync(int channelId, Guid WebPageItemGuid, string? fromCulture, string? toCulture, SectionEventsPageContent request, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<SectionEventsPageContent>>> UpdatePageAsync(int channelId, Guid WebPageItemGuid, string? culture, SectionEventsPageContent request, CancellationToken cancellationToken);
    Task DeletePageAsync(int channelId, Guid WebPageItemGuid, string? culture, bool deleteAllCultures, CancellationToken cancellationToken);
}
public class SectionEventsCrudService : ISectionEventsCrudService
{
    private readonly IContentService _contentRetriever;
    private readonly IValidationState _validationState;
    private readonly ICultureService _cultureService;
    private readonly IPageContentValidator _pageContentValidator;
    private readonly IInfoProvider<ChannelInfo> _channelInfoProvider;
    private readonly IWebPageManager _webPageManager;
    private readonly IMappingHelper _mappingHelper;
    private static readonly LanguageTag defaultCulture = LanguageTag.GB;
    private const string SECTION_EVENTS_CONTENT_TYPE = SectionEvents.CONTENT_TYPE_NAME;
    private const string WEB_PAGE_GUID = "WebPageItemGUID";

#pragma warning disable S107
    public SectionEventsCrudService(
        IContentService contentRetriever,
        IValidationState validationState,
        IWebsiteChannelContext websiteChannelContext,
        ICultureService cultureService,
        IPageContentValidator pageContentValidator,
        IInfoProvider<ChannelInfo> channelInfoProvider,
        IWebPageManagerFactory webPageManagerFactory,
        IUserInfoProvider userInfoProvider,
        IMappingHelper mappingHelper)
#pragma warning restore S107
    {
        UserInfo user = userInfoProvider.Get("administrator");

        _contentRetriever = contentRetriever;
        _validationState = validationState;
        _cultureService = cultureService;
        _pageContentValidator = pageContentValidator;
        _channelInfoProvider = channelInfoProvider;
        _webPageManager = webPageManagerFactory.Create(websiteChannelContext.WebsiteChannelID == 0 ? 1 : websiteChannelContext.WebsiteChannelID, user.UserID);
        _mappingHelper = mappingHelper;
    }

    public async Task<ActionResult<IEnumerable<PageOutput<SectionEventsPageContent>>>> GetPagesAsync(int channelId, string? culture, bool? onlyPublished, CancellationToken cancellationToken)
    {
        try
        {
            var currentCulture = await _mappingHelper.ValidateAndNormalizeSiteCultureAsync(channelId, culture ?? defaultCulture.Language, cancellationToken) ?? defaultCulture;

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

            var results = await _contentRetriever.GetPagesAsync<SectionEvents>(
                retrieveParameters,
                query => { },
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var sectionEvents = results?.ToList() ?? new List<SectionEvents>();

            var pageOutputs = new List<PageOutput<SectionEventsPageContent>>();
            foreach (var page in sectionEvents)
            {
                pageOutputs.Add(await MapSectionEventsToPageOutput(page, channel.ChannelName, currentCulture.Locale, cancellationToken));
            }

            return pageOutputs;
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

    public async Task<ActionResult<PageOutput<SectionEventsPageContent>>> GetPageAsync(int channelId, Guid WebPageItemGuid, string? culture, CancellationToken cancellationToken)
    {
        try
        {
            var currentCulture = await _mappingHelper.ValidateAndNormalizeSiteCultureAsync(channelId, culture ?? defaultCulture.Language, cancellationToken) ?? defaultCulture;

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

            var result = await _contentRetriever.GetPagesAsync<SectionEvents>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, WebPageItemGuid)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var sectionEventsPage = result?.FirstOrDefault();

            if (sectionEventsPage is null)
            {
                return new NotFoundResult();
            }

            return await MapSectionEventsToPageOutput(sectionEventsPage, channel.ChannelName, currentCulture.Locale, cancellationToken);
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

    public async Task<ActionResult<PageOutput<SectionEventsPageContent>>> CreatePageAsync(int channelId, int WebPageParentID, string? culture, SectionEventsPageContent request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return new BadRequestResult();
        }

        try
        {
            var currentCulture = await _mappingHelper.ValidateAndNormalizeSiteCultureAsync(channelId, culture ?? defaultCulture.Language, cancellationToken) ?? defaultCulture;

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

            if (WebPageParentID != 0)
            {
                var parentPage = await _contentRetriever.GetPagesAsync<SectionEvents>(_retrieveCurrentPage, query => query
                .Where(w => w.WhereEquals("WebPageItemID", WebPageParentID)), RetrievalCacheSettings.CacheDisabled, cancellationToken);

                if (parentPage == null)
                {
                    _validationState.AddUnprocessableEntityError(nameof(WebPageParentID), ErrorMessage.ParentWithNodeIdNotFound(WebPageParentID));
                    throw new UnprocessableEntityException();
                }
            }

            if (request.EventsWebinarsListFilters is { Count: > 0 })
            {
                request.EventsWebinarsListFilters = request.EventsWebinarsListFilters
                    .SelectMany(f => (f ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            _pageContentValidator.ValidateAndNormalizePageContent(request);

            _validationState.ThrowIfThereAreErrors();

            string pageDisplayName = request.MetadataTitle ?? "New Section Events Page Created by API";

            var itemData = await CreateSectionEventsItemData(request, channel.ChannelName, currentCulture.Locale, cancellationToken);

            var contentItemParameters = new ContentItemParameters(SECTION_EVENTS_CONTENT_TYPE, itemData);
            var createPageParameters = new CreateWebPageParameters(pageDisplayName, currentCulture.Locale, contentItemParameters)
            {
                ParentWebPageItemID = WebPageParentID
            };

            int newPageId = await _webPageManager.Create(createPageParameters, cancellationToken);

            if (!await _webPageManager.TryUpdateDraft(newPageId, currentCulture.Locale, new UpdateDraftData(itemData), cancellationToken))
            {
                throw new NotFoundException("Failed to update page draft");
            }

            if (!await _webPageManager.TryPublish(newPageId, currentCulture.Locale, cancellationToken: cancellationToken))
            {
                throw new NotFoundException("Failed to publish updated page");
            }

            var _retrieveNewPage = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 5,
                ChannelName = channel.ChannelName,
                LanguageName = currentCulture.Locale,
                IsForPreview = false
            };

            var createdResult = await _contentRetriever.GetPagesAsync<SectionEvents>(_retrieveNewPage,
                query => query.Where(w => w.WhereEquals("WebPageItemID", newPageId)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var createdPage = createdResult?.FirstOrDefault();
            return createdPage != null
                ? await MapSectionEventsToPageOutput(createdPage, channel.ChannelName, currentCulture.Locale, cancellationToken)
                : new BadRequestResult();
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

    public async Task<ActionResult<PageOutput<SectionEventsPageContent>>> CreatePageCultureVersionAsync(int channelId, Guid WebPageItemGuid, string? fromCulture, string? toCulture, SectionEventsPageContent request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return new BadRequestResult();
        }

        try

        {
            var fromLangCulture = await _mappingHelper.ValidateAndNormalizeSiteCultureAsync(channelId, fromCulture ?? defaultCulture.Language, cancellationToken) ?? defaultCulture;

            if (string.IsNullOrEmpty(fromLangCulture.Language))
            {
                throw new NotFoundException("Invalid from culture specified");
            }

            var toLangCulture = await _mappingHelper.ValidateAndNormalizeSiteCultureAsync(channelId, toCulture ?? defaultCulture.Language, cancellationToken) ?? defaultCulture;

            if (string.IsNullOrEmpty(toLangCulture.Language))
            {
                throw new NotFoundException("Invalid to culture specified");
            }

            var channel = await _channelInfoProvider.GetAsync(channelId);
            if (string.IsNullOrEmpty(channel?.ChannelName))
            {
                throw new NotFoundException("Channel not found");
            }


            if (request.EventsWebinarsListFilters is { Count: > 0 })
            {
                request.EventsWebinarsListFilters = request.EventsWebinarsListFilters
                    .SelectMany(f => (f ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            _pageContentValidator.ValidateAndNormalizePageContent(request);

            _validationState.ThrowIfThereAreErrors();

            var _retrieveCurrentPage = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 1,
                ChannelName = channel.ChannelName,
                LanguageName = fromLangCulture.Locale,
                IsForPreview = false
            };

            var result = await _contentRetriever.GetPagesAsync<SectionEvents>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, WebPageItemGuid)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var existingPage = result?.FirstOrDefault();
            if (existingPage is null)
            {
                return new NotFoundResult();
            }

            if (existingPage.SystemFields is null)
            {
                throw new NotFoundException("Page system fields are missing");
            }

            var itemData = await CreateSectionEventsItemData(request, channel.ChannelName, toLangCulture.Locale, cancellationToken);

            var createVariantParameters = new CMS.Websites.CreateLanguageVariantParameters(
                existingPage.SystemFields.WebPageItemID,
                toLangCulture.Locale,
                existingPage.SystemFields.ContentItemName ?? "Section Events culture variant",
                itemData);

            if (!await _webPageManager.TryCreateLanguageVariant(createVariantParameters, cancellationToken))
            {
                throw new NotFoundException("Failed to create language variant");
            }

            if (!await _webPageManager.TryUpdateDraft(existingPage.SystemFields.WebPageItemID, toLangCulture.Locale, new UpdateDraftData(itemData), cancellationToken))
            {
                throw new NotFoundException("Failed to update page draft");
            }

            if (!await _webPageManager.TryPublish(existingPage.SystemFields.WebPageItemID, toLangCulture.Locale, cancellationToken: cancellationToken))
            {
                throw new NotFoundException("Failed to publish updated page");
            }

            var _retrieveNewVariant = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 5,
                ChannelName = channel.ChannelName,
                LanguageName = toLangCulture.Locale,
                IsForPreview = false
            };

            var variantResult = await _contentRetriever.GetPagesAsync<SectionEvents>(_retrieveNewVariant,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, WebPageItemGuid)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var variantPage = variantResult?.FirstOrDefault();
            return variantPage != null
                ? await MapSectionEventsToPageOutput(variantPage, channel.ChannelName, toLangCulture.Locale, cancellationToken)
                : new BadRequestResult();
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

    public async Task<ActionResult<PageOutput<SectionEventsPageContent>>> UpdatePageAsync(int channelId, Guid WebPageItemGuid, string? culture, SectionEventsPageContent request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return new BadRequestResult();
        }

        try
        {
            var currentCulture = await _mappingHelper.ValidateAndNormalizeSiteCultureAsync(channelId, culture ?? defaultCulture.Locale, cancellationToken) ?? defaultCulture;

            if (string.IsNullOrEmpty(currentCulture.Locale))
            {
                throw new NotFoundException("Invalid culture specified");
            }

            var channel = await _channelInfoProvider.GetAsync(channelId);
            if (string.IsNullOrEmpty(channel?.ChannelName))
            {
                throw new NotFoundException("Channel not found");
            }

            _pageContentValidator.ValidateAndNormalizePageContent(request);

            _validationState.ThrowIfThereAreErrors();

            var _retrieveCurrentPage = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 1,
                ChannelName = channel.ChannelName,
                LanguageName = currentCulture.Locale,
                IsForPreview = false
            };

            var result = await _contentRetriever.GetPagesAsync<SectionEvents>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, WebPageItemGuid)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var existingPage = result?.FirstOrDefault();
            if (existingPage is null)
            {
                return new NotFoundResult();
            }

            if (existingPage.SystemFields is null)
            {
                throw new NotFoundException("Page system fields are missing");
            }

            var itemData = await CreateSectionEventsItemData(request, channel.ChannelName, currentCulture.Locale, cancellationToken);

            await _webPageManager.TryCreateDraft(existingPage.SystemFields.WebPageItemID, currentCulture.Locale, cancellationToken: cancellationToken);

            if (!await _webPageManager.TryUpdateDraft(existingPage.SystemFields.WebPageItemID, currentCulture.Locale, new UpdateDraftData(itemData), cancellationToken))
            {
                throw new NotFoundException("Failed to update page draft");
            }

            if (!await _webPageManager.TryPublish(existingPage.SystemFields.WebPageItemID, currentCulture.Locale, cancellationToken: cancellationToken))
            {
                throw new NotFoundException("Failed to publish updated page");
            }

            var updatedResult = await _contentRetriever.GetPagesAsync<SectionEvents>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, WebPageItemGuid)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var updatedPage = updatedResult?.FirstOrDefault();
            return updatedPage != null
                ? await MapSectionEventsToPageOutput(updatedPage, channel.ChannelName, currentCulture.Locale, cancellationToken)
                : new BadRequestResult();
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

    public async Task DeletePageAsync(int channelId, Guid WebPageItemGuid, string? culture, bool deleteAllCultures, CancellationToken cancellationToken)
    {
        var currentCulture = await _mappingHelper.ValidateAndNormalizeSiteCultureAsync(channelId, culture ?? defaultCulture.Locale, cancellationToken) ?? defaultCulture;

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
            var allCultures = await _cultureService.GetSiteCulturesLanguageCodeAsync(channelId, cancellationToken);

            foreach (var cult in allCultures)
            {
                var _retrieveCurrentPage = new RetrievePagesParameters
                {
                    LinkedItemsMaxLevel = 1,
                    ChannelName = channel.ChannelName,
                    LanguageName = cult.ToString(),
                    IsForPreview = false
                };

                var result = await _contentRetriever.GetPagesAsync<SectionEvents>(_retrieveCurrentPage,
                    query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, WebPageItemGuid)),
                    RetrievalCacheSettings.CacheDisabled,
                    cancellationToken);

                var existingPage = result?.FirstOrDefault();
                if (existingPage is not null)
                {
                    await _webPageManager.Delete(
                        new DeleteWebPageParameters(existingPage.SystemFields.WebPageItemID, cult.ToString())
                        {
                            Permanently = true
                        },
                        cancellationToken);
                }
            }
        }
        else
        {
            var _retrieveCurrentPage = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 1,
                ChannelName = channel.ChannelName,
                LanguageName = currentCulture.Locale,
                IsForPreview = false
            };

            var result = await _contentRetriever.GetPagesAsync<SectionEvents>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, WebPageItemGuid)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var existingPage = result?.FirstOrDefault();
            if (existingPage is null)
            {
                throw new NotFoundException();
            }

            await _webPageManager.Delete(
                new DeleteWebPageParameters(existingPage.SystemFields.WebPageItemID, currentCulture.Locale)
                {
                    Permanently = true
                },
                cancellationToken);
        }
    }

    private async Task<ContentItemData> CreateSectionEventsItemData(
        SectionEventsPageContent content,
        string channelName,
        string culture,
        CancellationToken cancellationToken)
    {
        var eventsWebinarsListFeaturedPages = await _mappingHelper.BuildFeaturedEventOrWebinarReferencesAsync(
    content.EventsWebinarsListFeaturedPages, channelName, culture, cancellationToken);

        var teaserImageReferences = await _mappingHelper.BuildContentAttachmentImageReferences(content.MetadataTeaserImage, cancellationToken);
        var heroBackgroundImageReferences = await _mappingHelper.BuildContentAttachmentImageReferences(content.HeroBackgroundImage, cancellationToken);
        var heroBackgroundImageMobileReferences = await _mappingHelper.BuildContentAttachmentImageReferences(content.HeroBackgroundImage_Mobile, cancellationToken);
        var ogImageReferences = await _mappingHelper.BuildContentAttachmentImageReferences(content.MetadataOGImage, cancellationToken);
        var ogTwitterImageReferences = await _mappingHelper.BuildContentAttachmentImageReferences(content.MetadataOGTwitterImage, cancellationToken);

        return new ContentItemData(new Dictionary<string, object>
        {
            { nameof(SectionEvents.MetadataTitle), content.MetadataTitle ?? string.Empty },
            { nameof(SectionEvents.MetadataDescription), content.MetadataDescription ?? string.Empty },
            { nameof(SectionEvents.MetadataTeaserImage), teaserImageReferences },
            { nameof(SectionEvents.MetadataTeaserImageAltText), content.MetadataTeaserImageAltText ?? string.Empty },
            { nameof(SectionEvents.MetadataCanonical), content.MetadataCanonical ?? string.Empty },

            { nameof(SectionEvents.HeroLayout), content.HeroLayout ?? string.Empty },
            { nameof(SectionEvents.HeroTitle), content.HeroTitle ?? string.Empty },
            { nameof(SectionEvents.HeroDescription), content.HeroDescription ?? string.Empty },
            { nameof(SectionEvents.HeroBackgroundImage), heroBackgroundImageReferences },
            { nameof(SectionEvents.HeroBackgroundImageAltText), content.HeroBackgroundImageAltText ?? string.Empty },
            { nameof(SectionEvents.HeroBackgroundImage_Mobile), heroBackgroundImageMobileReferences },
            { nameof(SectionEvents.HeroBackgroundImage_MobileAltText), content.HeroBackgroundImage_MobileAltText ?? string.Empty },
            { nameof(SectionEvents.HeroCallToActionButtonType), content.HeroCallToActionButtonType ?? string.Empty },
            { nameof(SectionEvents.HeroCallToActionButtonText), content.HeroCallToActionButtonText ?? string.Empty },
            //{ nameof(SectionEvents.HeroCallToActionButtonUrl), heroCallToActionButtonUrlReferences },

            { nameof(SectionEvents.EventsWebinarsListTitle), content.EventsWebinarsListTitle ?? string.Empty },
            { nameof(SectionEvents.EventsWebinarsListDescription), content.EventsWebinarsListDescription ?? string.Empty },
            { nameof(SectionEvents.EventsWebinarsListFilteringEnabled), content.EventsWebinarsListFilteringEnabled },
            { nameof(SectionEvents.EventsWebinarsListFilters), NormalizeFilters(content.EventsWebinarsListFilters) },
            { nameof(SectionEvents.EventsWebinarsListFeaturedPages), eventsWebinarsListFeaturedPages },
            { nameof(SectionEvents.EventsWebinarsListAdvancedSearchDescription), content.EventsWebinarsListAdvancedSearchDescription ?? string.Empty },
            { nameof(SectionEvents.EventsWebinarsListAdvancedSearchLinkText), content.EventsWebinarsListAdvancedSearchLinkText ?? string.Empty },
            //{ nameof(SectionEvents.EventsWebinarsListAdvancedSearchLinkUrl), eventsWebinarsListAdvancedSearchLinkUrlReferences },
            { nameof(SectionEvents.ShowPastEvents), content.ShowPastEvents },

            { nameof(SectionEvents.MetadataOGTitle), content.MetadataOGTitle ?? string.Empty },
            { nameof(SectionEvents.MetadataOGDescription), content.MetadataOGDescription ?? string.Empty },
            { nameof(SectionEvents.MetadataOGImage), ogImageReferences },
            { nameof(SectionEvents.MetadataOGTwitterImageSizeCheckbox), content.MetadataOGTwitterImageSizeCheckbox },
            { nameof(SectionEvents.MetadataOGTwitterImage), ogTwitterImageReferences },
            { nameof(SectionEvents.MetadataNofollow), content.MetadataNofollow },
            { nameof(SectionEvents.MetadataNoindex), content.MetadataNoindex },
            { nameof(SectionEvents.IncludeInSitemap), content.IncludeInSitemap },

            { nameof(SectionEvents.PageTitle), content.MetadataTitle ?? string.Empty },
            { nameof(SectionEvents.PageKeywords), string.Empty },
        });
    }
    private static string NormalizeFilters(IEnumerable<string>? filters)
    {
        if (filters is null) return string.Empty;
        var tokens = filters
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Distinct(StringComparer.OrdinalIgnoreCase);
        return string.Join(';', tokens);
    }

    private async Task<PageOutput<SectionEventsPageContent>> MapSectionEventsToPageOutput(
        SectionEvents page, string channelName, string culture, CancellationToken cancellationToken)
    {
        var advancedSearchLinkPath = await _mappingHelper.ResolveWebPageRelatedItemPathAsync(
            page.EventsWebinarsListAdvancedSearchLinkUrl?.FirstOrDefault(),
            channelName, culture, cancellationToken);

        return new PageOutput<SectionEventsPageContent>
        {
            General = new PageGeneralOutput
            {
                Name = page.SystemFields?.WebPageItemName ?? string.Empty,
                Culture = culture ?? string.Empty,
                WebPageItemParentID = page.SystemFields?.WebPageItemParentID ?? 0,
                PageGuid = page.SystemFields?.WebPageItemGUID ?? Guid.Empty,
                WebPageItemID = page.SystemFields?.WebPageItemID ?? 0,
                ContentItemGuid = page.SystemFields?.ContentItemGUID ?? Guid.Empty,
                ContentItemID = page.SystemFields?.ContentItemID ?? 0,
                Path = page.SystemFields?.WebPageItemTreePath ?? string.Empty
            },
            Content = new SectionEventsPageContent
            {
                MetadataTitle = page.MetadataTitle,
                MetadataDescription = page.MetadataDescription,
                MetadataTeaserImage = page.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataTeaserImageAltText = page.MetadataTeaserImageAltText,
                MetadataCanonical = page.MetadataCanonical,

                HeroLayout = page.HeroLayout,
                HeroTitle = page.HeroTitle,
                HeroDescription = page.HeroDescription,
                HeroBackgroundImage = page.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                HeroBackgroundImageAltText = page.HeroBackgroundImageAltText,
                HeroBackgroundImage_Mobile = page.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                HeroBackgroundImage_MobileAltText = page.HeroBackgroundImage_MobileAltText,
                HeroCallToActionButtonType = page.HeroCallToActionButtonType,
                HeroCallToActionButtonText = page.HeroCallToActionButtonText,
                HeroCallToActionButtonUrl = page.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageItemTreePath ?? string.Empty,

                EventsWebinarsListTitle = page.EventsWebinarsListTitle,
                EventsWebinarsListDescription = page.EventsWebinarsListDescription,
                EventsWebinarsListFilteringEnabled = page.EventsWebinarsListFilteringEnabled,
                EventsWebinarsListFilters = string.IsNullOrWhiteSpace(page.EventsWebinarsListFilters)
                    ? new HashSet<string>()
                    : page.EventsWebinarsListFilters
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .ToHashSet(),
                EventsWebinarsListFeaturedPages = page.EventsWebinarsListFeaturedPages?
                    .OfType<IMetadata>()
                    .Select(c => c.MetadataTitle)
                    .ToList() ?? new List<string>(),
                EventsWebinarsListAdvancedSearchDescription = page.EventsWebinarsListAdvancedSearchDescription,
                EventsWebinarsListAdvancedSearchLinkText = page.EventsWebinarsListAdvancedSearchLinkText,
                EventsWebinarsListAdvancedSearchLinkUrl = advancedSearchLinkPath ?? string.Empty,
                ShowPastEvents = page.ShowPastEvents,

                MetadataOGTitle = page.MetadataOGTitle,
                MetadataOGDescription = page.MetadataOGDescription,
                MetadataOGImage = page.MetadataOGImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataOGTwitterImageSizeCheckbox = page.MetadataOGTwitterImageSizeCheckbox,
                MetadataOGTwitterImage = page.MetadataOGTwitterImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataNofollow = page.MetadataNofollow,
                MetadataNoindex = page.MetadataNoindex,
                IncludeInSitemap = page.IncludeInSitemap,
                PublishFrom = page.SystemFields?.ContentItemCommonDataFirstPublishedWhen,
                PublishTo = page.SystemFields?.ContentItemCommonDataLastPublishedWhen,
            },
            Metadata = new PageMetadataOutput
            {
                PageTitle = new InheritableString { Value = page.MetadataTitle ?? string.Empty },
                PageDescription = new InheritableString { Value = page.MetadataDescription ?? string.Empty },
                PageKeywords = new InheritableString { Value = string.Empty },
            },
            Navigation = new PageNavigationOutput
            {
                ShowInMenu = false,
                ShowInSitemap = page.IncludeInSitemap,
            },
        };
    }
}