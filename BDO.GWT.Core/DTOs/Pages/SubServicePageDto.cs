using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Web.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs
{
    public class SubServicePageDto
    {

        public MetaDataDto? MetaData { get; set; }
        public HeroDto Hero { get; set; } = null!;
    }
}
