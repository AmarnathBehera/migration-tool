using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs
{
    public class BlogContributorsPageDto
    {
        public HeroDto Hero { get; set; } = null!;

        public MetaDataDto MetaData { get; set; } = null!;

        public string PeopleListDefaultOrder { get; set; } = null!;

        public bool ShowAuthoredInsights { get; set; }

        public string AuthoredInsightsTitle { get; set; } = null!;

        public string? AuthoredInsightsDescription { get; set; }

        public Link? AuthoredInsightsBackgroundImageUrl { get; set; }
    }
}
