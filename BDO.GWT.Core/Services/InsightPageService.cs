using AngleSharp.Dom;
using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Core.DTOs.SchemaMarkups;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs.Pages;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services
{
    public interface IInsightPageService
    { 
        Task<InsightPageDto> GetInsightPageDtoAsync(Insight? page, CancellationToken cancellationToken);
    }
    public class InsightPageService : IInsightPageService
    {
        private readonly IWebsiteChannelContext _websiteChannelContext;
        private readonly ISettingService _settingsService;
        private readonly ISocialMediaButtonsService _socialMediaButtonsService;
        private readonly IContentService _contentService;
        private readonly IContentRetriever _contentRetriever;
        private readonly IInfoProvider<ChannelInfo> _channelInfoProvider;
        private readonly IInfoProvider<WebsiteChannelInfo> _websiteChannelInfoProvider;

        public InsightPageService(IContentRetriever contentRetriever, IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider,
            IContentService contentService, IInfoProvider<ChannelInfo> channelInfoProvider,
            IWebsiteChannelContext websiteChannelContext, ISettingService settingsService, ISocialMediaButtonsService socialMediaButtonsService
            )
        {
            _contentRetriever = contentRetriever;
            _websiteChannelInfoProvider = websiteChannelInfoProvider;
            _websiteChannelContext = websiteChannelContext;
            _contentService = contentService;
            _settingsService = settingsService;
            _socialMediaButtonsService = socialMediaButtonsService;
            _channelInfoProvider = channelInfoProvider;
            
        }
        public async Task<InsightPageDto> GetInsightPageDtoAsync(Insight? page, CancellationToken cancellationToken)
        {
            var insightPage = await _contentRetriever.RetrieveCurrentPage<Insight>(new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);
            var contentPageDto = await GetContentPageDtoAsync(insightPage, cancellationToken);
            var sys = insightPage.SystemFields;

            var dto = new InsightPageDto(contentPageDto)
            {
                SchemaMarkup = await GetInsightSchemaMarkupDtoAsync(insightPage, cancellationToken),
                InsightSource = await GetInsightSourceDtoAsync(insightPage, cancellationToken),
                BaseContent = new BaseContent
                {
                    ContentItemID = sys?.ContentItemID ?? 0,
                    ContentItemGUID = sys?.ContentItemGUID ?? Guid.Empty,
                    ContentItemName = sys?.ContentItemName,
                    ContentItemContentTypeID = sys?.ContentItemContentTypeID ?? 0,
                    ContentItemIsSecured = sys?.ContentItemIsSecured ?? false,
                    ContentItemCommonDataContentLanguageID = sys?.ContentItemCommonDataContentLanguageID ?? 0,
                    ContentItemCommonDataFirstPublishedWhen = sys?.ContentItemCommonDataFirstPublishedWhen ?? default,
                    ContentItemCommonDataLastPublishedWhen = sys?.ContentItemCommonDataLastPublishedWhen ?? default
                },

                BaseWebPage = new BaseWebPage
                {
                    WebPageItemGUID = sys?.WebPageItemGUID ?? Guid.Empty,
                    WebPageItemID = sys?.WebPageItemID ?? 0,
                    WebPageItemName = sys?.WebPageItemName,
                    WebPageItemWebsiteChannelID = sys?.WebPageItemWebsiteChannelId ?? 0,
                    WebPageUrlPath = sys?.WebPageUrlPath,
                    WebPageItemOrder = sys?.WebPageItemOrder ?? 0,
                    WebPageItemTreePath = sys?.WebPageItemTreePath,
                    WebPageUrlPathContentLanguageID = sys?.WebPageUrlPathContentLanguageID ?? 0,
                    WebPageItemParentID = sys?.WebPageItemParentID ?? 0
                },
                MetaData = new MetaDataDto
                {
                    MetaTitle = insightPage.MetadataTitle,
                    MetaDescription = insightPage.MetadataDescription,
                    MetadataTeaserImage = insightPage.MetadataTeaserImage.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                    MetaTeaserImageAltText = insightPage.MetadataTeaserImageAltText,
                    MetaCanonical = insightPage.MetadataCanonical,
                },
                SearchAndSEO = new SearchAndSEODto
                {
                    MetadataNofollow = insightPage.MetadataNofollow,
                    MetadataNoindex = insightPage.MetadataNoindex,
                    IncludeInSitemap = insightPage.IncludeInSitemap,
                },
                OGMetaData = new OGMetaDataDto
                {
                    MetadataOGTitle = insightPage.MetadataOGTitle,
                    MetadataOGDescription = insightPage.MetadataOGDescription,
                    MetadataOGImage = insightPage.MetadataOGImage.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                    MetadataOGTwitterImage = insightPage.MetadataOGTwitterImage.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                    MetadataOGTwitterImageSizeCheckbox = insightPage.MetadataOGTwitterImageSizeCheckbox,
                    Url = insightPage.MetadataCanonical,
                }
            };

            if (!insightPage.ShowSidebar)
            {
                var contentPageSidebarDto = await GetContentPageSidebarDto(insightPage, cancellationToken);
                var showCopyLinkButton = await GetShowCopyLinkOptionFromSectionPageAsync(insightPage, contentPageSidebarDto.ShowCopyLinkButton, cancellationToken);
                dto.IsLayoutNarrow = insightPage.UseNarrowLayout;
                dto.PageConfiguration = GetPageConfigurationDto(contentPageSidebarDto, showCopyLinkButton);
                dto.Authors = GetPageAuthorsDto(contentPageSidebarDto);
                dto.ShowPeopleInline = insightPage.ShowAuthorsAtTheTop;
            }

            if (dto.Sidebar is not null)
            {
                var showCopyLinkButton = await GetShowCopyLinkOptionFromSectionPageAsync(insightPage, dto.Sidebar.ShowCopyLinkButton, cancellationToken);
                dto.Sidebar.ShowCopyLinkButton = showCopyLinkButton;
            }

            return dto;
        }

        private static PageConfigurationDto GetPageConfigurationDto(ContentSidebarDto contentPageSidebarDto, bool showCopyLinkButton)
        {
            var pageConfigurationDto = new PageConfigurationDto
            {
                TypeName = contentPageSidebarDto.TypeName,
                PublishDate = contentPageSidebarDto.PublishDate,
                UpdatedDate = contentPageSidebarDto.UpdatedDate,
                ShowPrintButton = contentPageSidebarDto.ShowPrintButton,
                DownloadButtonAttachment = contentPageSidebarDto.DownloadButtonAttachment,
                ShowCopyLinkButton = showCopyLinkButton,
                SocialMediaButtons = contentPageSidebarDto.SocialMediaButtons,
            };

            return pageConfigurationDto;
        }
        private static PeopleGridDto GetPageAuthorsDto(ContentSidebarDto contentPageSidebarDto)
        {
            var peopleGridDto = new PeopleGridDto
            {
                Title = contentPageSidebarDto.RelevantContactPeopleTitle,
                Items = contentPageSidebarDto.RelevantContactPeople.Select(x => new PersonCardDto
                {
                    DisplayName = x.FullName,
                    TeaserImageUrl = x.Image?.Url,
                    TeaserImageAltText = x.Image?.AltText,
                    JobTitle = x.JobTitle,
                    PersonUrl = x.Url,
                }),
                ExternalItems = contentPageSidebarDto.RelatedExternalPeople.Select(x => new PersonCardDto
                {
                    DisplayName = x.FullName,
                    TeaserImageUrl = x.Image?.Url,
                    TeaserImageAltText = x.Image?.AltText,
                    JobTitle = x.JobTitle,
                    PersonUrl = x.Url,
                }),
                UseNameAsLink = true,
            };

            return peopleGridDto;
        }


        public async Task<ContentPageDto> GetContentPageDtoAsync(Insight contentPage, CancellationToken cancellationToken)
        {
            var copyInsight = contentPage.CopyInsight?.FirstOrDefault();
            var dto = new ContentPageDto
            {
                Title = copyInsight?.ShowInsightTitle == true ? copyInsight.Title : null,
                Description = copyInsight?.ShowInsightDescription == true ? HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(copyInsight.Description) : null,
                Content = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(copyInsight?.InsightContent),
                Sidebar = contentPage.ShowSidebar ? await GetContentPageSidebarDto(contentPage, cancellationToken) : null,
                CallToAction = copyInsight?.ShowCallToAction == true ? GetCallToActionDto(contentPage) : null,
            };

            return dto;
        }
        private static CallToActionDto? GetCallToActionDto(Insight contentPage)
        {
            var copyInsight = contentPage.CopyInsight?.FirstOrDefault();

            return PageHelper.GetCallToActionDto(
                copyInsight?.CallToActionDescription,
                copyInsight?.CallToActionButtonText,
                copyInsight?.CallToActionButtonUrl);
        }
        public async Task<ContentSidebarDto> GetSidebarDtoAsync(Insight page, CancellationToken cancellationToken)
        {
            var settings = await _settingsService.GetPrintorShareInfo();
            var settingInfo = settings?.FirstOrDefault();


            var sidebarDto = new ContentSidebarDto
            {

                ShowPrintButton = settingInfo?.DisablePrint is not true && page.ShowPrintButton,

                DownloadButtonAttachment = settingInfo?.DisableDownload == true ? null : page.DownloadButtonAttachment?
                    .FirstOrDefault()?
                    .ContentAttachmentAsset
                    .Url,
                SocialMediaButtons = page.ShowSocialMediaButtons ? await _socialMediaButtonsService.GetSocialMediaButtonsDto(page, page.PageTitle, cancellationToken) : null,
                ShowCopyLinkButton = settingInfo?.DisableCopyLinkButton == true
            };

            if (sidebarDto.SocialMediaButtons is not null)
            {
                sidebarDto.SocialMediaButtons.ShowCopyLinkButton = null;
            }

            return sidebarDto;
        }
        public async Task<SidebarExternalPersonDto[]> GetSidebarExternalPeopleDtosAsync(IEnumerable<ExternalPerson> externalPeopleNodeGuids, CancellationToken cancellationToken)
        {
            var guids = externalPeopleNodeGuids.Select(x => x.SystemFields.ContentItemGUID).ToList();
            
            var externalPeople = await _contentService.GetPagesAsync<ExternalPerson>(
                new RetrievePagesParameters { },
                query => query.Where(w => w.WhereIn("ContentItemGUID", guids)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken
            );
            
            var dtos = await Task.WhenAll(externalPeople.Select(async externalPerson => new SidebarExternalPersonDto
            {
                Image = PageHelper.GetImageDto(externalPerson.ImageFile.FirstOrDefault()?.ContentAttachmentAsset?.Url, externalPerson.ImageFileAltText),
                FullName = await GetDisplayNameAsync(externalPerson),
                JobTitle = externalPerson.JobTitle,
                Url = externalPerson.Url,
            }));

            return dtos;
        }

        public async Task<ContentSidebarDto> GetContentPageSidebarDto(Insight contentPage, CancellationToken cancellationToken)
        {
            var sidebarDto = await GetSidebarDtoAsync(contentPage, cancellationToken);
            sidebarDto.ShowCopyLinkButton = await GetShowCopyLinkOptionFromSectionPageAsync(contentPage, sidebarDto.ShowCopyLinkButton, cancellationToken);
            sidebarDto.TypeName = contentPage.ContentType.FirstOrDefault()?.Name;
            sidebarDto.PublishDate = contentPage.SystemFields.ContentItemCommonDataFirstPublishedWhen == DateTime.MinValue ? null : contentPage.SystemFields.ContentItemCommonDataFirstPublishedWhen;
            sidebarDto.UpdatedDate = contentPage.SystemFields.ContentItemCommonDataLastPublishedWhen == DateTime.MinValue ? null : contentPage.SystemFields.ContentItemCommonDataLastPublishedWhen;
            sidebarDto.RelevantContactPeopleTitle = ResourceStringKey.LiveSite.Insights.Authors;
            
            sidebarDto.RelevantContactPeople = await GetContentPageAuthorDtos(contentPage,cancellationToken);

            sidebarDto.RelatedExternalPeople = await GetSidebarExternalPeopleDtosAsync(contentPage.RelatedExternalPeople, cancellationToken);

            return sidebarDto;
        }
        private async Task<SidebarPersonDto[]> GetContentPageAuthorDtos(Insight contentPage,CancellationToken cancellationToken)
        {
           
            var people = contentPage.RelevantContactPerson?.OfType<Person>()
                .DistinctBy(x => x.SystemFields.ContentItemID)
                .ToArray() ?? Array.Empty<Person>();

            var internalPeople = contentPage.RelevantContactPerson?.OfType<InternalPerson>()
                .DistinctBy(x => x.SystemFields.ContentItemID)
                .ToArray() ?? Array.Empty<InternalPerson>();

            var internalPeopleAsPerson = internalPeople.Select(internalPerson => new Person
            {
                PersonFirstName = internalPerson.FirstName,
                PersonLastName = internalPerson.LastName,
                PersonSurnamePrefixes = internalPerson.SurnamePrefixes,
                PersonJobTitle = internalPerson.JobTitle,
                MetadataTeaserImage = internalPerson.MetadataTeaserImage,
                MetadataTeaserImageAltText = internalPerson.MetadataTeaserImageAltText,
                SystemFields = internalPerson.SystemFields, 
            });

            var unorderedPeople = people.Concat(internalPeopleAsPerson).ToArray();

            var dtos = await Task.WhenAll(unorderedPeople.Select(async person =>
            {
                var setting = await _settingsService.GetUsePersonPageTitleInfo();
                var usePersonPageTitle = setting?.FirstOrDefault()?.UsePageTitleToDisplayPersonName ?? false;

                string fullName;
                if (usePersonPageTitle)
                {
                    fullName = person.MetadataTitle;
                }
                else
                {
                    fullName = string.IsNullOrEmpty(person.PersonSurnamePrefixes)
                        ? $"{person.PersonFirstName} {person.PersonLastName}"
                        : $"{person.PersonFirstName} {person.PersonSurnamePrefixes} {person.PersonLastName}";
                }

                return new SidebarPersonDto
                {
                    Image = PageHelper.GetImageDto(person.MetadataTeaserImage.FirstOrDefault()?.ContentAttachmentAsset.Url, person.MetadataTeaserImageAltText),
                    FullName = fullName,
                    JobTitle = person.PersonJobTitle,
                    Url = person.SystemFields.WebPageUrlPath,
                };
            }));

            return dtos;
        }
       
        private async Task<bool> GetShowCopyLinkOptionFromSectionPageAsync(Insight insightPage, bool showCopyLinkButton, CancellationToken cancellationToken)
        {
            var insights = await _contentService.GetPagesAsync<Insight>(new RetrievePagesParameters { }, query => query.Where(w => w.WhereEquals("WebPageItemID", insightPage?.SystemFields.WebPageItemID)), RetrievalCacheSettings.CacheDisabled, cancellationToken);

            if (insights is null)
            {
                throw new MissingSectionPageException($"'No pages have been found of type '{nameof(Insight)}'.");
            }
            var insightpage = insights.FirstOrDefault();

            var sectioninsight = await _contentService.GetPagesAsync<SectionInsightsPage>(new RetrievePagesParameters { }, query => query.Where(w => w.WhereEquals("WebPageItemID", insightpage?.SystemFields.WebPageItemParentID)), RetrievalCacheSettings.CacheDisabled, cancellationToken);

            var sectioninsights = sectioninsight.FirstOrDefault();

            if (sectioninsights is null)
            {
                throw new MissingSectionPageException($"'No pages have been found of type '{nameof(SectionCareers)}'.");
            }


            return sectioninsights.ShowCopyUrlButtonInInsightPage && showCopyLinkButton;
        }
        private async Task<ArticleSchemaMarkupDto> GetInsightSchemaMarkupDtoAsync(Insight insights, CancellationToken cancellationToken)
        {
            var site = _websiteChannelContext.WebsiteChannelID;
            var websiteChannel = await _websiteChannelInfoProvider.GetAsync(_websiteChannelContext.WebsiteChannelID, cancellationToken);
            var presentationUrl = websiteChannel?.WebsiteChannelDomain;
            var authorNames = new List<string>();
            if (insights.RelevantContactPerson.Any())
            {
                var relevantContactIds = insights.RelevantContactPerson
                    .Select(p => ((IContentItemFieldsSource)p).SystemFields.ContentItemID)
                    .ToList();
                var contactNames = await GetKeyContactsDisplayNames(relevantContactIds, cancellationToken);
                authorNames.AddRange(contactNames);
            }

            if (insights.RelatedExternalPeople.Any())
            {
                var guids = insights.RelatedExternalPeople
               .Select(x => x.SystemFields.ContentItemGUID).ToList();
                var extPeople = await GetExternalPeopleDisplayNames(guids, cancellationToken);
                authorNames.AddRange(extPeople);
            }

            var name = _websiteChannelContext.WebsiteChannelName;

            if (authorNames.Count == 0)
            {
                authorNames.Add(name);
            }

            return new ArticleSchemaMarkupDto
            {
                Author = authorNames.Select(a => new PersonSchemaMarkupDto { Name = a }).ToList(),
                DatePublished = insights.SystemFields.ContentItemCommonDataFirstPublishedWhen,
                DateModified = insights.SystemFields.ContentItemCommonDataLastPublishedWhen,
                Description = HtmlHelper.ExtractTextFromHtml(insights.CopyInsight.FirstOrDefault()?.Description),
                MainEntityOfPage = presentationUrl,
                Headline = insights.CopyInsight.FirstOrDefault()?.Title,
                Image =  insights.CopyInsight.FirstOrDefault()?.ImageFile.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                Publisher = new OrganizationSchemaMarkupDto
                {
                    Name = name,
                    Logo =  _contentService.GetCurrentSiteLogoImageUrlAsync(),
                },
                Url = insights.SystemFields.WebPageUrlPath
            };
        }

        public async Task<List<string>> GetKeyContactsDisplayNames(ICollection<int> relevantContactPerson, CancellationToken cancellationToken)
        {
             var people = (await _contentService.GetPagesAsync<Person>(
               new RetrievePagesParameters { LinkedItemsMaxLevel = 4 },
               query => query
                   .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
                   .Where(w => w.WhereIn("ContentItemID", relevantContactPerson)),
               RetrievalCacheSettings.CacheDisabled,
               cancellationToken
           ));

            var internalPeople = (await _contentService.GetPagesAsync<InternalPerson>(
               new RetrievePagesParameters { LinkedItemsMaxLevel = 4 },
               query => query
                   .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
                   .Where(w => w.WhereIn("ContentItemID", relevantContactPerson)),
               RetrievalCacheSettings.CacheDisabled,
               cancellationToken
           ));

            var internalPeopleAsPerson = internalPeople.Select(internalPerson => new Person
            { 
                PersonFirstName = internalPerson.FirstName,
                PersonLastName = internalPerson.LastName,
                PersonSurnamePrefixes = internalPerson.SurnamePrefixes,
                PersonJobTitle = internalPerson.JobTitle,
                MetadataTeaserImage = internalPerson.MetadataTeaserImage,
                MetadataTeaserImageAltText = internalPerson.MetadataTeaserImageAltText,
            });
            var unorderedPeople = people.Concat(internalPeopleAsPerson).ToArray();
            var setting = await _settingsService.GetUsePersonPageTitleInfo();
            var usePersonPageTitle = setting?.FirstOrDefault()?.UsePageTitleToDisplayPersonName ?? false;
            if (usePersonPageTitle)
            {
                return unorderedPeople.Select(person => person.MetadataTitle).ToList();
            }
            else
            {
                return unorderedPeople
                    .Select(person => string.IsNullOrWhiteSpace(person.PersonSurnamePrefixes)
                        ? $"{person.PersonFirstName} {person.PersonLastName}"
                        : $"{person.PersonFirstName} {person.PersonSurnamePrefixes} {person.PersonLastName}")
                    .ToList();
            }

        }
       
        public async Task<List<string>> GetExternalPeopleDisplayNames(List<Guid> externalPeopleNodeGuids, CancellationToken cancellationToken)
        {
            var externalPeople = (await _contentService.GetPagesAsync<ExternalPerson>(
               new RetrievePagesParameters { LinkedItemsMaxLevel = 2 },
               query => query
                   .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
                   .Where(w => w.WhereIn("ContentItemGUID", externalPeopleNodeGuids)),
               RetrievalCacheSettings.CacheDisabled,
               cancellationToken
           ));
            var names = await Task.WhenAll(externalPeople.Select(async externalPerson => await GetDisplayNameAsync(externalPerson)));

            return names.ToList();
        }
        public async Task<string> GetDisplayNameAsync(ExternalPerson externalPerson)
        {
            var setting = await _settingsService.GetUsePersonPageTitleInfo();

            var usePersonPageTitle = setting?.FirstOrDefault()?.UsePageTitleToDisplayPersonName ?? false;

            if (usePersonPageTitle)
            {
                return externalPerson.MetadataTitle;
            }

            return string.IsNullOrEmpty(externalPerson.SurnamePrefixes) ? $"{externalPerson.FirstName} {externalPerson.LastName}" : $"{externalPerson.FirstName} {externalPerson.SurnamePrefixes} {externalPerson.LastName}";
        }

        private async Task<InsightSourceDto?> GetInsightSourceDtoAsync(Insight insight, CancellationToken cancellationToken)
        {
            var channelName = _websiteChannelContext.WebsiteChannelName;

            var channels = await _channelInfoProvider
                .Get()
                .WhereEquals(nameof(ChannelInfo.ChannelName), channelName)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

            var currentChannel = channels.FirstOrDefault();
            if (currentChannel?.ChannelGUID == Guid.Empty || currentChannel is null)
            {
                return null;
            }

            return new InsightSourceDto
            {
                Link = insight.MetadataCanonical,
                FirmName = insight.CopyInsight.FirstOrDefault()?.SharingSourceFirm,
                CopyDate = insight.SystemFields.ContentItemCommonDataFirstPublishedWhen,
            };
        }

    }
}
