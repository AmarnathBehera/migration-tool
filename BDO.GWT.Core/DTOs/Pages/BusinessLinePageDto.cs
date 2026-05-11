using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Web.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs
{
    public class BusinessLinePageDto
    {

        public MetaDataDto? MetaData { get; set; }
        public HeroDto Hero { get; set; } = null!;

        public bool ShowSideNavigation { get; set; }

        public string TileIcon { get; set; } = null!;
        public bool ComingSoon { get; set; }

        public SideBoxDto? Sidebox { get; set; }
    }
}
