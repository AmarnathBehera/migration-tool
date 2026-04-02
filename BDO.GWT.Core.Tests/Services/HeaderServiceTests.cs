using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Header;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Services.Header;
using CMS.Websites;
using CMS.Websites.Routing;
using GreenDonut;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services
{
    public class HeaderServiceTests
    {
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IContentRetriever> _contentRetrieverMock = null!;
        private Mock<IHomePageService> _homePageServiceMock = null!;
        private Mock<IWebsiteCulturesService> _websiteCulturesServiceMock = null!;
        private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
        private HeaderService _service = null!;

        [SetUp]
        public void Setup()
        {
            _contentServiceMock = new Mock<IContentService>();
            _contentRetrieverMock = new Mock<IContentRetriever>();
            _homePageServiceMock = new Mock<IHomePageService>();
            _websiteCulturesServiceMock = new Mock<IWebsiteCulturesService>();
            _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();

            _service = new HeaderService(
                _contentServiceMock.Object,
                _contentRetrieverMock.Object,
                _homePageServiceMock.Object,
                _websiteCulturesServiceMock.Object,
                _websiteChannelContextMock.Object
            );
        }

        [Test]
        public void GetAsync_WhenExceptionThrown_PropagatesException()
        {
            _websiteCulturesServiceMock
                .Setup(x => x.GetWebsiteCulturesAsync())
                .ThrowsAsync(new Exception());

            Assert.ThrowsAsync<Exception>(async () =>
                await _service.GetAsync(_contentServiceMock.Object, CancellationToken.None));
        }

        [Test]
        public async Task GetAsync_ReturnsHeaderDto_WithExpectedValues()
        {
            // Arrange
            var fakeHomePageUrl = "/home";
            _homePageServiceMock
                .Setup(x => x.GetHomePageUrl(It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeHomePageUrl);

            _websiteCulturesServiceMock
                .Setup(x => x.GetWebsiteCulturesAsync())
                .ReturnsAsync(new List<CultureDto>
                {
                    new() { ShortName = "en", Name = "English" },
                    new() { ShortName = "lt", Name = "Lithuanian" }
                });

            _websiteCulturesServiceMock
                .Setup(x => x.GetDisplayName(_websiteChannelContextMock.Object))
                .Returns("BDO Global");

            _contentServiceMock
                .Setup(x => x.GetContentType<NavigationFirstLevelItem>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NavigationFirstLevelItem> { new() { Title = "FirstLevel" } });

            // Act
            var headerDto = await _service.GetAsync(_contentServiceMock.Object, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                // Basic info
                Assert.That(headerDto.SiteName, Is.EqualTo("BDO Global"));
                Assert.That(headerDto.LogoURL, Is.EqualTo(fakeHomePageUrl));
                Assert.That(headerDto.LogoImageURL, Is.Not.Null.And.Not.Empty);

                // Cultures
                Assert.That(headerDto.Cultures.ElementAt(0).Name, Is.EqualTo("English"));
                Assert.That(headerDto.Cultures.ElementAt(0).ShortName, Is.EqualTo("en"));
                Assert.That(headerDto.Cultures.ElementAt(1).Name, Is.EqualTo("Lithuanian"));
                Assert.That(headerDto.Cultures.ElementAt(1).ShortName, Is.EqualTo("lt"));

                // Mega menu
                Assert.That(headerDto.MegaMenuItems, Is.Not.Null.And.Not.Empty);
                Assert.That(headerDto.MegaMenuItems.First().Title, Is.EqualTo("FirstLevel"));

                // Other collections
                Assert.That(headerDto.MenuItems, Is.Not.Null);
                Assert.That(headerDto.Contact, Is.Null.Or.InstanceOf<LinkItemDto>());
            });
        }

        [Test]
        public async Task GetLogoPageLinkAsync_ReturnsEmpty_WhenHomePageIsNull()
        {
            // Arrange
            _homePageServiceMock
                .Setup(x => x.GetHomePageUrl(It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            // Act
            var method = _service.GetType()
    .GetMethod("GetLogoPageLinkAsync",
               System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            // Cast to Task<string>
            var task = (Task<string>)method.Invoke(_service, new object[] { CancellationToken.None })!;
            var result = await task;

            // Assert
            Assert.That(result, Is.EqualTo(string.Empty));
        }
        [Test]
        public async Task BuildMegaMenuAsync_ReturnsFirstLevelMenu()
        {
            // Arrange
            var firstLevelItem = new NavigationFirstLevelItem { Title = "FirstLevel" };
            _contentServiceMock
                .Setup(s => s.GetContentType<NavigationFirstLevelItem>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NavigationFirstLevelItem> { firstLevelItem });

            _contentServiceMock
                .Setup(s => s.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(Enumerable.Empty<IWebPageFieldsSource>());

            // Act
            var method = _service.GetType()
                .GetMethod("BuildMegaMenuAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            var task = (Task<IEnumerable<FirstLevelMenuItemDto>>)method.Invoke(_service, new object[] { CancellationToken.None })!;
            var megaMenu = await task;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(megaMenu, Is.Not.Null);
                Assert.That(megaMenu.Any(), Is.True);
                Assert.That(megaMenu.First().Title, Is.EqualTo("FirstLevel"));
                Assert.That(megaMenu.First().Children, Is.Empty);
                Assert.That(megaMenu.First().Featured, Is.Null.Or.Empty);
            });
        }
    }
}