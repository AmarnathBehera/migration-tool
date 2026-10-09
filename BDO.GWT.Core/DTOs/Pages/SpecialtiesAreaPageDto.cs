using BDO.GWT.Web.Core.DTOs;

namespace BDO.GWT.Core.DTOs.Pages;

public class SpecialtiesAreaPageDto
    {
        public string Title { get; set; } = null!;

        public string BodyContent { get; set; } = null!;

        public HeroDto Hero { get; set; } = null!;

        public SideBoxDto? SideBox { get; set; } = null!;
    }

