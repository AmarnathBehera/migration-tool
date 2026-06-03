using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Web.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using BDO.GWT.Core.Data;

namespace BDO.GWT.Core.DTOs
{
    //Dto used for Blog page
    // CardsGrid used to display blogs
    //CardsGrid has API endpoint to fetch blogs on intial load and on filter change
    public class BlogPageDto
    {
        public string Title { get; set; } = null!;

        public HeroData Hero { get; set; } = null!;

        public bool EnableFilters { get; set; } = false;

        public CardsGridDto CardsGrid { get; set; } = null!;

    }
}
