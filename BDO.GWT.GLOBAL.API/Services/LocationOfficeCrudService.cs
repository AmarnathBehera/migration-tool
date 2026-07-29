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

public interface ILocationOfficeCrudService
{
    Task<ActionResult<IEnumerable<PageOutput<LocationOfficePageContent>>>> GetPagesAsync(int channelId, string? culture, bool? onlyPublished, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<LocationOfficePageContent>>> GetPageAsync(int channelId, Guid WebPageItemGuid, string? culture, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<LocationOfficePageContent>>> CreatePageAsync(int channelId, int WebPageParentID, string? culture, LocationOfficePageContent request, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<LocationOfficePageContent>>> CreatePageCultureVersionAsync(int channelId, Guid WebPageItemGuid, string? fromCulture, string? toCulture, LocationOfficePageContent request, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<LocationOfficePageContent>>> UpdatePageAsync(int channelId, Guid WebPageItemGuid, string? culture, LocationOfficePageContent request, CancellationToken cancellationToken);
    Task DeletePageAsync(int channelId, Guid WebPageItemGuid, string? culture, bool deleteAllCultures, CancellationToken cancellationToken);
}

public class LocationOfficeCrudService : ILocationOfficeCrudService
{
    private readonly IContentService _contentRetriever;
    private readonly IValidationState _validationState;
    private readonly ICultureService _cultureService;
    private readonly IPageContentValidator _pageContentValidator;
    private readonly IInfoProvider<ChannelInfo> _channelInfoProvider;
    private readonly IWebPageManager _webPageManager;
    private readonly IMappingHelper _mappingHelper;
    private static readonly LanguageTag defaultCulture = LanguageTag.GB;
    private const string LOCATION_OFFICE_CONTENT_TYPE = LocationOffice.CONTENT_TYPE_NAME;
    private const string WEB_PAGE_GUID = "WebPageItemGUID";

#pragma warning disable S107
    public LocationOfficeCrudService(
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

    public async Task<ActionResult<IEnumerable<PageOutput<LocationOfficePageContent>>>> GetPagesAsync(int channelId, string? culture, bool? onlyPublished, CancellationToken cancellationToken)
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

            var results = await _contentRetriever.GetPagesAsync<LocationOffice>(
                retrieveParameters,
                query => { },
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var offices = results?.ToList() ?? new List<LocationOffice>();

            return offices.Select(office => MapLocationOfficeToPageOutput(office, currentCulture.Locale)).ToList();
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

    public async Task<ActionResult<PageOutput<LocationOfficePageContent>>> GetPageAsync(int channelId, Guid WebPageItemGuid, string? culture, CancellationToken cancellationToken)
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

            var result = await _contentRetriever.GetPagesAsync<LocationOffice>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, WebPageItemGuid)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var office = result?.FirstOrDefault();

            if (office is null)
            {
                return new NotFoundResult();
            }

            return MapLocationOfficeToPageOutput(office, currentCulture.Locale);
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

    public async Task<ActionResult<PageOutput<LocationOfficePageContent>>> CreatePageAsync(int channelId, int WebPageParentID, string? culture, LocationOfficePageContent request, CancellationToken cancellationToken)
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
            var parentPage = await _contentRetriever.GetPagesAsync<LocationOffice>(_retrieveCurrentPage, query => query
                .Where(w => w.WhereEquals("WebPageItemID", WebPageParentID)), RetrievalCacheSettings.CacheDisabled, cancellationToken);

            if (parentPage == null)
            {
                _validationState.AddUnprocessableEntityError(nameof(WebPageParentID), ErrorMessage.ParentWithNodeIdNotFound(WebPageParentID));
                throw new UnprocessableEntityException();
            }

            _pageContentValidator.ValidateAndNormalizePageContent(request);

            _validationState.ThrowIfThereAreErrors();

            string pageDisplayName = request.MetadataTitle ?? "New Location Office Created by API";

            var itemData = await CreateLocationOfficeItemData(request, cancellationToken);

            var contentItemParameters = new ContentItemParameters(LOCATION_OFFICE_CONTENT_TYPE, itemData);
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

            var createdResult = await _contentRetriever.GetPagesAsync<LocationOffice>(_retrieveNewPage,
                query => query.Where(w => w.WhereEquals("WebPageItemID", newPageId)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var createdOffice = createdResult?.FirstOrDefault();
            return createdOffice != null ? MapLocationOfficeToPageOutput(createdOffice, currentCulture.Locale) : new BadRequestResult();
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

    public async Task<ActionResult<PageOutput<LocationOfficePageContent>>> CreatePageCultureVersionAsync(int channelId, Guid WebPageItemGuid, string? fromCulture, string? toCulture, LocationOfficePageContent request, CancellationToken cancellationToken)
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

            _pageContentValidator.ValidateAndNormalizePageContent(request);

            _validationState.ThrowIfThereAreErrors();

            var _retrieveCurrentPage = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 1,
                ChannelName = channel.ChannelName,
                LanguageName = fromLangCulture.Locale,
                IsForPreview = false
            };

            var result = await _contentRetriever.GetPagesAsync<LocationOffice>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, WebPageItemGuid)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var existingOffice = result?.FirstOrDefault();
            if (existingOffice is null)
            {
                return new NotFoundResult();
            }

            if (existingOffice.SystemFields is null)
            {
                throw new NotFoundException("Page system fields are missing");
            }

            var itemData = await CreateLocationOfficeItemData(request, cancellationToken);

            var createVariantParameters = new CMS.Websites.CreateLanguageVariantParameters(
                existingOffice.SystemFields.WebPageItemID,
                toLangCulture.Locale,
                existingOffice.SystemFields.ContentItemName ?? "Location Office culture variant",
                itemData);

            if (!await _webPageManager.TryCreateLanguageVariant(createVariantParameters, cancellationToken))
            {
                throw new NotFoundException("Failed to create language variant");
            }

            if (!await _webPageManager.TryUpdateDraft(existingOffice.SystemFields.WebPageItemID, toLangCulture.Locale, new UpdateDraftData(itemData), cancellationToken))
            {
                throw new NotFoundException("Failed to update page draft");
            }

            if (!await _webPageManager.TryPublish(existingOffice.SystemFields.WebPageItemID, toLangCulture.Locale, cancellationToken: cancellationToken))
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

            var variantResult = await _contentRetriever.GetPagesAsync<LocationOffice>(_retrieveNewVariant,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, WebPageItemGuid)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var variantOffice = variantResult?.FirstOrDefault();
            return variantOffice != null ? MapLocationOfficeToPageOutput(variantOffice, toLangCulture.Locale) : new BadRequestResult();
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

    public async Task<ActionResult<PageOutput<LocationOfficePageContent>>> UpdatePageAsync(int channelId, Guid WebPageItemGuid, string? culture, LocationOfficePageContent request, CancellationToken cancellationToken)
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

            var result = await _contentRetriever.GetPagesAsync<LocationOffice>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, WebPageItemGuid)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var existingOffice = result?.FirstOrDefault();
            if (existingOffice is null)
            {
                return new NotFoundResult();
            }

            if (existingOffice.SystemFields is null)
            {
                throw new NotFoundException("Page system fields are missing");
            }

            var itemData = await CreateLocationOfficeItemData(request, cancellationToken);

            await _webPageManager.TryCreateDraft(existingOffice.SystemFields.WebPageItemID, currentCulture.Locale, cancellationToken: cancellationToken);

            if (!await _webPageManager.TryUpdateDraft(existingOffice.SystemFields.WebPageItemID, currentCulture.Locale, new UpdateDraftData(itemData), cancellationToken))
            {
                throw new NotFoundException("Failed to update page draft");
            }

            if (!await _webPageManager.TryPublish(existingOffice.SystemFields.WebPageItemID, currentCulture.Locale, cancellationToken: cancellationToken))
            {
                throw new NotFoundException("Failed to publish updated page");
            }

            var updatedResult = await _contentRetriever.GetPagesAsync<LocationOffice>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, WebPageItemGuid)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var updatedOffice = updatedResult?.FirstOrDefault();
            return updatedOffice != null ? MapLocationOfficeToPageOutput(updatedOffice, currentCulture.Locale) : new BadRequestResult();
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

                var result = await _contentRetriever.GetPagesAsync<LocationOffice>(_retrieveCurrentPage,
                    query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, WebPageItemGuid)),
                    RetrievalCacheSettings.CacheDisabled,
                    cancellationToken);

                var existingOffice = result?.FirstOrDefault();
                if (existingOffice is not null)
                {
                    await _webPageManager.Delete(
                        new DeleteWebPageParameters(existingOffice.SystemFields.WebPageItemID, cult.ToString())
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

            var result = await _contentRetriever.GetPagesAsync<LocationOffice>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, WebPageItemGuid)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var existingOffice = result?.FirstOrDefault();
            if (existingOffice is null)
            {
                throw new NotFoundException();
            }

            await _webPageManager.Delete(
                new DeleteWebPageParameters(existingOffice.SystemFields.WebPageItemID, currentCulture.Locale)
                {
                    Permanently = true
                },
                cancellationToken);
        }
    }

