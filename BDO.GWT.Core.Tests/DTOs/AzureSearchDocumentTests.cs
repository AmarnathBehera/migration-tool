using System;
using System.Text.Json;

using BDO.GWT.Core.DTOs.AzureCognitiveSearch.Models;

using NUnit.Framework;

namespace BDO.GWT.Core.Tests.DTOs
{
    public class AzureSearchDocumentTests
    {
        [Test]
        public void Properties_CanBeSetAndRetrieved()
        {
            var now = DateTimeOffset.UtcNow;

            var document = new AzureSearchDocument
            {
                SysId = "obj-1",
                DocumentName = "Sample document",
                Url = "/sample-url",
                ClassName = "GWT.Insight",
                SysCulture = "en-US",
                ItemGuid = Guid.NewGuid().ToString(),

                AboutPageID = 1,
                BlogID = 2,
                BlogPostID = 3,
                CareersID = 4,
                DealID = 5,
                EventID = 6,
                InsightID = 7,
                NewsArticleID = 8,
                WebinarID = 9,

                ContentType = "Insight",
                EventType = "Webinar",

                Content = "content",
                ContentBody = "content body",
                DescriptionBody = "description body",
                DocumentContent = "document content",

                DocumentPageTitle = "page title",
                DocumentPageDescription = "page description",
                DocumentPageKeyWords = "keyword1,keyword2",
                MetadataTitle = "meta title",
                MetadataDescription = "meta description",
                MetadataTeaserImage = "/teaser.png",
                MetadataTeaserImageAltText = "teaser alt",
                DocumentTags = "tag1,tag2",

                HeroTitle = "hero title",
                HeroDescription = "hero description",
                HeroBackgroundImage = "/hero.png",
                Image = "/image.png",
                ImageAltText = "image alt",
                SectionTitle = "section title",

                DocumentID = 10,
                DocumentNodeID = 11,
                DocumentForeignKeyValue = 12,
                DocumentTagGroupID = 13,
                DocumentCreatedByUserID = 14,
                DocumentModifiedByUserID = 15,
                DocumentSearchExcluded = false,

                DocumentCreatedWhen = now,
                DocumentLastPublished = now,
                PublishFrom = now,
                PublishTo = now,
                SortDate = now,

                EventContactEmail = "contact@example.com",
                EventContactName = "Contact Name",
                EventContactPhone = "555-1234",
                EventDateFrom = now,
                EventDateTo = now,
                EventIsPrivate = true,
                EventLocationTitle = "Location title",
                EventLocationCity = "City",
                EventLocationAddress1 = "Address1",
                EventLocationAddress2 = "Address2",
                EventLocationState = "State",
                EventLocationCountry = "Country",
                EventLocationZip = "00000",

                PersonFirstName = "First",
                PersonLastName = "Last",
                PersonJobTitle = "Job title",
                PersonEmail = "person@example.com",
                PersonPhone = "555-0000",
                PersonMobilePhone = "555-1111",
                PersonDirectDialNumber = "555-2222",
                PersonSurnamePrefixes = "van",

                ClientName = "Client",
                DealContent = "Deal content",
                CpeDescription = "CPE description",
                RemoteDealID = 16,
                SectionDealsID = 17,

                RegionName = "Region",
                LocationCityID = 18,

                NodeID = 19,

                Facebook = "https://facebook.com/x",
                Twitter = "https://twitter.com/x",
                LinkedIn = "https://linkedin.com/x",

                WebsiteChannelName = "GlobalChannel",
                WebPageItemTreePath = "/tree/path",
                WebPageItemGuid = Guid.NewGuid().ToString(),
            };

            Assert.Multiple(() =>
            {
                Assert.That(document.SysId, Is.EqualTo("obj-1"));
                Assert.That(document.DocumentName, Is.EqualTo("Sample document"));
                Assert.That(document.Url, Is.EqualTo("/sample-url"));
                Assert.That(document.ClassName, Is.EqualTo("GWT.Insight"));
                Assert.That(document.SysCulture, Is.EqualTo("en-US"));

                Assert.That(document.AboutPageID, Is.EqualTo(1));
                Assert.That(document.BlogID, Is.EqualTo(2));
                Assert.That(document.BlogPostID, Is.EqualTo(3));
                Assert.That(document.CareersID, Is.EqualTo(4));
                Assert.That(document.DealID, Is.EqualTo(5));
                Assert.That(document.EventID, Is.EqualTo(6));
                Assert.That(document.InsightID, Is.EqualTo(7));
                Assert.That(document.NewsArticleID, Is.EqualTo(8));
                Assert.That(document.WebinarID, Is.EqualTo(9));

                Assert.That(document.ContentType, Is.EqualTo("Insight"));
                Assert.That(document.EventType, Is.EqualTo("Webinar"));

                Assert.That(document.DocumentSearchExcluded, Is.False);
                Assert.That(document.EventIsPrivate, Is.True);

                Assert.That(document.DocumentCreatedWhen, Is.EqualTo(now));
                Assert.That(document.SortDate, Is.EqualTo(now));

                Assert.That(document.WebsiteChannelName, Is.EqualTo("GlobalChannel"));
                Assert.That(document.WebPageItemTreePath, Is.EqualTo("/tree/path"));
            });
        }

