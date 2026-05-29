using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs.ReusableFieldsDTOs
{
    public class ContentWithShowOption
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public bool ShowTitle { get; set; }

        public bool ShowDescription { get; set; }
    }
}
