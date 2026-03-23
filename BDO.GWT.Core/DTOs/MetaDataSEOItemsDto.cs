using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs
{
    public class MetaDataSEOItemsDto
    {
        public MetaDataDto? MetaData { get; set; }

        public OGMetaDataDto? OGMetaData { get; set; }

        public SearchAndSEODto? SEOItemsDto { get; set; }
    }
}
