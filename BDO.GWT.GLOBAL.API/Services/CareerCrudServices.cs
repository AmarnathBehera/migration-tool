using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Domain.Core.Entities;
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

namespace BDO.GWT.GLOBAL.API.Services
{

    public interface ICareerCrudService
    {
        Task<ActionResult<IEnumerable<PageOutput<CareerPageContent>>>> GetCareerPagesAsync(int channelId, string? culture, bool? onlyPublished, CancellationToken cancellationToken);
        Task<ActionResult<PageOutput<CareerPageContent>>> GetCareerPageAsync(int channelId, Guid pageGUID, string? culture, CancellationToken cancellationToken);
        Task<ActionResult<PageOutput<CareerPageContent>>> CreateCareerPageAsync(int channelId, int nodeParentID, string? culture, CareerPageContent request, CancellationToken cancellationToken);
        Task<ActionResult<PageOutput<CareerPageContent>>> CreateCareerPageCultureVersionAsync(int channelId, Guid pageGUID, string? fromCulture, string? toCulture, CareerPageContent request, CancellationToken cancellationToken);
        Task<ActionResult<PageOutput<CareerPageContent>>> UpdateCareerPageAsync(int channelId, Guid pageGUID, string? culture, CareerPageContent request, CancellationToken cancellationToken);
        Task DeleteCareerPageAsync(int channelId, Guid pageGUID, string? culture, bool deleteAllCultures, CancellationToken cancellationToken);
    }
    public class CareerCrudService : ICareerCrudService
    {
        private readonly IContentService _contentRetriever;
        private readonly IValidationState _validationState;
        private readonly IWebsiteChannelContext _websiteChannelContext;
        private readonly ICultureService _cultureService;
        private readonly IInfoProvider<ChannelInfo> _channelInfoProvider;
        private readonly IWebPageManager _webPageManager;
        private readonly IMappingHelper _mappingHelper;
        private readonly IPageContentValidator _pageContentValidator;
        private readonly ICultureValidationService _cultureValidationService;
        private static readonly LanguageTag defaultCulture = LanguageTag.GB;
        private const string CAREER_CONTENT_TYPE = Career.CONTENT_TYPE_NAME;
        private const string WEB_PAGE_GUID = "WebPageItemGUID";
        private string CHANNEL_NAME = string.Empty;
        public CareerCrudService( IContentService contentRetriever, IValidationState validationState, 
            IWebsiteChannelContext websiteChannelContext, ICultureService cultureService, 
            IInfoProvider<ChannelInfo> channelInfoProvider,  
            IMappingHelper mappingHelper, IPageContentValidator pageContentValidator, ICultureValidationService cultureValidationService
            , IWebPageManagerFactory webPageManagerFactory, IUserInfoProvider userInfoProvider)
        {
            UserInfo user = userInfoProvider.Get("administrator");
            _contentRetriever = contentRetriever;
            _validationState = validationState;
            _websiteChannelContext = websiteChannelContext;
            _cultureService = cultureService;
            _channelInfoProvider = channelInfoProvider;
            _webPageManager = webPageManagerFactory.Create(_websiteChannelContext.WebsiteChannelID == 0 ? 1 : _websiteChannelContext.WebsiteChannelID, user.UserID);
            _mappingHelper = mappingHelper;
            _pageContentValidator = pageContentValidator;
            _cultureValidationService = cultureValidationService;

        }
        public async Task<ActionResult<PageOutput<CareerPageContent>>> CreateCareerPageAsync(int channelId, int nodeParentID, string? culture, CareerPageContent request, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                return new BadRequestResult();
            }

            var currentCulture = await _cultureValidationService.ValidateAndNormalizeSiteCultureAsync(channelId,
                                                     culture ?? defaultCulture.Language, cancellationToken) ?? defaultCulture;

            if (string.IsNullOrEmpty(currentCulture.Locale))
            {
                throw new NotFoundException("Invalid culture specified");
            }

            var channel = await _channelInfoProvider.GetAsync(channelId, cancellationToken);
            if (string.IsNullOrEmpty(channel?.ChannelName))
            {
                throw new NotFoundException("Channel not found");
            }

