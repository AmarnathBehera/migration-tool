using BDO.GWT.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.Entities
{
    public interface IDealCardPage
    {
        string MetadataTitle { get; set; }

        string? MetadataDescription { get; set; }

        Link? MetadataTeaserImage { get; set; }

        string? MetadataTeaserImageAltText { get; set; }

        List<string> Industry { get; set; }

        List<string> SubIndustry { get; set; }

        List<int> DealType { get; set; }

        public DateTime DateField { get; set; }

        public string? DealCountry { get; set; }

        bool IncludeInSitemap { get; set; }
    }
}
