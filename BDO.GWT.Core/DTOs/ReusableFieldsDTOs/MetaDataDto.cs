using CMS.ContentEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs.ReusableFieldsDTOs;

public class MetaDataDto
{
    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public IEnumerable<IContentItemFieldsSource> MetadataTeaserImage { get; set; }

    public string? MetaTeaserImageAltText { get; set; }

    public string? MetaCanonical { get; set; }

}