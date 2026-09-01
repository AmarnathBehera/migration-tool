using System;

using BDO.GWT.GLOBAL.Search;

using NUnit.Framework;

namespace BDO.GWT.Core.Tests.DTOs
{
    public class GlobalSearchModelTests
    {
        [Test]
        public void DefaultConstructor_StringProperties_AreEmpty_NotNull()
        {
            var model = new GlobalSearchModel();

            Assert.Multiple(() =>
            {
                Assert.That(model.ContentType, Is.EqualTo(string.Empty));
                Assert.That(model.EventType, Is.EqualTo(string.Empty));
                Assert.That(model.Content, Is.EqualTo(string.Empty));
                Assert.That(model.ContentBody, Is.EqualTo(string.Empty));
                Assert.That(model.DescriptionBody, Is.EqualTo(string.Empty));
                Assert.That(model.DocumentContent, Is.EqualTo(string.Empty));
                Assert.That(model.DocumentPageTitle, Is.EqualTo(string.Empty));
                Assert.That(model.DocumentPageDescription, Is.EqualTo(string.Empty));
                Assert.That(model.DocumentPageKeywords, Is.EqualTo(string.Empty));
                Assert.That(model.MetadataTitle, Is.EqualTo(string.Empty));
                Assert.That(model.MetadataDescription, Is.EqualTo(string.Empty));
                Assert.That(model.MetadataTeaserImage, Is.EqualTo(string.Empty));
                Assert.That(model.MetadataTeaserImageAltText, Is.EqualTo(string.Empty));
                Assert.That(model.DocumentTags, Is.EqualTo(string.Empty));
                Assert.That(model.HeroTitle, Is.EqualTo(string.Empty));
                Assert.That(model.HeroDescription, Is.EqualTo(string.Empty));
                Assert.That(model.HeroBackgroundImage, Is.EqualTo(string.Empty));
                Assert.That(model.Image, Is.EqualTo(string.Empty));
                Assert.That(model.ImageAltText, Is.EqualTo(string.Empty));
                Assert.That(model.SectionTitle, Is.EqualTo(string.Empty));
                Assert.That(model.EventContactEmail, Is.EqualTo(string.Empty));
                Assert.That(model.EventContactName, Is.EqualTo(string.Empty));
                Assert.That(model.EventContactPhone, Is.EqualTo(string.Empty));
                Assert.That(model.EventLocationTitle, Is.EqualTo(string.Empty));
                Assert.That(model.EventLocationCity, Is.EqualTo(string.Empty));
                Assert.That(model.EventLocationAddress1, Is.EqualTo(string.Empty));
                Assert.That(model.EventLocationAddress2, Is.EqualTo(string.Empty));
                Assert.That(model.EventLocationState, Is.EqualTo(string.Empty));
                Assert.That(model.EventLocationCountry, Is.EqualTo(string.Empty));
                Assert.That(model.EventLocationZip, Is.EqualTo(string.Empty));
                Assert.That(model.PersonFirstName, Is.EqualTo(string.Empty));
                Assert.That(model.PersonLastName, Is.EqualTo(string.Empty));
                Assert.That(model.PersonJobTitle, Is.EqualTo(string.Empty));
                Assert.That(model.PersonEmail, Is.EqualTo(string.Empty));
                Assert.That(model.PersonPhone, Is.EqualTo(string.Empty));
                Assert.That(model.PersonMobilePhone, Is.EqualTo(string.Empty));
                Assert.That(model.PersonDirectDialNumber, Is.EqualTo(string.Empty));
                Assert.That(model.PersonSurnamePrefixes, Is.EqualTo(string.Empty));
                Assert.That(model.ClientName, Is.EqualTo(string.Empty));
                Assert.That(model.DealContent, Is.EqualTo(string.Empty));
                Assert.That(model.CpeDescription, Is.EqualTo(string.Empty));
                Assert.That(model.RegionName, Is.EqualTo(string.Empty));
                Assert.That(model.Facebook, Is.EqualTo(string.Empty));
                Assert.That(model.Twitter, Is.EqualTo(string.Empty));
                Assert.That(model.LinkedIn, Is.EqualTo(string.Empty));
                Assert.That(model.WebsiteChannelName, Is.EqualTo(string.Empty));
                Assert.That(model.WebPageItemTreePath, Is.EqualTo(string.Empty));
                Assert.That(model.WebPageItemGuid, Is.EqualTo(string.Empty));
            });
        }

