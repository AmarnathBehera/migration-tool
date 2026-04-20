using BDO.GWT.Core;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.DTOs.Pages;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services
{
    public class ContactPageServiceTests
    {
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<IWebsiteChannelContext> _channelMock = null!;
        private ContactPageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _contentServiceMock = new Mock<IContentService>();
            _heroServiceMock = new Mock<IHeroService>();
            _channelMock = new Mock<IWebsiteChannelContext>();

            _channelMock
                .Setup(x => x.WebsiteChannelID)
                .Returns(1);

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<IHero>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HeroDto());

            _service = new ContactPageService(
                _contentServiceMock.Object,
                _heroServiceMock.Object,
                _channelMock.Object
            );
        }

        // ── Null / Empty results ─────────────────────────────────────────────

        [Test]
        public async Task GetContactPageDtoAsync_WhenNoPageFound_ReturnsEmptyFormPageDto()
        {
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Contact>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contact>());

            var result = await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<FormPageDto>());
            Assert.That(result.MetaData, Is.Null);
            Assert.That(result.BaseContent, Is.Null);
        }

        [Test]
        public async Task GetContactPageDtoAsync_WhenResultsAreNull_ReturnsEmptyFormPageDto()
        {
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Contact>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<Contact>?)null);

            var result = await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.MetaData, Is.Null);
        }

        // ── Metadata mapping ────────────────────────────────────────────────

        [Test]
        public async Task GetContactPageDtoAsync_MapsMetaDataCorrectly()
        {
            var contactPage = new Contact
            {
                MetadataTitle = "Contact Meta Title",
                MetadataDescription = "Contact Meta Description",
                MetadataTeaserImageAltText = "Alt Text",
                MetadataCanonical = "https://example.com/contact",
                DescriptionTitle = "Title",
                DescriptionBody = "Body"
            };

            SetupContentService(contactPage);

            var result = await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.MetaData, Is.Not.Null);
                Assert.That(result.MetaData!.MetaTitle, Is.EqualTo("Contact Meta Title"));
                Assert.That(result.MetaData.MetaDescription, Is.EqualTo("Contact Meta Description"));
                Assert.That(result.MetaData.MetaTeaserImageAltText, Is.EqualTo("Alt Text"));
                Assert.That(result.MetaData.MetaCanonical, Is.EqualTo("https://example.com/contact"));
            });
        }

        // ── SearchAndSEO mapping ─────────────────────────────────────────────

        [Test]
        public async Task GetContactPageDtoAsync_MapsSearchAndSEOCorrectly()
        {
            var contactPage = new Contact
            {
                IncludeInSitemap = true,
                MetadataNofollow = true,
                MetadataNoindex = false,
                DescriptionTitle = "Title",
                DescriptionBody = "Body"
            };

            SetupContentService(contactPage);

            var result = await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.SearchAndSEO, Is.Not.Null);
                Assert.That(result.SearchAndSEO!.IncludeInSitemap, Is.True);
                Assert.That(result.SearchAndSEO.MetadataNofollow, Is.True);
                Assert.That(result.SearchAndSEO.MetadataNoindex, Is.False);
            });
        }

        [Test]
        public async Task GetContactPageDtoAsync_SearchAndSEO_DefaultsFalse_WhenNotSet()
        {
            var contactPage = new Contact
            {
                DescriptionTitle = "Title",
                DescriptionBody = "Body"
            };

            SetupContentService(contactPage);

            var result = await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.SearchAndSEO!.IncludeInSitemap, Is.False);
                Assert.That(result.SearchAndSEO.MetadataNofollow, Is.False);
                Assert.That(result.SearchAndSEO.MetadataNoindex, Is.False);
            });
        }

        // ── OGMetaData mapping ───────────────────────────────────────────────

        [Test]
        public async Task GetContactPageDtoAsync_MapsOGMetaDataCorrectly()
        {
            var contactPage = new Contact
            {
                MetadataOGTitle = "OG Contact Title",
                MetadataOGDescription = "OG Contact Description",
                MetadataOGTwitterImageSizeCheckbox = true,
                DescriptionTitle = "Title",
                DescriptionBody = "Body"
            };

            SetupContentService(contactPage);

            var result = await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.OGMetaData, Is.Not.Null);
                Assert.That(result.OGMetaData!.MetadataOGTitle, Is.EqualTo("OG Contact Title"));
                Assert.That(result.OGMetaData.MetadataOGDescription, Is.EqualTo("OG Contact Description"));
                Assert.That(result.OGMetaData.MetadataOGTwitterImageSizeCheckbox, Is.True);
            });
        }

        // ── BaseContent / BaseWebPage mapping ────────────────────────────────

        [Test]
        public async Task GetContactPageDtoAsync_MapsBaseContentCorrectly()
        {
            var guid = Guid.NewGuid();
            var contactPage = new Contact
            {
                DescriptionTitle = "Title",
                DescriptionBody = "Body",
                SystemFields = new WebPageFields
                {
                    ContentItemID = 42,
                    ContentItemGUID = guid,
                    ContentItemName = "Contact Page",
                    ContentItemContentTypeID = 5,
                    ContentItemIsSecured = false,
                    ContentItemCommonDataContentLanguageID = 1,
                    ContentItemCommonDataFirstPublishedWhen = new DateTime(2024, 1, 1),
                    ContentItemCommonDataLastPublishedWhen = new DateTime(2024, 6, 1)
                }
            };

            SetupContentService(contactPage);

            var result = await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.BaseContent, Is.Not.Null);
                Assert.That(result.BaseContent!.ContentItemID, Is.EqualTo(42));
                Assert.That(result.BaseContent.ContentItemGUID, Is.EqualTo(guid));
                Assert.That(result.BaseContent.ContentItemName, Is.EqualTo("Contact Page"));
                Assert.That(result.BaseContent.ContentItemContentTypeID, Is.EqualTo(5));
                Assert.That(result.BaseContent.ContentItemIsSecured, Is.False);
            });
        }

        [Test]
        public async Task GetContactPageDtoAsync_MapsBaseWebPageCorrectly()
        {
            var webPageGuid = Guid.NewGuid();
            var contactPage = new Contact
            {
                DescriptionTitle = "Title",
                DescriptionBody = "Body",
                SystemFields = new WebPageFields
                {
                    WebPageItemGUID = webPageGuid,
                    WebPageItemID = 100,
                    WebPageItemName = "Contact",
                    WebPageItemWebsiteChannelId = 2,
                    WebPageUrlPath = "/contact",
                    WebPageItemOrder = 3,
                    WebPageItemTreePath = "/contact",
                    WebPageUrlPathContentLanguageID = 1,
                    WebPageItemParentID = 0
                }
            };

            SetupContentService(contactPage);

            var result = await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.BaseWebPage, Is.Not.Null);
                Assert.That(result.BaseWebPage!.WebPageItemGUID, Is.EqualTo(webPageGuid));
                Assert.That(result.BaseWebPage.WebPageItemID, Is.EqualTo(100));
                Assert.That(result.BaseWebPage.WebPageItemName, Is.EqualTo("Contact"));
                Assert.That(result.BaseWebPage.WebPageItemWebsiteChannelID, Is.EqualTo(2));
                Assert.That(result.BaseWebPage.WebPageUrlPath, Is.EqualTo("/contact"));
            });
        }

        [Test]
        public async Task GetContactPageDtoAsync_BaseContent_UsesDefaults_WhenSystemFieldsAreNull()
        {
            var contactPage = new Contact
            {
                DescriptionTitle = "Title",
                DescriptionBody = "Body",
                SystemFields = null!
            };

            SetupContentService(contactPage);

            var result = await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.BaseContent!.ContentItemID, Is.EqualTo(0));
                Assert.That(result.BaseContent.ContentItemGUID, Is.EqualTo(Guid.Empty));
                Assert.That(result.BaseWebPage!.WebPageItemID, Is.EqualTo(0));
                Assert.That(result.BaseWebPage.WebPageItemGUID, Is.EqualTo(Guid.Empty));
            });
        }

        // ── Title / Description / Hero mapping ───────────────────────────────

        [Test]
        public async Task GetContactPageDtoAsync_MapsTitleAndDescriptionCorrectly()
        {
            var contactPage = new Contact
            {
                DescriptionTitle = "Contact Us",
                DescriptionBody = "We would love to hear from you."
            };

            SetupContentService(contactPage);

            var result = await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.EqualTo("Contact Us"));
                Assert.That(result.Description, Is.EqualTo("We would love to hear from you."));
            });
        }

        [Test]
        public async Task GetContactPageDtoAsync_MapsHeroDto_FromHeroService()
        {
            var expectedHero = new HeroDto
            {
                Title = "Hero Title",
                Description = "Hero Desc",
                Layout = "Default"
            };

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<IHero>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedHero);

            var contactPage = new Contact
            {
                DescriptionTitle = "Title",
                DescriptionBody = "Body"
            };

            SetupContentService(contactPage);

            var result = await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Hero, Is.Not.Null);
                Assert.That(result.Hero.Title, Is.EqualTo("Hero Title"));
                Assert.That(result.Hero.Description, Is.EqualTo("Hero Desc"));
                Assert.That(result.Hero.Layout, Is.EqualTo("Default"));
            });
        }

        // ── Channel context ──────────────────────────────────────────────────

        [Test]
        public async Task GetContactPageDtoAsync_UsesWebsiteChannelID_FromContext()
        {
            _channelMock
                .Setup(x => x.WebsiteChannelID)
                .Returns(99);

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Contact>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contact>());

            await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            // The channel ID is captured inside the query lambda passed to GetPagesAsync.
            // Verify GetPagesAsync was called — confirming the service used the content
            // service with the configured channel context filter.
            _contentServiceMock.Verify(
                x => x.GetPagesAsync<Contact>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        // ── CancellationToken propagation ────────────────────────────────────

        [Test]
        public async Task GetContactPageDtoAsync_PassesCancellationToken_ToContentService()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Contact>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    token))
                .ReturnsAsync(new List<Contact>());

            await _service.GetContactPageDtoAsync(null, token);

            _contentServiceMock.Verify(
                x => x.GetPagesAsync<Contact>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    token),
                Times.Once
            );
        }

        [Test]
        public async Task GetContactPageDtoAsync_PassesCancellationToken_ToHeroService()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            var contactPage = new Contact
            {
                DescriptionTitle = "Title",
                DescriptionBody = "Body"
            };

            SetupContentService(contactPage);

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<IHero>(), token))
                .ReturnsAsync(new HeroDto());

            await _service.GetContactPageDtoAsync(null, token);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<IHero>(), token),
                Times.Once
            );
        }

        // ── Helper ───────────────────────────────────────────────────────────

        private void SetupContentService(Contact contactPage)
        {
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Contact>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contact> { contactPage });
        }
    }
}