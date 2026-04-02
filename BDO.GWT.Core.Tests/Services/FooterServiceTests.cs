using AngleSharp.Dom;
using BDO.GWT.Core.DTOs.Footer;
using BDO.GWT.Core.Services.Footer;
using BDO.GWT.Core.ValueObjects;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services
{
    public class FooterServiceTests
    {
        private Mock<IContentService> _contentServiceMock = null!;
        private FooterService _service = null!;

        [SetUp]
        public void Setup()
        {
            _contentServiceMock = new Mock<IContentService>();
            _service = new FooterService(_contentServiceMock.Object);
        }

        [Test]
        public async Task GetAsync_ReturnsFooterDto_WithDefaultValues()
        {
            // Arrange
            _contentServiceMock.Setup(x => x.GetItemsAsync<FooterLinks>())
                .ReturnsAsync(new List<FooterLinks>());

            _contentServiceMock.Setup(x => x.GetItemsAsync<SocialLinks>())
                .ReturnsAsync(new List<SocialLinks>());

            _contentServiceMock.Setup(x => x.GetContentType<Subscribe>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<Subscribe>());

            _contentServiceMock.Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(new List<IWebPageFieldsSource>());

            _contentServiceMock.Setup(x => x.GetPagesAsync<FooterPages>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FooterPages>());

            // Act
            var result = await _service.GetAsync(CancellationToken.None);

            // Assert
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
            _contentServiceMock.Setup(x => x.GetItemsAsync<FooterLinks>()).ReturnsAsync((IReadOnlyList<FooterLinks>)null!);
            _contentServiceMock.Setup(x => x.GetItemsAsync<SocialLinks>()).ReturnsAsync((IReadOnlyList<SocialLinks>)null!);
            _contentServiceMock.Setup(x => x.GetContentType<Subscribe>(It.IsAny<CancellationToken>())).ReturnsAsync(Enumerable.Empty<Subscribe>());
            _contentServiceMock.Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>())).ReturnsAsync((IEnumerable<IWebPageFieldsSource>)null!);
            _contentServiceMock.Setup(x => x.GetPagesAsync<FooterPages>(
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
            // Arrange
            var footerLinks = Enumerable.Range(1, 20)
                .Select(i => new FooterLinks { Title = $"Link {i}" })
                .ToList();

            _contentServiceMock.Setup(x => x.GetItemsAsync<FooterLinks>()).ReturnsAsync(footerLinks);
            _contentServiceMock.Setup(x => x.GetItemsAsync<SocialLinks>()).ReturnsAsync(new List<SocialLinks>());
            _contentServiceMock.Setup(x => x.GetContentType<Subscribe>(It.IsAny<CancellationToken>())).ReturnsAsync(Enumerable.Empty<Subscribe>());
            _contentServiceMock.Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>())).ReturnsAsync(new List<IWebPageFieldsSource>());
            _contentServiceMock.Setup(x => x.GetPagesAsync<FooterPages>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FooterPages>());

            // Act
            var result = await _service.GetAsync(CancellationToken.None);

            // Assert
            Assert.That(result?.FooterLinks.Count, Is.LessThanOrEqualTo(12));
        }

        [Test]
        public async Task GetAsync_LimitsSocialButtons_To5()
        {
            // Arrange
            var socialLinks = Enumerable.Range(1, 10)
                .Select(i => new SocialLinks { Alttext = $"Alt {i}" })
                .ToList();

            _contentServiceMock.Setup(x => x.GetItemsAsync<SocialLinks>()).ReturnsAsync(socialLinks);
            _contentServiceMock.Setup(x => x.GetItemsAsync<FooterLinks>()).ReturnsAsync(new List<FooterLinks>());
            _contentServiceMock.Setup(x => x.GetContentType<Subscribe>(It.IsAny<CancellationToken>())).ReturnsAsync(Enumerable.Empty<Subscribe>());
            _contentServiceMock.Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>())).ReturnsAsync(new List<IWebPageFieldsSource>());
            _contentServiceMock.Setup(x => x.GetPagesAsync<FooterPages>(
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
            _contentServiceMock.Setup(x => x.GetItemsAsync<FooterLinks>()).ThrowsAsync(new Exception());

            Assert.ThrowsAsync<Exception>(async () =>
                await _service.GetAsync(CancellationToken.None));
        }
    }
}