        [Test]
        public void DefaultConstructor_NullableProperties_AreNull()
        {
            var model = new GlobalSearchModel();

            Assert.Multiple(() =>
            {
                Assert.That(model.AboutPageID, Is.Null);
                Assert.That(model.BlogID, Is.Null);
                Assert.That(model.BlogPostID, Is.Null);
                Assert.That(model.CareersID, Is.Null);
                Assert.That(model.DealID, Is.Null);
                Assert.That(model.EventID, Is.Null);
                Assert.That(model.InsightID, Is.Null);
                Assert.That(model.NewsArticleID, Is.Null);
                Assert.That(model.WebinarID, Is.Null);
                Assert.That(model.DocumentID, Is.Null);
                Assert.That(model.DocumentNodeID, Is.Null);
                Assert.That(model.DocumentForeignKeyValue, Is.Null);
                Assert.That(model.DocumentTagGroupID, Is.Null);
                Assert.That(model.DocumentCreatedByUserID, Is.Null);
                Assert.That(model.DocumentModifiedByUserID, Is.Null);
                Assert.That(model.SearchExcluded, Is.Null);
                Assert.That(model.CreatedWhen, Is.Null);
                Assert.That(model.LastPublished, Is.Null);
                Assert.That(model.PublishFrom, Is.Null);
                Assert.That(model.PublishTo, Is.Null);
                Assert.That(model.SortDate, Is.Null);
                Assert.That(model.EventDateFrom, Is.Null);
                Assert.That(model.EventDateTo, Is.Null);
                Assert.That(model.EventIsPrivate, Is.Null);
                Assert.That(model.RemoteDealID, Is.Null);
                Assert.That(model.SectionDealsID, Is.Null);
                Assert.That(model.LocationCityID, Is.Null);
                Assert.That(model.NodeID, Is.Null);
            });
        }

        [Test]
        public void Properties_CanBeSetAndRetrieved()
        {
            var now = DateTimeOffset.UtcNow;

            var model = new GlobalSearchModel
            {
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
                DocumentPageKeywords = "keyword1,keyword2",
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
                SearchExcluded = true,

                CreatedWhen = now,
                LastPublished = now,
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
                Assert.That(model.AboutPageID, Is.EqualTo(1));
                Assert.That(model.BlogID, Is.EqualTo(2));
                Assert.That(model.BlogPostID, Is.EqualTo(3));
                Assert.That(model.CareersID, Is.EqualTo(4));
                Assert.That(model.DealID, Is.EqualTo(5));
                Assert.That(model.EventID, Is.EqualTo(6));
                Assert.That(model.InsightID, Is.EqualTo(7));
                Assert.That(model.NewsArticleID, Is.EqualTo(8));
                Assert.That(model.WebinarID, Is.EqualTo(9));

                Assert.That(model.ContentType, Is.EqualTo("Insight"));
                Assert.That(model.EventType, Is.EqualTo("Webinar"));

                Assert.That(model.SearchExcluded, Is.True);
                Assert.That(model.EventIsPrivate, Is.True);

                Assert.That(model.CreatedWhen, Is.EqualTo(now));
                Assert.That(model.SortDate, Is.EqualTo(now));
                Assert.That(model.EventDateFrom, Is.EqualTo(now));
                Assert.That(model.EventDateTo, Is.EqualTo(now));

                Assert.That(model.EventContactEmail, Is.EqualTo("contact@example.com"));
                Assert.That(model.EventLocationCity, Is.EqualTo("City"));

                Assert.That(model.PersonFirstName, Is.EqualTo("First"));
                Assert.That(model.PersonLastName, Is.EqualTo("Last"));

                Assert.That(model.ClientName, Is.EqualTo("Client"));
                Assert.That(model.RemoteDealID, Is.EqualTo(16));
                Assert.That(model.SectionDealsID, Is.EqualTo(17));

                Assert.That(model.RegionName, Is.EqualTo("Region"));
                Assert.That(model.LocationCityID, Is.EqualTo(18));
                Assert.That(model.NodeID, Is.EqualTo(19));

                Assert.That(model.Facebook, Is.EqualTo("https://facebook.com/x"));
                Assert.That(model.Twitter, Is.EqualTo("https://twitter.com/x"));
                Assert.That(model.LinkedIn, Is.EqualTo("https://linkedin.com/x"));

                Assert.That(model.WebsiteChannelName, Is.EqualTo("GlobalChannel"));
                Assert.That(model.WebPageItemTreePath, Is.EqualTo("/tree/path"));
                Assert.That(model.WebPageItemGuid, Is.Not.Empty);
            });
        }

        [Test]
        public void Model_IsAssignableTo_BaseAzureSearchModel()
        {
            var model = new GlobalSearchModel();

            Assert.That(model, Is.InstanceOf<Kentico.Xperience.AzureSearch.Indexing.BaseAzureSearchModel>());
        }

        [Test]
        public void NullableDateTimeProperties_CanBeSetToNullAfterAssignment()
        {
            var model = new GlobalSearchModel
            {
                CreatedWhen = DateTimeOffset.UtcNow,
            };

            model.CreatedWhen = null;

            Assert.That(model.CreatedWhen, Is.Null);
        }

        [Test]
        public void BooleanProperties_CanBeToggled()
        {
            var model = new GlobalSearchModel
            {
                SearchExcluded = false,
                EventIsPrivate = false,
            };

            Assert.Multiple(() =>
            {
                Assert.That(model.SearchExcluded, Is.False);
                Assert.That(model.EventIsPrivate, Is.False);
            });

            model.SearchExcluded = true;
            model.EventIsPrivate = true;

            Assert.Multiple(() =>
            {
                Assert.That(model.SearchExcluded, Is.True);
                Assert.That(model.EventIsPrivate, Is.True);
            });
        }
    }
}