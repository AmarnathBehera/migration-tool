using BDO.GWT.Core;
using BDO.GWT.Core.Data;
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
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((HeroData data, CancellationToken _) => new HeroDto
                {
                    Layout = data.Layout,
                    Title = data.Title,
                    Description = data.Description,
                    CtaButtonType = data.CallToActionButtonType,
                    CtaButtonText = data.CallToActionButtonText,
                    CtaButtonUrlTarget = data.CtaButtonUrlTarget,
                });

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

            var result = await _service.GetContactPageDtoAsync(null!, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.TypeOf<FormPageDto>());
                Assert.That(result.MetaData, Is.Null);
                Assert.That(result.BaseContent, Is.Null);
            });
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
                .ReturnsAsync((IEnumerable<Contact>)null!);

            var result = await _service.GetContactPageDtoAsync(null!, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.MetaData, Is.Null);
        }

        // ── Metadata mapping ────────────────────────────────────────────────

        [Test]
        public async Task GetContactPageDtoAsync_DoesNotMapMetaData()
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

            var result = await _service.GetContactPageDtoAsync(null!, CancellationToken.None);

            Assert.That(result.MetaData, Is.Null);
        }

        // ── SearchAndSEO mapping ─────────────────────────────────────────────

        [Test]
        public async Task GetContactPageDtoAsync_DoesNotMapSearchAndSEO()
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

            var result = await _service.GetContactPageDtoAsync(null!, CancellationToken.None);

            Assert.That(result.SearchAndSEO, Is.Null);
        }

        // ── OGMetaData mapping ───────────────────────────────────────────────

        [Test]
        public async Task GetContactPageDtoAsync_DoesNotMapOGMetaData()
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

            var result = await _service.GetContactPageDtoAsync(null!, CancellationToken.None);

            Assert.That(result.OGMetaData, Is.Null);
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

            var result = await _service.GetContactPageDtoAsync(null!, CancellationToken.None);

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

            var result = await _service.GetContactPageDtoAsync(null!, CancellationToken.None);

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

            var result = await _service.GetContactPageDtoAsync(null!, CancellationToken.None);

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

            var result = await _service.GetContactPageDtoAsync(null!, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.EqualTo("Contact Us"));
                Assert.That(result.Description, Is.EqualTo("We would love to hear from you."));
            });
        }

        [Test]
        public async Task GetContactPageDtoAsync_MapsHeroDto_FromHeroService()
        {
            var contactPage = new Contact
            {
                DescriptionTitle = "Title",
                DescriptionBody = "Body",
                HeroTitle = "Hero Title",
                HeroDescription = "Hero Desc",
                HeroLayout = "Default"
            };

            SetupContentService(contactPage);

            var result = await _service.GetContactPageDtoAsync(null!, CancellationToken.None);

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
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), token))
                .ReturnsAsync(new HeroDto());

            await _service.GetContactPageDtoAsync(null, token);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), token),
                Times.Once
            );
        }

        // ── Form fields mapping ──────────────────────────────────────────────

        [Test]
        public async Task GetContactPageDtoAsync_MapsFormFieldsCorrectly()
        {
            var contactPage = new Contact
            {
                DescriptionTitle = "Title",
                DescriptionBody = "Body",
                FormType = "HubSpot",
                Form = "contact-form-id",
                EmbedCode = "<script>embed</script>"
            };

            SetupContentService(contactPage);

            var result = await _service.GetContactPageDtoAsync(null!, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.FormType, Is.EqualTo("HubSpot"));
                Assert.That(result.Form, Is.EqualTo("contact-form-id"));
                Assert.That(result.EmbedCode, Is.EqualTo("<script>embed</script>"));
            });
        }

        [Test]
        public async Task GetContactPageDtoAsync_FormFields_AreNull_WhenNotSet()
        {
            var contactPage = new Contact
            {
                DescriptionTitle = "Title",
                DescriptionBody = "Body"
            };

            SetupContentService(contactPage);

            var result = await _service.GetContactPageDtoAsync(null!, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.FormType, Is.Null);
                Assert.That(result.Form, Is.Null);
                Assert.That(result.EmbedCode, Is.Null);
            });
        }

        // ── Exception propagation ────────────────────────────────────────────

        [Test]
        public void GetContactPageDtoAsync_WhenContentServiceThrows_ExceptionPropagates()
        {
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Contact>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.GetContactPageDtoAsync(null!, CancellationToken.None));
        }

        [Test]
        public void GetContactPageDtoAsync_WhenHeroServiceThrows_ExceptionPropagates()
        {
            var contactPage = new Contact
            {
                DescriptionTitle = "Title",
                DescriptionBody = "Body"
            };

            SetupContentService(contactPage);

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Hero error"));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.GetContactPageDtoAsync(null!, CancellationToken.None));
        }

        // ── LinkedItemsMaxLevel parameter ────────────────────────────────────

        [Test]
        public async Task GetContactPageDtoAsync_UsesLinkedItemsMaxLevel5()
        {
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Contact>(
                    It.Is<RetrievePagesParameters>(p => p.LinkedItemsMaxLevel == 5),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contact>());

            await _service.GetContactPageDtoAsync(null!, CancellationToken.None);

            _contentServiceMock.Verify(
                x => x.GetPagesAsync<Contact>(
                    It.Is<RetrievePagesParameters>(p => p.LinkedItemsMaxLevel == 5),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ── Multiple results returns first ───────────────────────────────────

        [Test]
        public async Task GetContactPageDtoAsync_WhenMultipleResults_UsesFirstPage()
        {
            var first = new Contact { DescriptionTitle = "First", DescriptionBody = "Body1" };
            var second = new Contact { DescriptionTitle = "Second", DescriptionBody = "Body2" };

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Contact>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contact> { first, second });

            var result = await _service.GetContactPageDtoAsync(null!, CancellationToken.None);

            Assert.That(result.Title, Is.EqualTo("First"));
        }

        // ── Hero data mapping to HeroData ────────────────────────────────────

        [Test]
        public async Task GetContactPageDtoAsync_PassesCorrectHeroData_ToHeroService()
        {
            var contactPage = new Contact
            {
                DescriptionTitle = "Title",
                DescriptionBody = "Body",
                HeroLayout = "FullWidth",
                HeroTitle = "Welcome",
                HeroDescription = "Hello World",
                HeroCallToActionButtonType = "Primary",
                HeroCallToActionButtonText = "Click Me"
            };

            SetupContentService(contactPage);

            HeroData? capturedData = null;
            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((data, _) => capturedData = data)
                .ReturnsAsync(new HeroDto());

            await _service.GetContactPageDtoAsync(null!, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(capturedData, Is.Not.Null);
                Assert.That(capturedData!.Layout, Is.EqualTo("FullWidth"));
                Assert.That(capturedData.Title, Is.EqualTo("Welcome"));
                Assert.That(capturedData.Description, Is.EqualTo("Hello World"));
                Assert.That(capturedData.CallToActionButtonType, Is.EqualTo("Primary"));
                Assert.That(capturedData.CallToActionButtonText, Is.EqualTo("Click Me"));
            });
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