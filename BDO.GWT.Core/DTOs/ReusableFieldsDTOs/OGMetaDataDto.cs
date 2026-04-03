using CMS.ContentEngine;


namespace BDO.GWT.Core.DTOs.ReusableFieldsDTOs;

    public class OGMetaDataDto
    {
        public string? MetadataOGTitle { get; set; }

        public string? MetadataOGDescription { get; set; }

       public string? Url { get; set; }

       public string? MetadataOGImage { get; set; }

        public bool MetadataOGTwitterImageSizeCheckbox { get; set; }

        public IEnumerable<IContentItemFieldsSource> MetadataOGTwitterImage { get; set; }
    }

