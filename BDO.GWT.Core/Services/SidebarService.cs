using BDO.GWT.Core.DTOs;

namespace BDO.GWT.Core.Services;
public interface ISidebarService
{
    Task<ContentSidebarDto> GetNewsSidebarDtoAsync(NewsArticle page, CancellationToken cancellationToken);
    Task<DealSidebarDto> GetDealSidebarDtoAsync(Deal page, CancellationToken cancellationToken);
}
public class SidebarService : ISidebarService
{
   private readonly ISocialMediaButtonsService _socialMediaButtonsService;
   private readonly ISettingService _settingService;
    public SidebarService(ISocialMediaButtonsService socialMediaButtonsService, ISettingService settingService)
    {
        _socialMediaButtonsService = socialMediaButtonsService;
        _settingService = settingService;
    }

    public async Task<ContentSidebarDto> GetNewsSidebarDtoAsync(NewsArticle page, CancellationToken cancellationToken)
    {
        var settings = await _settingService.GetPrintorShareInfo();
        var settingInfo = settings?.FirstOrDefault();

        var sidebarDto = new ContentSidebarDto
        {
            ShowPrintButton = (settingInfo?.DisablePrint is not true) && page.ShowPrintButton,
            DownloadButtonAttachment = settingInfo?.DisableDownload is true
                                      ? null
                                      : page.DownloadButtonAttachment?.FirstOrDefault()?.ContentAttachmentAsset.Url,
            SocialMediaButtons = page.ShowSocialMediaButtons ? await _socialMediaButtonsService.GetSocialMediaButtonsDto(page, page.PageTitle, cancellationToken) : null,
            ShowCopyLinkButton = settingInfo?.DisableCopyLinkButton is not true,
        };

        if (sidebarDto.SocialMediaButtons is not null)
        {
            sidebarDto.SocialMediaButtons.ShowCopyLinkButton = null;
        }

        return sidebarDto;
    }

    public async Task<DealSidebarDto> GetDealSidebarDtoAsync(Deal page, CancellationToken cancellationToken)
    {
        var settings = await _settingService.GetPrintorShareInfo();
        var settingInfo = settings?.FirstOrDefault();

        var sidebarDto = new DealSidebarDto
        {
            ShowPrintButton = (settingInfo?.DisablePrint is not true) && page.ShowPrintButton,
            DownloadButtonAttachment = settingInfo?.DisableDownload is true
                                      ? null
                                      : page.DownloadButtonAttachment?.FirstOrDefault()?.ContentAttachmentAsset.Url,
            SocialMediaButtons = page.ShowSocialMediaButtons ? await _socialMediaButtonsService.GetSocialMediaButtonsDto(page, page.PageTitle, cancellationToken) : null,
            ShowCopyLinkButton = settingInfo?.DisableCopyLinkButton is not true,
            Date = page.DateField,
            CallToAction = page.ShowCallToAction ? new CallToActionDto
            {
                Description = page.CallToActionDescription,
                ButtonTitle = page.CallToActionButtonText,
                ButtonUrl = page.CallToActionButtonUrl,
            } : null,
        };

        if (sidebarDto.SocialMediaButtons is not null)
        {
            sidebarDto.SocialMediaButtons.ShowCopyLinkButton = null;
        }

        return sidebarDto;
    }
}