    private async Task<ContentItemData> CreateLocationOfficeItemData(LocationOfficePageContent content, CancellationToken cancellationToken)
    {
        var teaserImageReferences = await _mappingHelper.BuildContentAttachmentImageReferences(content.MetadataTeaserImage, cancellationToken);
        var officeImageReferences = await _mappingHelper.BuildContentAttachmentImageReferences(content.OfficeImage, cancellationToken);
        var ogImageReferences = await _mappingHelper.BuildContentAttachmentImageReferences(content.MetadataOGImage, cancellationToken);
        var ogTwitterImageReferences = await _mappingHelper.BuildContentAttachmentImageReferences(content.MetadataOGTwitterImage, cancellationToken);

        return new ContentItemData(new Dictionary<string, object>
        {
            { nameof(LocationOffice.MetadataTitle), content.MetadataTitle ?? string.Empty },
            { nameof(LocationOffice.MetadataDescription), content.MetadataDescription ?? string.Empty },
            { nameof(LocationOffice.MetadataTeaserImage), teaserImageReferences },
            { nameof(LocationOffice.MetadataTeaserImageAltText), content.MetadataTeaserImageAltText ?? string.Empty },
            { nameof(LocationOffice.MetadataCanonical), content.MetadataCanonical ?? string.Empty },

            { nameof(LocationOffice.OfficeImage), officeImageReferences },
            { nameof(LocationOffice.OfficeImageAltText), content.OfficeImageAltText ?? string.Empty },

            { nameof(LocationOffice.LocationAddressLine1), content.LocationAddressLine1 ?? string.Empty },
            { nameof(LocationOffice.LocationAddressLine2), content.LocationAddressLine2 ?? string.Empty },
            { nameof(LocationOffice.LocationZip), content.LocationZip ?? string.Empty },
            { nameof(LocationOffice.LocationZipDisplayType), content.LocationZipDisplayType ?? string.Empty },
            { nameof(LocationOffice.LocationCountry), content.LocationCountry ?? string.Empty },
            { nameof(LocationOffice.LocationLatitude), content.LocationLatitude ?? string.Empty },
            { nameof(LocationOffice.LocationLongitude), content.LocationLongitude ?? string.Empty },
            { nameof(LocationOffice.LocationEmail), content.LocationEmail ?? string.Empty },
            { nameof(LocationOffice.LocationType), content.LocationType },
            { nameof(LocationOffice.LocationPhone), content.LocationPhone ?? string.Empty },
            { nameof(LocationOffice.LocationFax), content.LocationFax ?? string.Empty },
            { nameof(LocationOffice.OpeningHours), content.OpeningHours ?? string.Empty },

            { nameof(LocationOffice.LocationPOBoxNumber), content.LocationPOBoxNumber ?? string.Empty },
            { nameof(LocationOffice.LocationPOCity), content.LocationPOCity ?? string.Empty },
            { nameof(LocationOffice.LocationPOZip), content.LocationPOZip ?? string.Empty },
            { nameof(LocationOffice.LocationPOZipDisplayType), content.LocationPOZipDisplayType ?? string.Empty },
            { nameof(LocationOffice.LocationPOCountry), content.LocationPOCountry ?? string.Empty },

            { nameof(LocationOffice.MetadataOGTitle), content.MetadataOGTitle ?? string.Empty },
            { nameof(LocationOffice.MetadataOGDescription), content.MetadataOGDescription ?? string.Empty },
            { nameof(LocationOffice.MetadataOGImage), ogImageReferences },
            { nameof(LocationOffice.MetadataOGTwitterImageSizeCheckbox), content.MetadataOGTwitterImageSizeCheckbox },
            { nameof(LocationOffice.MetadataOGTwitterImage), ogTwitterImageReferences },
            { nameof(LocationOffice.MetadataNofollow), content.MetadataNofollow },
            { nameof(LocationOffice.MetadataNoindex), content.MetadataNoindex },
            { nameof(LocationOffice.IncludeInSitemap), content.IncludeInSitemap },

            { nameof(LocationOffice.PageTitle), content.MetadataTitle ?? string.Empty },
            { nameof(LocationOffice.PageKeywords), string.Empty },
        });
    }