        [Test]
        public void DefaultConstructor_AllPropertiesAreNull()
        {
            var document = new AzureSearchDocument();

            Assert.Multiple(() =>
            {
                Assert.That(document.SysId, Is.Null);
                Assert.That(document.DocumentName, Is.Null);
                Assert.That(document.Url, Is.Null);
                Assert.That(document.AboutPageID, Is.Null);
                Assert.That(document.DocumentSearchExcluded, Is.Null);
                Assert.That(document.EventIsPrivate, Is.Null);
                Assert.That(document.DocumentCreatedWhen, Is.Null);
                Assert.That(document.WebPageItemGuid, Is.Null);
            });
        }

        [Test]
        public void Serialize_Deserialize_RoundTrip_PreservesValues()
        {
            var original = new AzureSearchDocument
            {
                SysId = "obj-42",
                DocumentName = "Round trip document",
                Url = "/round-trip",
                ContentType = "News",
                InsightID = 99,
                DocumentSearchExcluded = true,
                DocumentCreatedWhen = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            };

            var json = JsonSerializer.Serialize(original);
            var deserialized = JsonSerializer.Deserialize<AzureSearchDocument>(json);

            Assert.That(deserialized, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(deserialized!.SysId, Is.EqualTo(original.SysId));
                Assert.That(deserialized.DocumentName, Is.EqualTo(original.DocumentName));
                Assert.That(deserialized.Url, Is.EqualTo(original.Url));
                Assert.That(deserialized.ContentType, Is.EqualTo(original.ContentType));
                Assert.That(deserialized.InsightID, Is.EqualTo(original.InsightID));
                Assert.That(deserialized.DocumentSearchExcluded, Is.EqualTo(original.DocumentSearchExcluded));
                Assert.That(deserialized.DocumentCreatedWhen, Is.EqualTo(original.DocumentCreatedWhen));
            });
        }

        [Test]
        public void Serialize_UsesJsonPropertyNameMappings()
        {
            var document = new AzureSearchDocument
            {
                SysId = "obj-key",
                DocumentName = "Doc name",
                ClassName = "ContentTypeName-value",
                SysCulture = "LanguageName-value",
                DocumentSearchExcluded = true,
            };

            var json = JsonSerializer.Serialize(document);

            using var jsonDocument = JsonDocument.Parse(json);
            var root = jsonDocument.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(root.TryGetProperty("ObjectID", out _), Is.True);
                Assert.That(root.TryGetProperty("Name", out _), Is.True);
                Assert.That(root.TryGetProperty("ContentTypeName", out _), Is.True);
                Assert.That(root.TryGetProperty("LanguageName", out _), Is.True);
                Assert.That(root.TryGetProperty("SearchExcluded", out _), Is.True);
                Assert.That(root.TryGetProperty("SysId", out _), Is.False);
                Assert.That(root.TryGetProperty("ClassName", out _), Is.False);
            });
        }
    }
}