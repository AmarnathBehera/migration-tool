using BDO;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.Core.Tests.Services
{
    public class StatusCodePageServiceTests
    {
        private StatusCodePageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _service = new StatusCodePageService();
        }

        [Test]
        public void GetStatusCodePageDto_MapsBasicFields_Correctly()
        {
            var page = CreateStatusCodePage();

            var result = _service.GetStatusCodePageDto(page);

            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.EqualTo("Page Not Found"));
                Assert.That(result.Description, Is.EqualTo("The requested page does not exist."));
            });
        }

        [Test]
        public void GetStatusCodePageDto_MapsMetaData_Correctly()
        {
            var page = CreateStatusCodePage();

            var result = _service.GetStatusCodePageDto(page);

            Assert.That(result.MetaDataSEOItems, Is.Not.Null);
            var metaData = result.MetaDataSEOItems!.MetaData;

            Assert.Multiple(() =>
            {
                Assert.That(metaData!.MetaTitle, Is.EqualTo("Page Not Found"));
                Assert.That(metaData.MetaDescription, Is.EqualTo("The requested page does not exist."));
                Assert.That(metaData.MetadataTeaserImage, Is.EqualTo("/media/teaser.jpg"));
                Assert.That(metaData.MetaTeaserImageAltText, Is.EqualTo("Teaser Alt"));
                Assert.That(metaData.MetaCanonical, Is.EqualTo("https://example.com/404"));
            });
        }

        [Test]
        public void GetStatusCodePageDto_MapsSeoItems_Correctly()
        {
            var page = CreateStatusCodePage();
            page.IncludeInSitemap = true;
            page.MetadataNofollow = true;
            page.MetadataNoindex = false;

            var result = _service.GetStatusCodePageDto(page);

            var seo = result.MetaDataSEOItems!.SEOItemsDto;

            Assert.Multiple(() =>
            {
                Assert.That(seo!.IncludeInSitemap, Is.True);
                Assert.That(seo.MetadataNofollow, Is.True);
                Assert.That(seo.MetadataNoindex, Is.False);
            });
        }

        [Test]
        public void GetStatusCodePageDto_MapsOpenGraphMetaData_Correctly()
        {
            var page = CreateStatusCodePage();

            var result = _service.GetStatusCodePageDto(page);

            var og = result.MetaDataSEOItems!.OGMetaData;

            Assert.Multiple(() =>
            {
                Assert.That(og!.MetadataOGTitle, Is.EqualTo("OG Title"));
                Assert.That(og.MetadataOGDescription, Is.EqualTo("OG Description"));
                Assert.That(og.MetadataOGImage, Is.EqualTo("/media/og-image.jpg"));
                Assert.That(og.MetadataOGTwitterImage, Is.EqualTo("/media/twitter-image.jpg"));
                Assert.That(og.MetadataOGTwitterImageSizeCheckbox, Is.True);
            });
        }

        [Test]
        public void GetStatusCodePageDto_ReturnsNullImageUrls_WhenNoImagesProvided()
        {
            var page = CreateStatusCodePage();
            page.MetadataTeaserImage = Enumerable.Empty<ContentAttachment>();
            page.MetadataOGImage = Enumerable.Empty<ContentAttachment>();
            page.MetadataOGTwitterImage = Enumerable.Empty<ContentAttachment>();

            var result = _service.GetStatusCodePageDto(page);

            Assert.Multiple(() =>
            {
                Assert.That(result.MetaDataSEOItems!.MetaData!.MetadataTeaserImage, Is.Null);
                Assert.That(result.MetaDataSEOItems.OGMetaData!.MetadataOGImage, Is.Null);
                Assert.That(result.MetaDataSEOItems.OGMetaData.MetadataOGTwitterImage, Is.Null);
            });
        }

        [Test]
        public void GetStatusCodePageDto_UsesFirstImage_WhenMultipleImagesProvided()
        {
            var page = CreateStatusCodePage();
            page.MetadataTeaserImage = new[]
            {
                CreateAttachment("/media/first.jpg"),
                CreateAttachment("/media/second.jpg")
            };

            var result = _service.GetStatusCodePageDto(page);

            Assert.That(
                result.MetaDataSEOItems!.MetaData!.MetadataTeaserImage,
                Is.EqualTo("/media/first.jpg"));
        }

        private static StatusCode CreateStatusCodePage()
        {
            return new StatusCode
            {
                HttpStatusCode = 404,
                MetadataTitle = "Page Not Found",
                MetadataDescription = "The requested page does not exist.",
                MetadataTeaserImage = new[] { CreateAttachment("/media/teaser.jpg") },
                MetadataTeaserImageAltText = "Teaser Alt",
                MetadataCanonical = "https://example.com/404",
                IncludeInSitemap = false,
                MetadataNofollow = false,
                MetadataNoindex = false,
                MetadataOGTitle = "OG Title",
                MetadataOGDescription = "OG Description",
                MetadataOGImage = new[] { CreateAttachment("/media/og-image.jpg") },
                MetadataOGTwitterImage = new[] { CreateAttachment("/media/twitter-image.jpg") },
                MetadataOGTwitterImageSizeCheckbox = true
            };
        }

        private static ContentAttachment CreateAttachment(string url)
        {
            return new ContentAttachment
            {
                ContentAttachmentAsset = new ContentItemAsset { Url = url }
            };
        }
    }
}
