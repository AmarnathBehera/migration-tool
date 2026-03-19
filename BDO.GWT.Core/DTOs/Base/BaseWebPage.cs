using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core
{
    public class BaseWebPage
    {
        public int WebPageItemID { get; set; }

        public Guid WebPageItemGUID { get; set; }

        public string? WebPageItemName { get; set; }

        public int WebPageItemOrder { get; set; }

        public int WebPageItemParentID { get; set; }

        public string? WebPageItemTreePath { get; set; }

        public int WebPageItemWebsiteChannelID { get; set; }

        public string? WebPageUrlPath { get; set; }

        public int WebPageUrlPathContentLanguageID { get; set; }
    }
}
