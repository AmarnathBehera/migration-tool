using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core
{
    public class BaseContent
    {
        public int ContentItemID { get; set; }

        public Guid ContentItemGUID { get; set; }

        public string? ContentItemName { get; set; }

        public bool ContentItemIsSecured { get; set; }

        public int ContentItemContentTypeID { get; set; }

        public int ContentItemCommonDataContentLanguageID { get; set; }

        public DateTime? ContentItemCommonDataFirstPublishedWhen { get; set; }

        public DateTime? ContentItemCommonDataLastPublishedWhen { get; set; }

        public string? ContentItemCommonDataVersionStatus { get; set; }
    }
}
