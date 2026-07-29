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

public interface IInsightCrudService
{
    Task<ActionResult<IEnumerable<PageOutput<InsightPageContent>>>> GetInsightPagesAsync(int channelId, string? culture, bool? onlyPublished, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<InsightPageContent>>> GetInsightPageAsync(int channelId, Guid pageGUID, string? culture, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<InsightPageContent>>> CreatePageAsync(int channelId, int nodeParentID, string? culture, InsightPageContent request, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<InsightPageContent>>> CreatePageCultureVersionAsync(int channelId, Guid pageGUID, string? fromCulture, string? toCulture, InsightPageContent request, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<InsightPageContent>>> UpdatePageAsync(int channelId, Guid pageGUID, string? culture, InsightPageContent request, CancellationToken cancellationToken);
    Task DeletePageAsync(int channelId, Guid pageGUID, string? culture, bool deleteAllCultures, CancellationToken cancellationToken);
}

public class InsightCrudService : IInsightCrudService
{
    private readonly IContentService _contentRetriever;
    private readonly IValidationState _validationState;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly ICultureService _cultureService;
    private readonly IPageContentValidator _pageContentValidator;
    private readonly IInfoProvider<ChannelInfo> _channelInfoProvider;
    private readonly IWebPageManager _webPageManager;
    private readonly IMappingHelper _mappingHelper;
    private static readonly LanguageTag defaultCulture = LanguageTag.GB;
    private const string INSIGHT_CONTENT_TYPE = Insight.CONTENT_TYPE_NAME;
    private const string WEB_PAGE_GUID = "WebPageItemGUID";

#pragma warning disable S107
    public InsightCrudService(
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
        _websiteChannelContext = websiteChannelContext;
        _cultureService = cultureService;
        _pageContentValidator = pageContentValidator;
        _channelInfoProvider = channelInfoProvider;
        _webPageManager = webPageManagerFactory.Create(_websiteChannelContext.WebsiteChannelID == 0 ? 1 : _websiteChannelContext.WebsiteChannelID, user.UserID);
        _mappingHelper = mappingHelper;
    }

    public async Task<ActionResult<IEnumerable<PageOutput<InsightPageContent>>>> GetInsightPagesAsync(int channelId, string? culture, bool? onlyPublished, CancellationToken cancellationToken)
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

            var results = await _contentRetriever.GetPagesAsync<Insight>(
                retrieveParameters,
                query => { },
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var insights = results?.ToList() ?? new List<Insight>();

            return insights.Select(insight => MapInsightToPageOutput(insight, currentCulture.Locale)).ToList();
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

    public async Task<ActionResult<PageOutput<InsightPageContent>>> GetInsightPageAsync(int channelId, Guid pageGUID, string? culture, CancellationToken cancellationToken)
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

            var result = await _contentRetriever.GetPagesAsync<Insight>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var insight = result?.FirstOrDefault();

            if (insight is null)
            {
                return new NotFoundResult();
            }

            return MapInsightToPageOutput(insight, currentCulture.Locale);
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

    public async Task<ActionResult<PageOutput<InsightPageContent>>> CreatePageAsync(int channelId, int nodeParentID, string? culture, InsightPageContent request, CancellationToken cancellationToken)
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

            var parentPage = await _contentRetriever.GetPagesAsync<SectionInsightsPage>(_retrieveCurrentPage, query => query
                .Where(w => w.WhereEquals("WebPageItemID", nodeParentID)), RetrievalCacheSettings.CacheDisabled, cancellationToken);

            if (parentPage == null)
            {
                _validationState.AddUnprocessableEntityError(nameof(nodeParentID), ErrorMessage.ParentWithNodeIdNotFound(nodeParentID));
                throw new UnprocessableEntityException();
            }

            _pageContentValidator.ValidateAndNormalizePageContent(request);

            _validationState.ThrowIfThereAreErrors();

            // The page display name shown in the admin UI
            string pageDisplayName = request.MetadataTitle ?? "New Insight Created by API";

            var itemData = await CreateInsightItemData(request, cancellationToken);

            // Creates a page metadata object
            var contentItemParameters = new ContentItemParameters(INSIGHT_CONTENT_TYPE, itemData);
            var createPageParameters = new CreateWebPageParameters(pageDisplayName, currentCulture.Locale, contentItemParameters)
            {
                ParentWebPageItemID = nodeParentID
            };

            // Creates the page in the database
            int newPageId = await _webPageManager.Create(createPageParameters, cancellationToken);

            // Update the draft with new content
            if (!await _webPageManager.TryUpdateDraft(newPageId, currentCulture.Locale, new UpdateDraftData(itemData), cancellationToken))
            {
                throw new NotFoundException("Failed to update page draft");
            }

            // Publish the updated page
            if (!await _webPageManager.TryPublish(newPageId, currentCulture.Locale, cancellationToken: cancellationToken))
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

            var createdResult = await _contentRetriever.GetPagesAsync<Insight>(_retrieveNewPage,
                query => query.Where(w => w.WhereEquals("WebPageItemID", newPageId)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var createdInsight = createdResult?.FirstOrDefault();
            return createdInsight != null ? MapInsightToPageOutput(createdInsight, currentCulture.Locale) : new BadRequestResult();
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

    public async Task<ActionResult<PageOutput<InsightPageContent>>> CreatePageCultureVersionAsync(int channelId, Guid pageGUID, string? fromCulture, string? toCulture, InsightPageContent request, CancellationToken cancellationToken)
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

            // Get existing page
            var result = await _contentRetriever.GetPagesAsync<Insight>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var existingInsight = result?.FirstOrDefault();
            if (existingInsight is null)
            {
                return new NotFoundResult();
            }

            if (existingInsight.SystemFields is null)
            {
                throw new NotFoundException("Page system fields are missing");
            }

            // Create new culture version data
            var itemData = await CreateInsightItemData(request, cancellationToken);

            // Create language variant for the existing page
            var createVariantParameters = new CMS.Websites.CreateLanguageVariantParameters(
                existingInsight.SystemFields.WebPageItemID,
                toLangCulture.Locale,
                existingInsight.SystemFields.ContentItemName ?? "Insight culture variant",
                itemData);

            // Creates the language variant in the database
            if (!await _webPageManager.TryCreateLanguageVariant(createVariantParameters, cancellationToken))
            {
                throw new NotFoundException("Failed to create language variant");
            }

            // Update the draft with new content
            if (!await _webPageManager.TryUpdateDraft(existingInsight.SystemFields.WebPageItemID, toLangCulture.Locale, new UpdateDraftData(itemData), cancellationToken))
            {
                throw new NotFoundException("Failed to update page draft");
            }

            // Publish the updated page
            if (!await _webPageManager.TryPublish(existingInsight.SystemFields.WebPageItemID, toLangCulture.Locale, cancellationToken: cancellationToken))
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

            var variantResult = await _contentRetriever.GetPagesAsync<Insight>(_retrieveNewVariant,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var variantInsight = variantResult?.FirstOrDefault();
            return variantInsight != null ? MapInsightToPageOutput(variantInsight, toLangCulture.Locale) : new BadRequestResult();
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

    public async Task<ActionResult<PageOutput<InsightPageContent>>> UpdatePageAsync(int channelId, Guid pageGUID, string? culture, InsightPageContent request, CancellationToken cancellationToken)
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

            // Get existing page
            var result = await _contentRetriever.GetPagesAsync<Insight>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var existingInsight = result?.FirstOrDefault();
            if (existingInsight is null)
            {
                return new NotFoundResult();
            }

            if (existingInsight.SystemFields is null)
            {
                throw new NotFoundException("Page system fields are missing");
            }

            // Build ContentItemData for the update
            var itemData = await CreateInsightItemData(request, cancellationToken);

            // Create a draft, update it, and publish
            await _webPageManager.TryCreateDraft(existingInsight.SystemFields.WebPageItemID, currentCulture.Locale, cancellationToken: cancellationToken);

            // Update the draft with new content
            if (!await _webPageManager.TryUpdateDraft(existingInsight.SystemFields.WebPageItemID, currentCulture.Locale, new UpdateDraftData(itemData), cancellationToken))
            {
                throw new NotFoundException("Failed to update page draft");
            }

            // Publish the updated page
            if (!await _webPageManager.TryPublish(existingInsight.SystemFields.WebPageItemID, currentCulture.Locale, cancellationToken: cancellationToken))
            {
                throw new NotFoundException("Failed to publish updated page");
            }

            // Retrieve the updated page and return it
            var updatedResult = await _contentRetriever.GetPagesAsync<Insight>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var updatedInsight = updatedResult?.FirstOrDefault();
            return updatedInsight != null ? MapInsightToPageOutput(updatedInsight, currentCulture.Locale) : new BadRequestResult();
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
            // Get all available cultures for the channel
            var allCultures = await _cultureService.GetSiteCulturesLanguageCodeAsync(channelId, cancellationToken);

            // Delete the page variant for each culture
            foreach (var cult in allCultures)
            {
                var _retrieveCurrentPage = new RetrievePagesParameters
                {
                    LinkedItemsMaxLevel = 1,
                    ChannelName = channel.ChannelName,
                    LanguageName = cult.ToString(),
                    IsForPreview = false
                };

                // Get existing page for this culture
                var result = await _contentRetriever.GetPagesAsync<Insight>(_retrieveCurrentPage,
                    query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID)),
                    RetrievalCacheSettings.CacheDisabled,
                    cancellationToken);

                var existingInsight = result?.FirstOrDefault();
                if (existingInsight is not null)
                {
                    await _webPageManager.Delete(
                        new DeleteWebPageParameters(existingInsight.SystemFields.WebPageItemID, cult.ToString())
                        {
                            Permanently = true
                        },
                        cancellationToken);
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
            var result = await _contentRetriever.GetPagesAsync<Insight>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var existingInsight = result?.FirstOrDefault();
            if (existingInsight is null)
            {
                throw new NotFoundException();
            }

            await _webPageManager.Delete(
                new DeleteWebPageParameters(existingInsight.SystemFields.WebPageItemID, currentCulture.Locale)
                {
                    Permanently = true
                },
                cancellationToken);
        }
    }

    private async Task<ContentItemData> CreateInsightItemData(InsightPageContent content, CancellationToken cancellationToken)
    {
        var metadataBusinessIssues = await _mappingHelper.BuildReferencesAsync<Businessissue>(content.MetadataBusinessIssues, x => x.Name, cancellationToken);
        var contentType = await _mappingHelper.BuildReferencesAsync<Contenttypes>(content.ContentType, x => x.Name, cancellationToken);
        var metadataBusinessLines = await _mappingHelper.BuildReferencesPageAsync<BusinessLine>(content.MetadataBusinessLines, x => x.MetadataTitle, cancellationToken);
        var metadataServiceAreas = await _mappingHelper.BuildReferencesPageAsync<ServiceArea>(content.MetadataServiceAreas, x => x.MetadataTitle, cancellationToken);
        var metadataServices = await _mappingHelper.BuildReferencesPageAsync<Service>(content.MetadataServices, x => x.MetadataTitle, cancellationToken);
        var metadataIndustryCategories = await _mappingHelper.BuildReferencesPageAsync<IndustryCategory>(content.MetadataIndustryCategories, x => x.MetadataTitle, cancellationToken);
        var metadataIndustries = await _mappingHelper.BuildReferencesPageAsync<Industry>(content.MetadataIndustries, x => x.MetadataTitle, cancellationToken);
        var metadataSpecialtiesCategories = await _mappingHelper.BuildReferencesPageAsync<SpecialtiesCategory>(content.MetadataSpecialtiesCategories, x => x.MetadataTitle, cancellationToken);
        var metadataSpecialtiesAreas = await _mappingHelper.BuildReferencesPageAsync<SpecialtiesArea>(content.MetadataSpecialtiesAreas, x => x.MetadataTitle, cancellationToken);
        var metadataSpecialtiesPages = await _mappingHelper.BuildReferencesPageAsync<SpecialtiesPage>(content.MetadataSpecialtiesPages, x => x.MetadataTitle, cancellationToken);
        var metadataOfficeLocation = await _mappingHelper.BuildReferencesPageAsync<LocationOffice>(content.MetadataOfficeLocation, x => x.MetadataTitle, cancellationToken);
        var micrositeMasterPages = await _mappingHelper.BuildReferencesPageAsync<MicrositeMasterPage>(content.MicrositeMasterPages, x => x.MetadataTitle, cancellationToken);
        var micrositePages = await _mappingHelper.BuildReferencesPageAsync<MicrositePage>(content.MicrositePages, x => x.MetadataTitle, cancellationToken);
        var insights = await _mappingHelper.BuildReferencesPageAsync<Insight>(content.Insights, x => x.MetadataTitle, cancellationToken);

        var teaserImage = await _mappingHelper.BuildContentAttachmentImageReferences(content.MetadataTeaserImage, cancellationToken);
        var image = await _mappingHelper.BuildContentAttachmentImageReferences(content.Image, cancellationToken);
        var ogImage = await _mappingHelper.BuildContentAttachmentImageReferences(content.MetadataOGImage, cancellationToken);
        var ogTwitterImage = await _mappingHelper.BuildContentAttachmentImageReferences(content.MetadataOGTwitterImage, cancellationToken);

        //var downloadAttachment = await _mappingHelper.BuildContentAttachmentReferences(content.DownloadButtonAttachment, cancellationToken);
        //var insightAttachment = await _mappingHelper.BuildContentAttachmentReferences(content.InsightAttachment, cancellationToken);

        return new ContentItemData(new Dictionary<string, object>
        {
            { nameof(Insight.MetadataTitle), content.MetadataTitle ?? string.Empty },
            { nameof(Insight.MetadataDescription), content.MetadataDescription ?? string.Empty },
            { nameof(Insight.MetadataTeaserImage), teaserImage },
            { nameof(Insight.MetadataTeaserImageAltText), content.MetadataTeaserImageAltText ?? string.Empty },
            { nameof(Insight.MetadataCanonical), content.MetadataCanonical ?? string.Empty },
            { nameof(Insight.ComingSoon), content.ComingSoon },
            { nameof(Insight.ShowSidebar), content.ShowSidebar },
            { nameof(Insight.ShowPrintButton), content.ShowPrintButton },
            { nameof(Insight.ShowSocialMediaButtons), content.ShowSocialMediaButtons },
            { nameof(Insight.ShowAuthorsAtTheTop), content.ShowAuthorsAtTheTop },
            { nameof(Insight.UseNarrowLayout), content.UseNarrowLayout },
            //{ nameof(Insight.DownloadButtonAttachment), downloadAttachment },

            { nameof(Insight.MetadataBusinessIssues), metadataBusinessIssues },
            { nameof(Insight.ContentType), contentType },
            { nameof(Insight.MetadataBusinessLine), metadataBusinessLines },
            { nameof(Insight.MetadataServiceAreas), metadataServiceAreas },
            { nameof(Insight.MetadataServices), metadataServices },
            { nameof(Insight.MetadataIndustryCategories), metadataIndustryCategories },
            { nameof(Insight.MetadataIndustries), metadataIndustries },
            { nameof(Insight.MetadataSpecialtiesCategories), metadataSpecialtiesCategories },
            { nameof(Insight.MetadataSpecialtiesAreas), metadataSpecialtiesAreas },
            { nameof(Insight.MetadataSpecialtiesPages), metadataSpecialtiesPages },
            { nameof(Insight.MetadataOfficeLocation), metadataOfficeLocation },
            { nameof(Insight.MicrositeMasterPages), micrositeMasterPages },
            { nameof(Insight.MicrositePages), micrositePages },
            { nameof(Insight.Insights), insights },

            { nameof(Insight.MetadataOGTitle), content.MetadataOGTitle ?? string.Empty },
            { nameof(Insight.MetadataOGDescription), content.MetadataOGDescription ?? string.Empty },
            { nameof(Insight.MetadataOGImage), ogImage },
            { nameof(Insight.MetadataOGTwitterImage), ogTwitterImage },
            { nameof(Insight.MetadataOGTwitterImageSizeCheckbox), content.MetadataOGTwitterImageSizeCheckbox },
            { nameof(Insight.MetadataNofollow), content.MetadataNofollow },
            { nameof(Insight.MetadataNoindex), content.MetadataNoindex },
            { nameof(Insight.IncludeInSitemap), content.IncludeInSitemap },

            { nameof(Insight.PageTitle), content.MetadataTitle ?? string.Empty },
            { nameof(Insight.PageKeywords), string.Empty },
        });
    }

    private static PageOutput<InsightPageContent> MapInsightToPageOutput(Insight insight, string culture)
    {
        return new PageOutput<InsightPageContent>
        {
            General = new PageGeneralOutput
            {
                Name = insight.SystemFields?.WebPageItemName ?? string.Empty,
                Culture = culture ?? string.Empty,
                WebPageItemParentID = insight.SystemFields?.WebPageItemParentID ?? 0,
                PageGuid = insight.SystemFields?.WebPageItemGUID ?? Guid.Empty,
                WebPageItemID = insight.SystemFields?.WebPageItemID ?? 0,
                ContentItemGuid = insight.SystemFields?.ContentItemGUID ?? Guid.Empty,
                ContentItemID = insight.SystemFields?.ContentItemID ?? 0,
                Path = insight.SystemFields?.WebPageItemTreePath ?? string.Empty
            },
            Content = new InsightPageContent
            {
                MetadataTitle = insight.MetadataTitle,
                MetadataDescription = insight.MetadataDescription,
                MetadataCanonical = insight.MetadataCanonical,
                MetadataTeaserImage = insight.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataTeaserImageAltText = insight.MetadataTeaserImageAltText,
                ComingSoon = insight.ComingSoon,
                ShowSidebar = insight.ShowSidebar,
                ShowPrintButton = insight.ShowPrintButton,
                ShowSocialMediaButtons = insight.ShowSocialMediaButtons,
                UseNarrowLayout = insight.UseNarrowLayout,
                ShowAuthorsAtTheTop = insight.ShowAuthorsAtTheTop,
                MetadataBusinessIssues = insight.MetadataBusinessIssues.Select(c => c.Name).ToList(),
                MetadataBusinessLines = insight.MetadataBusinessLine.Select(c => c.MetadataTitle).ToList(),
                MetadataServiceAreas = insight.MetadataServiceAreas.Select(c => c.MetadataTitle).ToList(),
                MetadataServices = insight.MetadataServices.Select(c => c.MetadataTitle).ToList(),
                MetadataIndustries = insight.MetadataIndustries.Select(c => c.MetadataTitle).ToList(),
                MetadataIndustryCategories = insight.MetadataIndustryCategories.Select(c => c.MetadataTitle).ToList(),
                MetadataSpecialtiesCategories = insight.MetadataSpecialtiesCategories.Select(c => c.MetadataTitle).ToList(),
                MetadataSpecialtiesAreas = insight.MetadataSpecialtiesAreas.Select(c => c.MetadataTitle).ToList(),
                MetadataSpecialtiesPages = insight.MetadataSpecialtiesPages.Select(c => c.MetadataTitle).ToList(),
                MetadataOfficeLocation = insight.MetadataOfficeLocation.Select(c => c.MetadataTitle).ToList(),
                MicrositeMasterPages = insight.MicrositeMasterPages.Select(c => c.MetadataTitle).ToList(),
                MicrositePages = insight.MicrositePages.Select(c => c.MetadataTitle).ToList(),
                IncludeInSitemap = insight.IncludeInSitemap,
                MetadataNofollow = insight.MetadataNofollow,
                MetadataNoindex = insight.MetadataNoindex,
                PublishFrom = insight.SystemFields?.ContentItemCommonDataFirstPublishedWhen,
                PublishTo = insight.SystemFields?.ContentItemCommonDataLastPublishedWhen,
            },
            Metadata = new PageMetadataOutput
            {
                PageTitle = new InheritableString { Value = insight.MetadataTitle ?? string.Empty },
                PageDescription = new InheritableString { Value = insight.MetadataDescription ?? string.Empty },
                PageKeywords = new InheritableString { Value = string.Empty },
            },
            Navigation = new PageNavigationOutput
            {
                ShowInMenu = false,
                ShowInSitemap = insight.IncludeInSitemap,
            },
        };
    }
}