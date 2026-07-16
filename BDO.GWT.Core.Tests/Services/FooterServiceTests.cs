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
        private FooterService _service = null!;

        [SetUp]
        public void Setup()
        {
            _contentServiceMock = new Mock<IContentService>();
            _channelInfoProviderMock = new Mock<IInfoProvider<ChannelInfo>>();
            _service = new FooterService(_contentServiceMock.Object, _channelInfoProviderMock.Object);
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

        #endregion
    }
}