            _pageContentValidator.ValidateAndNormalizePageContent(request);

            _validationState.ThrowIfThereAreErrors();

            CHANNEL_NAME = channel.ChannelName;
            var _retrieveCurrentPage = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 3,
                ChannelName = channel.ChannelName,
                LanguageName = currentCulture.Locale,
                IsForPreview = false
            };

            var parentPage = await _contentRetriever.GetPagesAsync<Careers>(_retrieveCurrentPage, query => query
                .Where(w => w.WhereEquals("WebPageItemID", nodeParentID)), RetrievalCacheSettings.CacheDisabled, cancellationToken);

            if (parentPage == null || !parentPage.Any())
            {
                var message = ErrorMessage.ParentWithNodeIdNotFound(nodeParentID);

                _validationState.AddUnprocessableEntityError(
                    nameof(nodeParentID),
                    message);

                throw new UnprocessableEntityException(message);
            }



            // The page display name shown in the admin UI
            string pageDisplayName = request.MetadataTitle ?? "New Career Created by API";

            var itemData = await CreateCMSTreeItem(request, currentCulture.Locale, cancellationToken);

            // Creates a page metadata object
            var contentItemParameters = new ContentItemParameters(CAREER_CONTENT_TYPE, itemData);
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
            if (!await _webPageManager.TryPublish(newPageId, currentCulture.Locale, cancellationToken))
            {
                throw new NotFoundException("Failed to publish updated page");
            }

