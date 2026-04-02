using BDO.GWT.Core;
using CMS.ContentEngine;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Tests
{
    [TestFixture]
    public class HomePageServiceTests
    {
        private Mock<IContentService> _contentServiceMock;
        private Mock<IContentQueryExecutor> _executorMock;
        private Mock<IWebPageDataContextRetriever> _contextMock;
        private Mock<IWebsiteChannelContext> _channelMock;

        private HomePageService _service;

        [SetUp]
        public void Setup()
        {
            _contentServiceMock = new Mock<IContentService>();
            _executorMock = new Mock<IContentQueryExecutor>();
            _contextMock = new Mock<IWebPageDataContextRetriever>();
            _channelMock = new Mock<IWebsiteChannelContext>();

            _service = new HomePageService(
                _contentServiceMock.Object,
                _executorMock.Object,
                _contextMock.Object,
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
    }
}