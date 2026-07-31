using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Footer;
using BDO.GWT.Core.Services.Footer;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services
{
    public class FooterServiceTests
    {
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IInfoProvider<ChannelInfo>> _channelInfoProviderMock = null!;
        private Mock<IWebPageUrlResolver> _urlResolverMock = null!;
        private FooterService _service = null!;

        [SetUp]
        public void Setup()
        {
            _contentServiceMock = new Mock<IContentService>();
            _channelInfoProviderMock = new Mock<IInfoProvider<ChannelInfo>>();
            _urlResolverMock = new Mock<IWebPageUrlResolver>();
            // Register the additional interface BEFORE the mock's .Object is accessed below.
            // The GetAsync(int, CancellationToken?) extension used by FooterService internally
            // casts the provider to IInfoByIdProvider<ChannelInfo>, so the mock must implement it.
            // Moq requires As<T>() to be called prior to any .Object access.
            _channelInfoProviderMock.As<IInfoByIdProvider<ChannelInfo>>();
            _service = new FooterService(_contentServiceMock.Object, _channelInfoProviderMock.Object, _urlResolverMock.Object);
        }

        #region Helpers

        private void SetupEmptyDefaults()
        {
            _contentServiceMock
                .Setup(x => x.GetItemsChannelAsync<FooterLinks>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FooterLinks>());

            _contentServiceMock
                .Setup(x => x.GetItemsChannelAsync<SocialLinks>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SocialLinks>());

            _contentServiceMock
                .Setup(x => x.GetContentType<Subscribe>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<Subscribe>());

            _contentServiceMock
                .Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(new List<IWebPageFieldsSource>());

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<FooterPages>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FooterPages>());

            _urlResolverMock
                .Setup(x => x.GetRelativePath(It.IsAny<IWebPageFieldsSource>()))
                .Returns(string.Empty);
        }

        #endregion

        #region GetAsync – general

        [Test]
        public async Task GetAsync_ReturnsFooterDto_WithDefaultValues()
        {
            SetupEmptyDefaults();

            var result = await _service.GetAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.TypeOf<FooterDto>());
                Assert.That(result?.FooterLinks, Is.Not.Null);
                Assert.That(result?.SocialButtons, Is.Not.Null);
                Assert.That(result?.DisplaySubscriptionsSection, Is.True);
            });
        }

        [Test]
        public async Task GetAsync_WhenNoData_ReturnsEmptyCollections()
        {
            _contentServiceMock
                .Setup(x => x.GetItemsChannelAsync<FooterLinks>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<FooterLinks>)null!);

            _contentServiceMock
                .Setup(x => x.GetItemsChannelAsync<SocialLinks>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<SocialLinks>)null!);

            _contentServiceMock
                .Setup(x => x.GetContentType<Subscribe>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<Subscribe>());

            _contentServiceMock
                .Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync((IEnumerable<IWebPageFieldsSource>)null!);

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<FooterPages>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<FooterPages>)null!);

            var result = await _service.GetAsync(CancellationToken.None);

            Assert.That(result?.FooterLinks, Is.Empty);
            Assert.That(result?.SocialButtons, Is.Empty);
        }

        [Test]
        public async Task GetAsync_LimitsFooterLinks_To12()
        {
            var footerLinks = Enumerable.Range(1, 20)
                .Select(i => new FooterLinks { Title = $"Link {i}" })
                .ToList();

            _contentServiceMock
                .Setup(x => x.GetItemsChannelAsync<FooterLinks>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(footerLinks);

            _contentServiceMock
                .Setup(x => x.GetItemsChannelAsync<SocialLinks>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SocialLinks>());

            _contentServiceMock
                .Setup(x => x.GetContentType<Subscribe>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<Subscribe>());

            _contentServiceMock
                .Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(new List<IWebPageFieldsSource>());

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<FooterPages>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FooterPages>());

            var result = await _service.GetAsync(CancellationToken.None);

            Assert.That(result?.FooterLinks.Count, Is.LessThanOrEqualTo(12));
        }

        [Test]
        public async Task GetAsync_LimitsSocialButtons_To5()
        {
            var socialLinks = Enumerable.Range(1, 10)
                .Select(i => new SocialLinks { Alttext = $"Alt {i}" })
                .ToList();

            _contentServiceMock
                .Setup(x => x.GetItemsChannelAsync<SocialLinks>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(socialLinks);

            _contentServiceMock
                .Setup(x => x.GetItemsChannelAsync<FooterLinks>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FooterLinks>());

            _contentServiceMock
                .Setup(x => x.GetContentType<Subscribe>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<Subscribe>());

            _contentServiceMock
                .Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(new List<IWebPageFieldsSource>());

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<FooterPages>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FooterPages>());

            var result = await _service.GetAsync(CancellationToken.None);

            Assert.That(result?.SocialButtons.Count, Is.LessThanOrEqualTo(5));
        }

        [Test]
        public void GetAsync_WhenDependencyThrows_ExceptionIsPropagated()
        {
            _contentServiceMock
                .Setup(x => x.GetItemsChannelAsync<FooterLinks>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("service failure"));

            Assert.ThrowsAsync<Exception>(async () =>
                await _service.GetAsync(CancellationToken.None));
        }

        #endregion

        #region FooterLinkDto – DTO property coverage

        [Test]
        public void FooterLinkDto_DefaultValues_AreCorrect()
        {
            var dto = new FooterLinkDto();

            Assert.Multiple(() =>
            {
                Assert.That(dto.OpenInNewTab, Is.False);
                Assert.That(dto.Order, Is.EqualTo(0));
                Assert.That(dto.Title, Is.Null);
                Assert.That(dto.URL, Is.Null);
                Assert.That(dto.Target, Is.EqualTo("_self"));
            });
        }

        [Test]
        public void FooterLinkDto_SetProperties_RetainsValues()
        {
            var dto = new FooterLinkDto
            {
                Title = "Privacy Policy",
                URL = "/privacy",
                OpenInNewTab = true,
                Order = 3,
                Target = "_blank"
            };

            Assert.Multiple(() =>
            {
                Assert.That(dto.Title, Is.EqualTo("Privacy Policy"));
                Assert.That(dto.URL, Is.EqualTo("/privacy"));
                Assert.That(dto.OpenInNewTab, Is.True);
                Assert.That(dto.Order, Is.EqualTo(3));
                Assert.That(dto.Target, Is.EqualTo("_blank"));
            });
        }

        [Test]
        public void FooterLinkDto_IsSubclassOf_LinkItemDto()
        {
            var dto = new FooterLinkDto();

            Assert.That(dto, Is.InstanceOf<LinkItemDto>());
        }

        #endregion

        #region FooterLinkDto – mapped from FooterLinks channel items

        [Test]
        public async Task GetAsync_FooterLinks_Order_IsAssignedSequentially()
        {
            var footerLinks = Enumerable.Range(1, 3)
                .Select(i => new FooterLinks { Title = $"Link {i}", TargetURL = Enumerable.Empty<IWebPageFieldsSource>() })
                .ToList();

            _contentServiceMock
                .Setup(x => x.GetItemsChannelAsync<FooterLinks>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(footerLinks);

            _contentServiceMock
                .Setup(x => x.GetItemsChannelAsync<SocialLinks>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SocialLinks>());

            _contentServiceMock
                .Setup(x => x.GetContentType<Subscribe>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<Subscribe>());

            _contentServiceMock
                .Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(new List<IWebPageFieldsSource>());

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<FooterPages>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FooterPages>());

            var result = await _service.GetAsync(CancellationToken.None);

            var orders = result?.FooterLinks.Select(l => l.Order).ToList();
            Assert.That(orders, Is.Ordered.Ascending);
        }

        [Test]
        public async Task GetAsync_FooterLinks_WhenTargetUrlGuidIsEmpty_MapsEmptyUrl()
        {
            var footerLinks = new List<FooterLinks>
            {
                new FooterLinks
                {
                    Title = "No Target",
                    OpenInNewTab = false,
                    TargetURL = new List<IWebPageFieldsSource>
                    {
                        CreateWebPageSource(Guid.Empty, "/")
                    }
                }
            };

            _contentServiceMock
                .Setup(x => x.GetItemsChannelAsync<FooterLinks>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(footerLinks);

            _contentServiceMock
                .Setup(x => x.GetItemsChannelAsync<SocialLinks>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SocialLinks>());

            _contentServiceMock
                .Setup(x => x.GetContentType<Subscribe>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<Subscribe>());

            _contentServiceMock
                .Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(new List<IWebPageFieldsSource>());

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<FooterPages>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FooterPages>());

            var result = await _service.GetAsync(CancellationToken.None);

            var link = result?.FooterLinks.FirstOrDefault(l => l.Title == "No Target");
            Assert.That(link, Is.Not.Null);
            Assert.That(link!.URL, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task GetAsync_FooterLinks_WhenNullTitle_MapsFallbackEmpty()
        {
            var footerLinks = new List<FooterLinks>
            {
                new FooterLinks
                {
                    Title = null!,
                    OpenInNewTab = false,
                    TargetURL = Enumerable.Empty<IWebPageFieldsSource>()
                }
            };

            _contentServiceMock
                .Setup(x => x.GetItemsChannelAsync<FooterLinks>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(footerLinks);

            _contentServiceMock
                .Setup(x => x.GetItemsChannelAsync<SocialLinks>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SocialLinks>());

            _contentServiceMock
                .Setup(x => x.GetContentType<Subscribe>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<Subscribe>());

            _contentServiceMock
                .Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(new List<IWebPageFieldsSource>());

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<FooterPages>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FooterPages>());

            var result = await _service.GetAsync(CancellationToken.None);

            var link = result?.FooterLinks.First();
            Assert.That(link!.Title, Is.EqualTo(string.Empty));
        }

        #endregion

        #region FooterLinkDto – helper

        /// <summary>
        /// Creates a minimal <see cref="IWebPageFieldsSource"/> mock with the given GUID and relative path.
        /// </summary>
        private static IWebPageFieldsSource CreateWebPageSource(Guid guid, string relativePath)
        {
            var mock = new Mock<IWebPageFieldsSource>();
            mock.Setup(p => p.SystemFields).Returns(new WebPageFields
            {
                WebPageItemGUID = guid,
                WebPageUrlPath = relativePath
            });
            return mock.Object;
        }

        /// <summary>
        /// Creates a linked footer-link item that is both an <see cref="IWebPageFieldsSource"/>
        /// (so it survives the OfType filter) and an <see cref="IMetadata"/> (so MetadataTitle maps).
        /// </summary>
        private static IContentItemFieldsSource CreateMandatoryFooterLinkSource(string metadataTitle)
        {
            var mock = new Mock<IWebPageFieldsSource>();
            mock.As<IContentItemFieldsSource>();
            mock.As<IMetadata>()
                .Setup(m => m.MetadataTitle)
                .Returns(metadataTitle);

            return (IContentItemFieldsSource)mock.Object;
        }

        #endregion

        #region GetAsyncForAPI – general

        // NOTE: Kentico's ChannelInfo derives from AbstractInfoBase, whose property setters
        // (and its constructor's default-data load) call SetValue -> get_DataClass -> EnsureData,
        // which lazily resolves services from the CMS IoC container. That container is not
        // initialized in unit tests, so ANY normal instantiation throws ServiceResolutionException.
        //
        // To avoid this entirely we:
        //   1. Create the instance WITHOUT running any constructor (GetUninitializedObject), and
        //   2. Override the virtual ChannelName with a plain auto-property so reads/writes never
        //      touch AbstractInfoBase.GetValue/SetValue.
        private sealed class TestChannelInfo : ChannelInfo
        {
            public override string ChannelName { get; set; } = string.Empty;
        }

        private void SetupChannelInfo(string channelName = "TestChannel")
        {
            // Bypass the ChannelInfo/AbstractInfoBase constructor chain completely.
            var channelInfo = (TestChannelInfo)System.Runtime.CompilerServices.RuntimeHelpers
                .GetUninitializedObject(typeof(TestChannelInfo));

            // Safe: sets the override's auto-property backing field, not AbstractInfoBase.SetValue.
            channelInfo.ChannelName = channelName;

            _channelInfoProviderMock
                .As<IInfoByIdProvider<ChannelInfo>>()
                .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken?>()))
                .ReturnsAsync(channelInfo);
        }

        private void SetupEmptyDefaultsForAPI()
        {
            SetupChannelInfo();

            _contentServiceMock
                .Setup(x => x.GetItemsChannelOnChannelIdAsync<FooterLinks>(
                    It.IsAny<string>(),
                    It.IsAny<Action<ContentItemQueryBuilder>>(),                    
                    It.IsAny<int>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FooterLinks>());

            _contentServiceMock
                .Setup(x => x.GetItemsChannelOnChannelIdAsync<SocialLinks>(
                    It.IsAny<string>(),
                    It.IsAny<Action<ContentItemQueryBuilder>>(),                   
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SocialLinks>());

            _contentServiceMock
                .Setup(x => x.GetContentTypeForAPI<Subscribe>(                    
                    It.IsAny<string>(),
                    It.IsAny<string>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<Subscribe>());

            _contentServiceMock
                .Setup(x => x.GetItemsByGUIDForAPIAsync(
                    It.IsAny<List<Guid>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new List<IWebPageFieldsSource>());

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<FooterPages>(
                    It.IsAny<int>(),
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FooterPages>());
        }

        [Test]
        public async Task GetAsyncForAPI_ReturnsFooterDto_WithDefaultValues()
        {
            SetupEmptyDefaultsForAPI();

            var result = await _service.GetAsyncForAPI(1, "en-GB", CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.TypeOf<FooterDto>());
                Assert.That(result?.FooterLinks, Is.Not.Null);
                Assert.That(result?.SocialButtons, Is.Not.Null);
                Assert.That(result?.DisplaySubscriptionsSection, Is.True);
            });
        }

        [Test]
        public async Task GetAsyncForAPI_WhenNoData_ReturnsEmptyCollections()
        {
            SetupChannelInfo();

            _contentServiceMock
                .Setup(x => x.GetItemsChannelOnChannelIdAsync<FooterLinks>(
                    It.IsAny<string>(),
                    It.IsAny<Action<ContentItemQueryBuilder>>(),                    
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<FooterLinks>)null!);

            _contentServiceMock
                .Setup(x => x.GetItemsChannelOnChannelIdAsync<SocialLinks>(
                    It.IsAny<string>(),
                    It.IsAny<Action<ContentItemQueryBuilder>>(),
                    It.IsAny<int>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<SocialLinks>)null!);

            _contentServiceMock
                .Setup(x => x.GetContentTypeForAPI<Subscribe>(                    
                    It.IsAny<string>(),
                    It.IsAny<string>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<Subscribe>());

            _contentServiceMock
                .Setup(x => x.GetItemsByGUIDForAPIAsync(
                    It.IsAny<List<Guid>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((IEnumerable<IWebPageFieldsSource>)null!);

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<FooterPages>(
                    It.IsAny<int>(),
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<FooterPages>)null!);

            var result = await _service.GetAsyncForAPI(1, "en-GB", CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.That(result?.FooterLinks, Is.Empty);
                Assert.That(result?.SocialButtons, Is.Empty);
            } );
        }

        [Test]
        public async Task GetAsyncForAPI_LimitsFooterLinks_To12()
        {
            SetupEmptyDefaultsForAPI();

            var footerLinks = Enumerable.Range(1, 20)
                .Select(i => new FooterLinks { Title = $"Link {i}" })
                .ToList();

            _contentServiceMock
                .Setup(x => x.GetItemsChannelOnChannelIdAsync<FooterLinks>(
                    It.IsAny<string>(),
                    It.IsAny<Action<ContentItemQueryBuilder>>(),                    
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(footerLinks);

            var result = await _service.GetAsyncForAPI(1, "en-GB", CancellationToken.None);

            Assert.That(result?.FooterLinks.Count, Is.LessThanOrEqualTo(12));
        }

        [Test]
        public async Task GetAsyncForAPI_LimitsSocialButtons_To5()
        {
            SetupEmptyDefaultsForAPI();

            var socialLinks = Enumerable.Range(1, 10)
                .Select(i => new SocialLinks { Alttext = $"Alt {i}" })
                .ToList();

            _contentServiceMock
                .Setup(x => x.GetItemsChannelOnChannelIdAsync<SocialLinks>(
                    It.IsAny<string>(),
                    It.IsAny<Action<ContentItemQueryBuilder>>(),                    
                    It.IsAny<int>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(socialLinks);

            var result = await _service.GetAsyncForAPI(1, "en-GB", CancellationToken.None);

            Assert.That(result?.SocialButtons.Count, Is.LessThanOrEqualTo(5));
        }

        [Test]
        public void GetAsyncForAPI_WhenDependencyThrows_ExceptionIsPropagated()
        {
            SetupChannelInfo();

            _contentServiceMock
                .Setup(x => x.GetItemsChannelOnChannelIdAsync<FooterLinks>(
                    It.IsAny<string>(),
                    It.IsAny<Action<ContentItemQueryBuilder>>(),                   
                    It.IsAny<int>(), 
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("service failure"));

            Assert.ThrowsAsync<Exception>(async () =>
                await _service.GetAsyncForAPI(1, "en-GB", CancellationToken.None));
        }

        [Test]
        public async Task GetAsyncForAPI_WhenCultureIsNull_UsesDefaultCulture()
        {
            SetupEmptyDefaultsForAPI();

            var result = await _service.GetAsyncForAPI(1, null, CancellationToken.None);

            Assert.That(result, Is.Not.Null);

            // FooterService applies the "GB" default culture fallback (culture ?? defaultCulture.Language)
            // only when building the mandatory footer links, i.e. on the RetrievePagesParameters.LanguageName
            // passed to GetPagesAsync<FooterPages>. The raw (null) culture flows to the other calls.
            _contentServiceMock.Verify(
                x => x.GetPagesAsync<FooterPages>(
                    It.IsAny<int>(),
                    It.Is<RetrievePagesParameters>(p => p.LanguageName == "en"),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetAsyncForAPI_PassesChannelIdAndCulture_ToDependencies()
        {
            SetupEmptyDefaultsForAPI();

            var result = await _service.GetAsyncForAPI(42, "fr-FR", CancellationToken.None);

            Assert.That(result, Is.Not.Null);

            _contentServiceMock.Verify(
                x => x.GetItemsChannelOnChannelIdAsync<FooterLinks>(
                    It.Is<string>(c => c == "fr-FR"),
                    It.IsAny<Action<ContentItemQueryBuilder>>(),                    
                    It.Is<int>(id => id == 42),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _contentServiceMock.Verify(
                x => x.GetItemsChannelOnChannelIdAsync<SocialLinks>(
                    It.Is<string>(c => c == "fr-FR"),
                    It.IsAny<Action<ContentItemQueryBuilder>>(),                    
                    It.Is<int>(id => id == 42),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region GetAsyncForAPI – mandatory footer links from page items

        [Test]
        public async Task GetAsyncForAPIMapsMandatoryFooterLinksFromFooterPages()
        {
            SetupEmptyDefaultsForAPI();

            var pages = new List<FooterPages>
            {
                new FooterPages
                {
                    Title = "Root",
                    FooterLink = new List<IContentItemFieldsSource>
                    {
                        CreateMandatoryFooterLinkSource("Privacy Policy")
                    }
                }
            };

            _urlResolverMock
                .Setup(x => x.GetRelativePath(It.IsAny<IWebPageFieldsSource>()))
                .Returns("/privacy");

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<FooterPages>(
                    It.IsAny<int>(),
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pages);

            var result = await _service.GetAsyncForAPI(1, "en-GB", CancellationToken.None);

            var mandatory = result?.FooterLinks.FirstOrDefault(l => l.Title == "Privacy Policy");
            Assert.That(mandatory, Is.Not.Null);
            Assert.That(mandatory!.URL, Is.EqualTo("/privacy"));
        }

        
       

        [Test]
        public async Task GetAsyncForAPI_MandatoryFooterLinks_UsesFallbacks_WhenTitleNullAndNoUrl()
        {
            SetupEmptyDefaultsForAPI();

            var pages = new List<FooterPages>
            {
                new FooterPages
                {
                    Title = "Root",
                    FooterLink = new List<IContentItemFieldsSource>
                    {
                        CreateMandatoryFooterLinkSource(null!)
                    }
                }
            };

            // GetRelativePath returns null -> URL should fall back to string.Empty.
            _urlResolverMock
                .Setup(x => x.GetRelativePath(It.IsAny<IWebPageFieldsSource>()))
                .Returns((string)null!);

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<FooterPages>(
                    It.IsAny<int>(),
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pages);

            var result = await _service.GetAsyncForAPI(1, "en-GB", CancellationToken.None);

            var mandatory = result?.FooterLinks.FirstOrDefault();
            Assert.Multiple(() =>
            {
                Assert.That(mandatory, Is.Not.Null);
                Assert.That(mandatory!.Title, Is.EqualTo(string.Empty));
                Assert.That(mandatory!.URL, Is.EqualTo(string.Empty));
            });
        }

        #endregion
    }
}