            // Retrieve the created page and return it
            var _retrieveNewPage = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 1,
                ChannelName = channel.ChannelName,
                LanguageName = currentCulture.Locale,
                IsForPreview = false
            };

            var createdResult = await _contentRetriever.GetPagesAsync<Career>(_retrieveNewPage,
                query => query.Where(w => w.WhereEquals("WebPageItemID", newPageId)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var createdCareer = createdResult?.FirstOrDefault();
            return createdCareer != null ? MapCareerToPageOutput(createdCareer) : new BadRequestResult();
        }

        public Task<ActionResult<PageOutput<CareerPageContent>>> CreateCareerPageCultureVersionAsync(int channelId, Guid pageGUID, string? fromCulture, string? toCulture, CareerPageContent request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCareerPageAsync(int channelId, Guid pageGUID, string? culture, bool deleteAllCultures, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        public async Task<ActionResult<PageOutput<CareerPageContent>>> GetCareerPageAsync(int channelId, Guid pageGUID, string? culture, CancellationToken cancellationToken)
        {
            var currentCulture = await _cultureValidationService.ValidateAndNormalizeSiteCultureAsync(channelId, 
                                                     culture ?? defaultCulture.Language, cancellationToken) ?? defaultCulture;

            if (string.IsNullOrEmpty(currentCulture.Locale))
            {
                throw new NotFoundException("Invalid culture specified");
            }

            var channel = await _channelInfoProvider.GetAsync(channelId, cancellationToken);
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

            var result = await _contentRetriever.GetPagesAsync<Career>(_retrieveCurrentPage,
                query =>
                {
                    query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID));
                },
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var careerPage = result?.FirstOrDefault();

            if (careerPage is null)
            {
                throw new NotFoundException();
            }

            return new ActionResult<PageOutput<CareerPageContent>>(MapCareerToPageOutput(careerPage));
        }

        public async Task<ActionResult<IEnumerable<PageOutput<CareerPageContent>>>> GetCareerPagesAsync(int channelId, string? culture, bool? onlyPublished, CancellationToken cancellationToken)
        {
            var currentCulture = await _cultureValidationService.ValidateAndNormalizeSiteCultureAsync(channelId,
                                                     culture ?? defaultCulture.Language, cancellationToken) ?? defaultCulture;

            if (string.IsNullOrEmpty(currentCulture.Locale))
            {
                throw new NotFoundException("Invalid culture specified");
            }

            var channel = await _channelInfoProvider.GetAsync(channelId, cancellationToken);
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

            var results = await _contentRetriever.GetPagesAsync<Career>(
            retrieveParameters,
            query => { },
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

            if(results == null || !results.Any())
            {
                throw new NotFoundException("No career pages found");
            }
            var careerList = results.ToList();
            return careerList.Select(MapCareerToPageOutput).ToList();
        }

        public Task<ActionResult<PageOutput<CareerPageContent>>> UpdateCareerPageAsync(int channelId, Guid pageGUID, string? culture, CareerPageContent request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private static PageOutput<CareerPageContent> MapCareerToPageOutput(Career career)
        {
            var language = ContentLanguageInfo.Provider.Get()
            .FirstOrDefault(l => l.ContentLanguageID == career.SystemFields.WebPageUrlPathContentLanguageID);


            return new PageOutput<CareerPageContent>
            {
                General = new PageGeneralOutput
                {
                    Name = career.SystemFields?.WebPageItemName ?? string.Empty,
                    Culture = language?.ContentLanguageName ?? "en-gb",
                    NodeParentID = career.SystemFields?.WebPageItemParentID ?? 0,
                    NodeID = career.SystemFields?.WebPageItemParentID ?? 0,
                    PageID = career.SystemFields?.WebPageItemID ?? 0,
                    NodeGUID = career.SystemFields?.ContentItemGUID ?? Guid.Empty,
                    PageGUID = career.SystemFields?.WebPageItemGUID ?? Guid.Empty,
                    NodeAliasPath = career.SystemFields?.WebPageUrlPath ?? string.Empty
                },
                Content = new CareerPageContent
                {
                    MetadataTitle = career.MetadataTitle ?? string.Empty,
                    MetadataDescription = career.MetadataDescription,
                    MetadataTeaserImage = career.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                    MetadataTeaserImageAltText = career.MetadataTeaserImageAltText,
                    MetadataCanonical = career.MetadataCanonical,
                    HeroLayout = career.HeroLayout ?? string.Empty,
                    HeroTitle = career.HeroTitle,
                    HeroDescription = career.HeroDescription,
                    HeroBackgroundImage = career.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                    HeroBackgroundImageAltText = career.HeroBackgroundImageAltText,
                    HeroBackgroundImage_Mobile = career.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                    HeroBackgroundImage_MobileAltText = career.HeroBackgroundImage_MobileAltText,
                    HeroCallToActionButtonType = career.HeroCallToActionButtonType,
                    HeroCallToActionButtonText = career.HeroCallToActionButtonText,
                    HeroCallToActionButtonUrl = career.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath,
                    ShowSidebar = career.ShowSidebar,
                    ShowPrintButton = career.ShowPrintButton,
                    DownloadButtonAttachment = career.DownloadButtonAttachment?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                    ShowCallToAction = career.ShowCallToAction,
                    CallToActionDescription = career.CallToActionDescription,
                    CallToActionButtonText = career.CallToActionButtonText,
                    CallToActionButtonUrl = career.CallToActionButtonUrl,
                    ShowSocialMediaButtons = career.ShowSocialMediaButtons,              
                    MetadataOGTitle = career.MetadataOGTitle,
                    MetadataOGDescription = career.MetadataOGDescription,
                    MetadataOGImage = career.MetadataOGImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                    MetadataOGTwitterImageSizeCheckbox = career.MetadataOGTwitterImageSizeCheckbox,
                    MetadataOGTwitterImage = career.MetadataOGTwitterImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                    MetadataNofollow = career.MetadataNofollow,
                    MetadataNoindex = career.MetadataNoindex,
                    IncludeInSitemap = career.IncludeInSitemap,
                    CareerContactPeopleID = career.CareerContactPeopleID?.FirstOrDefault()?.SystemFields?.WebPageItemID ?? 0,
                    MetadataRelevantContactPerson = career.MetadataRelevantContactPerson?.Select(p => p.SystemFields?.WebPageItemID ?? 0).ToArray() ?? Array.Empty<int>(),
                    MetadataOfficeLocation = career.MetadataOfficeLocation?.Select(o => o.SystemFields?.WebPageItemID ?? 0).ToArray() ?? Array.Empty<int>(),
                    CareerReference = career.CareerReference,
                    CareerJobTitle = career.CareerJobTitle?.FirstOrDefault()?.Name ?? string.Empty,
                    CareerLevel = career.CareerLevel?.FirstOrDefault()?.Name ?? string.Empty,
                    CareerEmploymentType = career.CareerEmploymentType?.FirstOrDefault()?.Name ?? string.Empty,
                    CareerApplyForm = career.CareerApplyForm,
                    PublishTo = career.SystemFields.ContentItemCommonDataFirstPublishedWhen,
                    PublishFrom = career.SystemFields.ContentItemCommonDataLastPublishedWhen,                                   

                },
                Metadata = new PageMetadataOutput
                {
                    PageTitle = new InheritableString { Value = career.MetadataTitle },
                    PageDescription = new InheritableString { Value = career.MetadataDescription },
                    PageKeywords = new InheritableString { Value = string.Empty },
                },
                Navigation = new PageNavigationOutput
                {
                    ShowInMenu = false,
                    ShowInSitemap = career.IncludeInSitemap,
                },
            };

        }

        private async Task<ContentItemData> CreateCMSTreeItem(CareerPageContent careerContent, string? culture, CancellationToken cancellationToken)
        {
           
            var downloadButtonReferences = await _mappingHelper.BuildContentAttachmentImageReferences(careerContent.DownloadButtonAttachment, cancellationToken);
            var ogImageReferences = await _mappingHelper.BuildContentAttachmentImageReferences(careerContent.MetadataOGImage, cancellationToken);
            var ogTwitterImageReferences = await _mappingHelper.BuildContentAttachmentImageReferences(careerContent .MetadataOGTwitterImage, cancellationToken);
            var teaserImageReferences = await _mappingHelper.BuildContentAttachmentImageReferences(careerContent.MetadataTeaserImage, cancellationToken);
            var heroBackgroundImageReferences = await _mappingHelper.BuildContentAttachmentImageReferences(careerContent.HeroBackgroundImage, cancellationToken);
            var heroBackgroundImageMobileReferences = await _mappingHelper.BuildContentAttachmentImageReferences(careerContent.HeroBackgroundImage_Mobile, cancellationToken);

            //var relevantContactPersonReferences = await _mappingHelper.BuildWebPageIdReferencesAsync<Person>(careerContent.CareerReference, CHANNEL_NAME, culture, cancellationToken);
            var heroCallToActionButtonUrlReferences = await _mappingHelper.BuildContentAttachmentPersonPageReferences(careerContent.HeroCallToActionButtonUrl, CHANNEL_NAME, culture, cancellationToken);
            var careerContactPeopleReferences = await _mappingHelper.BuildWebPageSingleIdReferencesAsync<Person>(careerContent.CareerContactPeopleID, CHANNEL_NAME, culture, cancellationToken);
            var metadataRelevantContactPersonReferences = await _mappingHelper.BuildWebPageIdReferencesAsync<Person>(careerContent.MetadataRelevantContactPerson, CHANNEL_NAME, culture, cancellationToken);
            var metadataOfficeLocationReferences = await _mappingHelper.BuildWebPageIdReferencesAsync<LocationOffice>(careerContent.MetadataOfficeLocation, CHANNEL_NAME, culture, cancellationToken);

            var careerJobTitleReferences = await _mappingHelper.BuildReusableContentItemNameReferenceAsync<CareerJobTitle>(careerContent.CareerJobTitle, cancellationToken);
            var careerLevelReferences = await _mappingHelper.BuildReusableContentItemNameReferenceAsync<CareerLevels>(careerContent.CareerLevel, cancellationToken);
            var careerEmploymentTypeReferences = await _mappingHelper.BuildReusableContentItemNameReferenceAsync<CareerEmploymentTypes>(careerContent.CareerEmploymentType, cancellationToken);

            return new ContentItemData(new Dictionary<string, object>
        {
           
            
            { nameof(Career.ShowCallToAction), careerContent.ShowCallToAction },
            { nameof(Career.CallToActionDescription), careerContent.CallToActionDescription ?? string.Empty },
            { nameof(Career.CallToActionButtonText), careerContent.CallToActionButtonText ?? string.Empty },
            { nameof(Career.CallToActionButtonUrl), careerContent.CallToActionButtonUrl ?? string.Empty },
            { nameof(Career.ShowSidebar), careerContent.ShowSidebar },
            { nameof(Career.ShowPrintButton), careerContent.ShowPrintButton },
            { nameof(Career.DownloadButtonAttachment), downloadButtonReferences },
            { nameof(Career.ShowSocialMediaButtons), careerContent.ShowSocialMediaButtons },
            { nameof(Career.MetadataOGTitle), careerContent.MetadataOGTitle ?? string.Empty },
            { nameof(Career.MetadataOGDescription), careerContent.MetadataOGDescription ?? string.Empty },
            { nameof(Career.MetadataOGImage), ogImageReferences },
            { nameof(Career.MetadataOGTwitterImageSizeCheckbox), careerContent.MetadataOGTwitterImageSizeCheckbox },
            { nameof(Career.MetadataOGTwitterImage), ogTwitterImageReferences },
            { nameof(Career.MetadataNofollow), careerContent.MetadataNofollow },
            { nameof(Career.MetadataNoindex), careerContent.MetadataNoindex },
            { nameof(Career.IncludeInSitemap), careerContent.IncludeInSitemap },
            { nameof(Career.MetadataTitle), careerContent.MetadataTitle ?? string.Empty },
            { nameof(Career.MetadataDescription), careerContent.MetadataDescription ?? string.Empty },
            { nameof(Career.MetadataTeaserImage), teaserImageReferences },
            { nameof(Career.MetadataTeaserImageAltText), careerContent.MetadataTeaserImageAltText ?? string.Empty },
            { nameof(Career.MetadataCanonical), careerContent.MetadataCanonical ?? string.Empty },
            { nameof(Career.HeroLayout), careerContent.HeroLayout ?? string.Empty },
            { nameof(Career.HeroTitle), careerContent.HeroTitle ?? string.Empty },
            { nameof(Career.HeroDescription), careerContent.HeroDescription ?? string.Empty },
            { nameof(Career.HeroBackgroundImage), heroBackgroundImageReferences },
            { nameof(Career.HeroBackgroundImageAltText), careerContent.HeroBackgroundImageAltText ?? string.Empty },
            { nameof(Career.HeroBackgroundImage_Mobile), heroBackgroundImageMobileReferences },
            { nameof(Career.HeroBackgroundImage_MobileAltText), careerContent.HeroBackgroundImage_MobileAltText ?? string.Empty },
            { nameof(Career.HeroCallToActionButtonType), careerContent.HeroCallToActionButtonType ?? string.Empty },
            { nameof(Career.HeroCallToActionButtonText), careerContent.HeroCallToActionButtonText ?? string.Empty },
            { nameof(Career.HeroCallToActionButtonUrl), heroCallToActionButtonUrlReferences },
            { nameof(Career.MetadataRelevantContactPerson), metadataRelevantContactPersonReferences },
            { nameof(Career.CareerContactPeopleID), careerContactPeopleReferences },
            { nameof(Career.MetadataOfficeLocation), metadataOfficeLocationReferences },
            { nameof(Career.CareerReference), careerContent.CareerReference ?? string.Empty },
            { nameof(Career.CareerJobTitle), careerJobTitleReferences },
            { nameof(Career.CareerLevel), careerLevelReferences },
            { nameof(Career.CareerEmploymentType), careerEmploymentTypeReferences },
            { nameof(Career.CareerApplyForm), careerContent.CareerApplyForm ?? string.Empty },
            { nameof(Career.PageTitle), careerContent.MetadataTitle ?? string.Empty },
            { nameof(Career.PageDescription), careerContent.MetadataDescription ?? string.Empty },
            { nameof(Career.PageKeywords), string.Empty },
        });
        }

      
    }
}
