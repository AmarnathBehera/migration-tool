using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;

using CMS.ContentEngine;
using CMS.Websites;

using Kentico.Content.Web.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace BDO.GWT.Web.Core.Services;

public interface ICareerDetailPageService
    {
        Task<CareerPageDto> GetCareerPageDtoAsync( CancellationToken cancellationToken);
    }
    public class CareerDetailPageService : ICareerDetailPageService
    {
        private readonly IHeroService _heroService;

    private readonly IContentRetriever _contentRetriever;
    private readonly ICultureContext _cultureContext;
    private readonly ISocialMediaButtonsService _socialMediaButtonsService;
    private readonly ISettingService _settingsService;
    private readonly IContentService _contentService;

    public CareerDetailPageService(IHeroService heroService, IContentRetriever contentRetriever, ICultureContext cultureContext, ISocialMediaButtonsService socialMediaButtonsService, ISettingService settingsService, IContentService contentService)
    {
        _heroService = heroService;

        _contentRetriever = contentRetriever;
        _cultureContext = cultureContext;
        _socialMediaButtonsService = socialMediaButtonsService;
        _settingsService = settingsService;
        _contentService = contentService;   
    }


        public async Task<CareerPageDto> GetCareerPageDtoAsync(CancellationToken cancellationToken)
        {
        var page = await _contentRetriever.RetrieveCurrentPage<Career>(
                new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);


        if (page == null)
        {
            return new CareerPageDto();
        }

        var bgImageUrl = page.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        var mobileImageUrl = page.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        var ctaUrlPath = page.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath;

        var heroData = new HeroData
        {
            Layout = page.HeroLayout,
            Title = page.HeroTitle,
            Description = page.HeroDescription,
            BackgroundImage = bgImageUrl != null ? Link.Parse(bgImageUrl) : null,
            BackgroundImageAltText = page.HeroBackgroundImageAltText,
            MobileBackgroundImage = mobileImageUrl != null ? Link.Parse(mobileImageUrl) : null,
            MobileBackgroundImageAltText = page.HeroBackgroundImage_MobileAltText,
            CallToActionButtonType = page.HeroCallToActionButtonType,
            CallToActionButtonText = page.HeroCallToActionButtonText,
            CallToActionButtonUrl = ctaUrlPath != null ? Link.Parse(ctaUrlPath) : null,
        };



        var dto = new CareerPageDto
        {
            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
            Content = await GetContentPageDtoAsync(page),
            Form = page.CareerApplyForm,
            Sidebar = page.ShowSidebar ? await GetCareerSidebarDtoAsync(page, cancellationToken) : null,
        };
 

            return dto;
        }
    

    private async Task<CareerContentDto> GetContentPageDtoAsync(Career careerPage)
        {
       
        var locationOfficeNames = careerPage.MetadataOfficeLocation.Select(i=> i.MetadataTitle).ToArray();


        var contactPersonName = careerPage.CareerContactPeopleID.FirstOrDefault();

        var dto = new CareerContentDto
            {
                Title = careerPage.MetadataTitle,
            JobTitle = careerPage.CareerJobTitle.FirstOrDefault()?.Name ?? string.Empty,
            Reference = careerPage.CareerReference,
            EmploymentType = careerPage.CareerEmploymentType.FirstOrDefault()?.Name ?? string.Empty,
            Level = careerPage.CareerLevel.FirstOrDefault()?.Name ?? string.Empty,
            Locations = locationOfficeNames,
                Content = careerPage.MetadataDescription,
            ContactPerson = contactPersonName?.MetadataTitle,
        };

            return dto;
        }

    private async Task<ContentSidebarDto> GetCareerSidebarDtoAsync(Career careerPage, CancellationToken cancellationToken)
    {
        var sidebarDto = await GetSidebarDtoAsync(careerPage, cancellationToken);

        sidebarDto.ShowCopyLinkButton = await GetShowCopyLinkOptionFromSectionPageAsync(careerPage,sidebarDto.ShowCopyLinkButton,cancellationToken); 

        sidebarDto.PublishDate = careerPage.DocumentPublishFrom; 

        sidebarDto.RelevantContactPeopleTitle = ResourceStringKey.LiveSite.Careers.RelevantContactPeople;

        sidebarDto.RelevantContactPeople = careerPage.MetadataRelevantContactPerson.Select(i => new SidebarPersonDto
        {
            FullName = i.PersonFirstName + i.PersonLastName,
            Image = new ImageDto { Url = i.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset.Url, AltText = i.MetadataTeaserImageAltText },
            JobTitle = i.PersonJobTitle,
            Url = i.GetUrl().RelativePath



        }).ToArray();


        return sidebarDto;
    }

    // need to add from common sidebar service
    public async Task<ContentSidebarDto> GetSidebarDtoAsync(Career page, CancellationToken cancellationToken)
    {
        var settings = await _settingsService.GetPrintorShareInfo();
        var settingInfo = settings?.FirstOrDefault();


        var sidebarDto = new ContentSidebarDto
        {

            ShowPrintButton =
        settingInfo?.DisablePrint is not true
        && page.ShowPrintButton,

            DownloadButtonAttachment =
        settingInfo?.DisableDownload == true
            ? null
            : page.DownloadButtonAttachment?
                .FirstOrDefault()?
                .ContentAttachmentAsset
                .Url,
            SocialMediaButtons = page.ShowSocialMediaButtons ? await _socialMediaButtonsService.GetSocialMediaButtonsDto(page,page.PageTitle, cancellationToken) : null,
            ShowCopyLinkButton = settingInfo?.DisableCopyLinkButton == true
        };

        if (sidebarDto.SocialMediaButtons is not null)
        {
            sidebarDto.SocialMediaButtons.ShowCopyLinkButton = null; 
        }

        return sidebarDto;
    }

    private async Task<bool> GetShowCopyLinkOptionFromSectionPageAsync(Career careerPage, bool showCopyLinkButton, CancellationToken cancellationToken)
    {
        var careers = await _contentService.GetPagesAsync<Careers>(new RetrievePagesParameters { },query => query.Where(w => w.WhereEquals("WebPageItemID",careerPage.SystemFields.WebPageItemParentID)), RetrievalCacheSettings.CacheDisabled, cancellationToken);

        if (careers is null)
        {
            throw new MissingSectionPageException($"'No pages have been found of type '{nameof(Careers)}'.");
        }
        var careerspage = careers.FirstOrDefault();

        var sectionCareers = await _contentService.GetPagesAsync<SectionCareers>(new RetrievePagesParameters { }, query => query.Where(w => w.WhereEquals("WebPageItemID", careerspage.SystemFields.WebPageItemParentID)), RetrievalCacheSettings.CacheDisabled, cancellationToken);

        var sectionCareer = sectionCareers.FirstOrDefault();

        if (sectionCareer is null)
        {
            throw new MissingSectionPageException($"'No pages have been found of type '{nameof(SectionCareers)}'.");
        }
        

        return sectionCareer.ShowCopyUrlButtonInJobOpportunity && showCopyLinkButton;
    }


}
