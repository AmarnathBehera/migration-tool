using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Web.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs
{
    public class ServicesAreaAndServicePageDto
    {
        public MetaDataDto? MetaData { get; set; }
        public HeroDto Hero { get; set; } = null!;
        
        public bool ShowSideNavigation { get; set; }

        public string Title { get; set; } = null!;

        public SideBoxDto? Sidebox { get; set; }
    }
}
