using BDO.GWT.Web.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.Data
{
    public class FlexibleBannerData: BaseWidgetData
    {
        public string Layout { get; set; } = null!;

        public string? Description { get; set; }

        public string? BackgroundImage { get; set; }

        public string? BackgroundImageAltText { get; set; }

        public string Theme { get; set; } = null!;

        public string ButtonSide { get; set; } = null!;

        public string TopMargin { get; set; } = null!;

        public string BottomMargin { get; set; } = null!;

        public ButtonDto PrimaryButton { get; set; } = null!;

        public ButtonDto? SecondaryButton { get; set; }
    }
}