    private static PageOutput<LocationOfficePageContent> MapLocationOfficeToPageOutput(LocationOffice office, string culture)
    {
        return new PageOutput<LocationOfficePageContent>
        {
            General = new PageGeneralOutput
            {
                Name = office.SystemFields?.WebPageItemName ?? string.Empty,
                Culture = culture ?? string.Empty,
                WebPageItemParentID = office.SystemFields?.WebPageItemParentID ?? 0,
                PageGuid = office.SystemFields?.WebPageItemGUID ?? Guid.Empty,
                WebPageItemID = office.SystemFields?.WebPageItemID ?? 0,
                ContentItemGuid = office.SystemFields?.ContentItemGUID ?? Guid.Empty,
                ContentItemID = office.SystemFields?.ContentItemID ?? 0,
                Path = office.SystemFields?.WebPageItemTreePath ?? string.Empty
            },
            Content = new LocationOfficePageContent
            {
                MetadataTitle = office.MetadataTitle,
                MetadataDescription = office.MetadataDescription,
                MetadataTeaserImage = office.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataTeaserImageAltText = office.MetadataTeaserImageAltText,
                MetadataCanonical = office.MetadataCanonical,

                OfficeImage = office.OfficeImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url ?? string.Empty,
                OfficeImageAltText = office.OfficeImageAltText ?? string.Empty,

                LocationAddressLine1 = office.LocationAddressLine1,
                LocationAddressLine2 = office.LocationAddressLine2,
                LocationZip = office.LocationZip,
                LocationZipDisplayType = office.LocationZipDisplayType,
                LocationCountry = office.LocationCountry,
                LocationLatitude = office.LocationLatitude,
                LocationLongitude = office.LocationLongitude,
                LocationEmail = office.LocationEmail,
                //LocationType = office.LocationType,
                LocationPhone = office.LocationPhone,
                LocationFax = office.LocationFax,
                OpeningHours = office.OpeningHours,

                LocationPOBoxNumber = office.LocationPOBoxNumber,
                LocationPOCity = office.LocationPOCity,
                LocationPOZip = office.LocationPOZip,
                LocationPOZipDisplayType = office.LocationPOZipDisplayType,
                LocationPOCountry = office.LocationPOCountry,

                MetadataOGTitle = office.MetadataOGTitle,
                MetadataOGDescription = office.MetadataOGDescription,
                MetadataOGImage = office.MetadataOGImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataOGTwitterImageSizeCheckbox = office.MetadataOGTwitterImageSizeCheckbox,
                MetadataOGTwitterImage = office.MetadataOGTwitterImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataNofollow = office.MetadataNofollow,
                MetadataNoindex = office.MetadataNoindex,
                IncludeInSitemap = office.IncludeInSitemap,
                PublishFrom = office.SystemFields?.ContentItemCommonDataFirstPublishedWhen,
                PublishTo = office.SystemFields?.ContentItemCommonDataLastPublishedWhen,
            },
            Metadata = new PageMetadataOutput
            {
                PageTitle = new InheritableString { Value = office.MetadataTitle ?? string.Empty },
                PageDescription = new InheritableString { Value = office.MetadataDescription ?? string.Empty },
                PageKeywords = new InheritableString { Value = string.Empty },
            },
            Navigation = new PageNavigationOutput
            {
                ShowInMenu = false,
                ShowInSitemap = office.IncludeInSitemap,
            },
        };
    }
}