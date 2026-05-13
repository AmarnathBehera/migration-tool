using CMS.ContentEngine;
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

        [Test]
        public async Task RetrieveTrendingTopicsDtoAsync_ReturnsNull_WhenTrendingTopicsPageIsNull()
        {
            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<TrendingTopics>(It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<TrendingTopics>?)null);

            var result = await _service.RetrieveTrendingTopicsDtoAsync(CancellationToken.None);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task RetrieveTrendingTopicsDtoAsync_ReturnsNull_WhenTrendingTopicPagesIsNull()
        {
            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<TrendingTopics>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TrendingTopics> { new TrendingTopics { Heading = "Topics" } });

            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<TrendingTopic>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<TrendingTopic>?)null);

            var result = await _service.RetrieveTrendingTopicsDtoAsync(CancellationToken.None);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task RetrieveTrendingTopicsDtoAsync_ReturnsMappedDto_WithIndustryFeaturedItem()
        {
            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<TrendingTopics>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TrendingTopics>
                {
                    new TrendingTopics { Heading = "Trending" }
                });

            var IndustryItem = new Industry
            {
                MetadataTitle = "Industry Title",
                MetadataDescription = "Industry Desc",
                SystemFields = new WebPageFields { WebPageUrlPath = "/industry" }
            };

            var trendingTopics = new List<TrendingTopic>
            {
                new TrendingTopic
                {
                    Title = "Tab1",
                    ImageAltText = "Alt1",
                    ImageUrl = new List<ContentAttachment>
                    {
                        new ContentAttachment
                        {
                            ContentAttachmentAsset = new ContentItemAsset { Url = "/img.png" }
                        }
                    },
                    CTANodeGuid = new List<IWebPageFieldsSource>
                    {
                        new Industry { SystemFields = new WebPageFields { WebPageUrlPath = "/read-more" } }
                    },
                    FeaturedItems = new List<IWebPageFieldsSource> { IndustryItem }
                }
            };

            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<TrendingTopic>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(trendingTopics);

            var result = await _service.RetrieveTrendingTopicsDtoAsync(CancellationToken.None);


            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.Heading, Is.EqualTo("Trending"));
                Assert.That(result?.Tabs.Count(), Is.EqualTo(1));
            });

            

            var tab = result.Tabs.First();

            Assert.Multiple(() =>
            {
                Assert.That(tab.Title, Is.EqualTo("Tab1"));
                Assert.That(tab.ImageURL, Is.EqualTo("/img.png"));
                Assert.That(tab.ImageAltText, Is.EqualTo("Alt1"));
                Assert.That(tab.ReadMoreLink, Is.EqualTo("/read-more"));
                Assert.That(tab.Items, Has.Count.EqualTo(1));
                Assert.That(tab.Items[0].Title, Is.EqualTo("Industry Title"));
                Assert.That(tab.Items[0].Description, Is.EqualTo("Industry Desc"));
                Assert.That(tab.Items[0].ReadMoreLink, Is.EqualTo("/industry"));
            });
            
        }

        [Test]
        public async Task RetrieveTrendingTopicsDtoAsync_ReturnsMappedDto_WithServicePageFeaturedItem()
        {
            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<TrendingTopics>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TrendingTopics>
                {
                    new TrendingTopics { Heading = "Trending" }
                });

            var ServiceItem = new Service
            {
                MetadataTitle = "Service Title",
                MetadataDescription = "Service Desc",
                SystemFields = new WebPageFields { WebPageUrlPath = "/service" }
            };

            var trendingTopics = new List<TrendingTopic>
            {
                new TrendingTopic
                {
                    Title = "Tab2",
                    ImageAltText = "Alt2",
                    ImageUrl = new List<ContentAttachment>(),
                    CTANodeGuid = new List<IWebPageFieldsSource>(),
                    FeaturedItems = new List<IWebPageFieldsSource> { ServiceItem }
                }
            };

            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<TrendingTopic>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(trendingTopics);

            var result = await _service.RetrieveTrendingTopicsDtoAsync(CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            var tab = result.Tabs.First();
            Assert.Multiple(() =>
            {
                Assert.That(tab.Items[0].Title, Is.EqualTo("Service Title"));
                Assert.That(tab.Items[0].Description, Is.EqualTo("Service Desc"));
                Assert.That(tab.Items[0].ReadMoreLink, Is.EqualTo("/service"));
            });
        }

        [Test]
        public async Task RetrieveTrendingTopicsDtoAsync_ReturnsMappedDto_WithUnknownFeaturedItem()
        {
            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<TrendingTopics>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TrendingTopics>
                {
                    new TrendingTopics { Heading = "Trending" }
                });

            var unknownItem = new Mock<IWebPageFieldsSource>();
            unknownItem.Setup(x => x.SystemFields).Returns(new WebPageFields
            {
                WebPageItemName = "Unknown Page",
                WebPageUrlPath = "/unknown"
            });

            var trendingTopics = new List<TrendingTopic>
            {
                new TrendingTopic
                {
                    Title = "Tab3",
                    ImageAltText = "Alt3",
                    ImageUrl = new List<ContentAttachment>(),
                    CTANodeGuid = new List<IWebPageFieldsSource>(),
                    FeaturedItems = new List<IWebPageFieldsSource> { unknownItem.Object }
                }
            };

            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<TrendingTopic>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(trendingTopics);

            var result = await _service.RetrieveTrendingTopicsDtoAsync(CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            var tab = result.Tabs.First();

            Assert.Multiple(() =>
            {
                Assert.That(tab.Items[0].Title, Is.EqualTo("Unknown Page"));
                Assert.That(tab.Items[0].ReadMoreLink, Is.EqualTo("/unknown"));
            });
            
        }
        [Test]
        public async Task RetrieveTrendingTopicsDtoAsync_ReturnsMappedDto_WithIndustryServiceFeaturedItem()
        {
            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<TrendingTopics>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TrendingTopics>
                {
            new TrendingTopics { Heading = "Trending" }
                });

            var industryServiceItem = new IndustryService
            {
                MetadataTitle = "Industry Service Title",
                MetadataDescription = "<p>Industry Service Desc</p>",
                SystemFields = new WebPageFields { WebPageUrlPath = "/industry-service" }
            };

            var trendingTopics = new List<TrendingTopic>
    {
        new TrendingTopic
        {
            Title = "Industry Service Tab",
            ImageAltText = "Alt",
            ImageUrl = new List<ContentAttachment>(),
            CTANodeGuid = new List<IWebPageFieldsSource>(),
            FeaturedItems = new List<IWebPageFieldsSource> { industryServiceItem }
        }
    };

            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<TrendingTopic>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(trendingTopics);

            var result = await _service.RetrieveTrendingTopicsDtoAsync(CancellationToken.None);

            Assert.That(result, Is.Not.Null);

            var tab = result.Tabs.First();

            Assert.Multiple(() =>
            {
                Assert.That(tab.Items[0].Title, Is.EqualTo("Industry Service Title"));
                Assert.That(tab.Items[0].Description, Is.EqualTo("Industry Service Desc"));
                Assert.That(tab.Items[0].ReadMoreLink, Is.EqualTo("/industry-service"));
            });

            
        }

        [Test]
        public async Task RetrieveTrendingTopicsDtoAsync_ReturnsMappedDto_WithServiceAreaFeaturedItem()
        {
            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<TrendingTopics>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TrendingTopics>
                {
            new TrendingTopics { Heading = "Trending" }
                });

            var serviceAreaItem = new ServiceArea
            {
                MetadataTitle = "Service Area Title",
                MetadataDescription = "<p>Service Area Desc</p>",
                SystemFields = new WebPageFields { WebPageUrlPath = "/service-area" }
            };

            var trendingTopics = new List<TrendingTopic>
    {
        new TrendingTopic
        {
            Title = "Service Area Tab",
            ImageAltText = "Alt",
            ImageUrl = new List<ContentAttachment>(),
            CTANodeGuid = new List<IWebPageFieldsSource>(),
            FeaturedItems = new List<IWebPageFieldsSource> { serviceAreaItem }
        }
    };

            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<TrendingTopic>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(trendingTopics);

            var result = await _service.RetrieveTrendingTopicsDtoAsync(CancellationToken.None);

            Assert.That(result, Is.Not.Null);

            var tab = result.Tabs.First();
            Assert.Multiple(() =>
            {
                Assert.That(tab.Items[0].Title, Is.EqualTo("Service Area Title"));
                Assert.That(tab.Items[0].Description, Is.EqualTo("Service Area Desc"));
                Assert.That(tab.Items[0].ReadMoreLink, Is.EqualTo("/service-area"));
            });

            
        }

        [Test]
        public async Task RetrieveTrendingTopicsDtoAsync_ReturnsMappedDto_WithSubServiceFeaturedItem()
        {
            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<TrendingTopics>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TrendingTopics>
                {
            new TrendingTopics { Heading = "Trending" }
                });

            var subServiceItem = new Sub_Service
            {
                MetadataTitle = "Sub Service Title",
                MetadataDescription = "<p>Sub Service Desc</p>",
                SystemFields = new WebPageFields { WebPageUrlPath = "/sub-service" }
            };

            var trendingTopics = new List<TrendingTopic>
    {
        new TrendingTopic
        {
            Title = "Sub Service Tab",
            ImageAltText = "Alt",
            ImageUrl = new List<ContentAttachment>(),
            CTANodeGuid = new List<IWebPageFieldsSource>(),
            FeaturedItems = new List<IWebPageFieldsSource> { subServiceItem }
        }
    };

            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<TrendingTopic>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(trendingTopics);

            var result = await _service.RetrieveTrendingTopicsDtoAsync(CancellationToken.None);

            Assert.That(result, Is.Not.Null);

            var tab = result.Tabs.First();
            Assert.Multiple(() =>
            {
                Assert.That(tab.Items[0].Title, Is.EqualTo("Sub Service Title"));
                Assert.That(tab.Items[0].Description, Is.EqualTo("Sub Service Desc"));
                Assert.That(tab.Items[0].ReadMoreLink, Is.EqualTo("/sub-service"));
            });
            
        }

        [Test]
        public async Task RetrieveTrendingTopicsDtoAsync_ReturnsEmptyTabs_WhenNoTrendingTopicPages()
        {
            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<TrendingTopics>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TrendingTopics>
                {
                    new TrendingTopics { Heading = "Trending" }
                });

            _contentServiceMock
                .Setup(x => x.RetrievePagesAsync<TrendingTopic>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TrendingTopic>());

            var result = await _service.RetrieveTrendingTopicsDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.Heading, Is.EqualTo("Trending"));
                Assert.That(result?.Tabs.Count(), Is.EqualTo(0));
            });
            
        }
    }
}