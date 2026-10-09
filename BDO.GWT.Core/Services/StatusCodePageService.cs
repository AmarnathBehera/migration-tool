using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using Microsoft.AspNetCore.Http;
using System.Web;

namespace BDO.GWT.Core.Services;

public interface IStatusCodePageService
{
    StatusCodePageDto GetStatusCodePageDto(StatusCode page);
}

public class StatusCodePageService : IStatusCodePageService
{
    public StatusCodePageDto GetStatusCodePageDto(StatusCode page)
    {
        var dto = new StatusCodePageDto
        {
            Title = page.MetadataTitle,
            //Description = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(page.MetadataDescription),
            Description = page.MetadataDescription,
            MetaDataSEOItems = new MetaDataSEOItemsDto
            {
                MetaData = new MetaDataDto
                {
                    MetaTitle = page.MetadataTitle,
                    MetaDescription = page.MetadataDescription,
                    MetadataTeaserImage = page.MetadataTeaserImage.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                    MetaTeaserImageAltText = page.MetadataTeaserImageAltText,
                    MetaCanonical = page.MetadataCanonical
                },

                SEOItemsDto = new SearchAndSEODto
                {
                    IncludeInSitemap = page.IncludeInSitemap,
                    MetadataNofollow = page.MetadataNofollow,
                    MetadataNoindex = page.MetadataNoindex
                },

                OGMetaData = new OGMetaDataDto
                {
                    MetadataOGTitle = page.MetadataOGTitle,
                    MetadataOGDescription = page.MetadataOGDescription,
                    MetadataOGImage = page.MetadataOGImage.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                    MetadataOGTwitterImage = page.MetadataOGTwitterImage.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                    MetadataOGTwitterImageSizeCheckbox = page.MetadataOGTwitterImageSizeCheckbox
                },
            }
        };

        return dto;
    }
}
