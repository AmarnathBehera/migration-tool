using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services
{
    public class HomePageServiceTests
    {
        private Mock<IContentService> _contentServiceMock;
        private Mock<IWebsiteChannelContext> _channelMock;

        private HomePageService _service;

        [SetUp]
        public void Setup()
        {
            _contentServiceMock = new Mock<IContentService>();
            _channelMock = new Mock<IWebsiteChannelContext>();
            _channelMock
                 .Setup(x => x.WebsiteChannelID)
                 .Returns(1);
            _service = new HomePageService(
                _contentServiceMock.Object,
                _channelMock.Object
            );
        }

        [Test]
        public async Task RetrieveHomeHeroSlidesDtoAsync_ReturnsMappedData()
        {
            var slides = new List<HomeHeroSlide>
            {
                new HomeHeroSlide
                {
                    Title = "Slide1",
                    Description = "Desc1",
                    CTAButtonText = "Click",
                    CTAButtonUrl = "/test"
                }
            };

            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<HomeHeroSlide>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(slides);

            var result = await _service.RetrieveHomeHeroSlidesDtoAsync(new HomePage(), CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));

            var item = result.First();

            Assert.That(item?.Title, Is.EqualTo("Slide1"));
            Assert.That(item?.Description, Is.EqualTo("Desc1"));
            Assert.That(item?.Button?.Title, Is.EqualTo("Click"));
            Assert.That(item?.Button?.URL, Is.EqualTo("/test"));
        }

        [Test]
        public async Task RetrieveHomeHeroSlidesDtoAsync_ReturnsNull_WhenEmpty()
        {
            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<HomeHeroSlide>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HomeHeroSlide>());

            var result = await _service.RetrieveHomeHeroSlidesDtoAsync(new HomePage(), CancellationToken.None);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetHomePageUrl_ReturnsNull_WhenNoData()
        {
            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<HomePage>(It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<HomePage>)null);

            var result = await _service.GetHomePageUrl(CancellationToken.None);

            Assert.That(result, Is.Null);
        }
        [Test]
        public async Task RetrieveHomeHeroSlidesDtoAsync_ReturnsNull_OnException()
        {
            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<HomeHeroSlide>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            var result = await _service.RetrieveHomeHeroSlidesDtoAsync(new HomePage(), CancellationToken.None);

            Assert.That(result, Is.Null);
        }
        [Test]
        public async Task RetrieveHomeHeroSlidesDtoAsync_PartialData_ReturnsNullButtonAsync()
        {
            var slides = new List<HomeHeroSlide>
    {
        new HomeHeroSlide
        {
            Title = "Slide1",
            CTAButtonText = "Click",
            CTAButtonUrl = null
        }
    };

            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<HomeHeroSlide>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(slides);

            var result = await _service.RetrieveHomeHeroSlidesDtoAsync(new HomePage(), CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.First()?.Button, Is.Null);
        }
        [Test]
        public async Task RetrieveHomeHeroSlidesDtoAsync_HandlesNullImages()
        {
            var slides = new List<HomeHeroSlide>
    {
        new HomeHeroSlide
        {
            Title = "Slide1",
            Image_Mobile = null,
            Image_Thumbnail = null
        }
    };

            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<HomeHeroSlide>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(slides);

            var result = await _service.RetrieveHomeHeroSlidesDtoAsync(new HomePage(), CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.First()?.ImageURL, Is.Null);
        }
        
        [Test]
        public async Task GetHomePageUrl_Throws_WhenEmptyList()
        {
            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<HomePage>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HomePage>());

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.GetHomePageUrl(CancellationToken.None));
        }
        [Test]
        public async Task GetHomePageDtoAsync_ReturnsNull_WhenNoHomePage()
        {
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<HomePage>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HomePage>());

            var result = await _service.GetHomePageDtoAsync(null, CancellationToken.None);

            Assert.That(result, Is.Null);
        }


        [Test]
        public async Task GetHomePageDtoAsync_MapsAllFieldsCorrectly()
        {
            var page = new HomePage
            {
                SystemFields = new WebPageFields
                {
                    ContentItemID = 1,
                    ContentItemName = "Home",
                    WebPageItemID = 10,
                    WebPageUrlPath = "/home",
                    WebPageItemParentID = 5
                }
            };

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<HomePage>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HomePage> { page });

            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<HomeHeroSlide>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HomeHeroSlide>());

            var result = await _service.GetHomePageDtoAsync(null, CancellationToken.None);

            // ✅ Core
            Assert.That(result, Is.Not.Null);

            // ✅ BaseContent
            Assert.That(result?.BaseContent?.ContentItemID, Is.EqualTo(1));
            Assert.That(result?.BaseContent?.ContentItemName, Is.EqualTo("Home"));

            // ✅ BaseWebPage
            Assert.That(result?.BaseWebPage?.WebPageItemID, Is.EqualTo(10));
            Assert.That(result?.BaseWebPage?.WebPageUrlPath, Is.EqualTo("/home"));
            Assert.That(result?.BaseWebPage?.WebPageItemParentID, Is.EqualTo(5));
        }
    }
}