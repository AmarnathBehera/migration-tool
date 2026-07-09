using System;

namespace BDO.GWT.Core.DTOs.Pages
{
    public class PageConfigurationDto
    {
        public string? TypeName { get; set; }

        public DateTime? PublishDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool? ShowPrintButton { get; set; }

        public string? DownloadButtonAttachment { get; set; }

        public bool ShowCopyLinkButton { get; set; }

        public SocialMediaButtonsDto? SocialMediaButtons { get; set; }
    }
}
