using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;

namespace BDO.GWT.Core.Services;

public interface ISidebarService
{
    Task<ContentSidebarDto> GetSidebarDtoAsync(NewsArticle page, CancellationToken cancellationToken);
}

public class SidebarService : ISidebarService
{
   private readonly ISocialMediaButtonsService _socialMediaButtonsService;
    public SidebarService(ISocialMediaButtonsService socialMediaButtonsService)
    {
        _socialMediaButtonsService = socialMediaButtonsService;
    }

    public async Task<ContentSidebarDto> GetSidebarDtoAsync(NewsArticle page, CancellationToken cancellationToken)
    {
        //var settingValueByKey = await _settingsService.GetCurrentSiteSettingsAsync([
        //    SettingName.BDO.DisablePrint,
        //    SettingName.BDO.DisableDownload,
        //    SettingName.BDO.DisableCopyLink,
        //], cancellationToken);

        var sidebarDto = new ContentSidebarDto
        {
            //ShowPrintButton = (!settingValueByKey.TryGetValue(SettingName.BDO.DisablePrint, out var disablePrint) || disablePrint is not true) && page.ShowPrintButton,
            DownloadButtonAttachment =// settingValueByKey.TryGetValue(SettingName.BDO.DisableDownload, out var disableDownload) && disableDownload is true
                //? null:
                 page.DownloadButtonAttachment?.FirstOrDefault()?.ContentAttachmentAsset.Url,
            SocialMediaButtons = page.ShowSocialMediaButtons ? await _socialMediaButtonsService.GetSocialMediaButtonsDto(page, cancellationToken) : null,
            //ShowCopyLinkButton =settingValueByKey.TryGetValue(SettingName.BDO.DisableCopyLink, out var disableCopyLink) && disableCopyLink is not true,
        };

        if (sidebarDto.SocialMediaButtons is not null)
        {
            sidebarDto.SocialMediaButtons.ShowCopyLinkButton = null;
        }

        return sidebarDto;
    }
}
