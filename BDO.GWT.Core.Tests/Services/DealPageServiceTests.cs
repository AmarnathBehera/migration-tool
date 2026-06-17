using BDO;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.DTOs.Pages;
using BDO.GWT.Web.Core.LiveSite.Services;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Tests.Services
{
    public class DealPageServiceTests
    {
        private static readonly string[] ExpectedDealTypeNames = ["M&A"];
        private static readonly string[] ExpectedIndustryNames = ["Finance", "Tech"];
        private static readonly string[] ExpectedSubIndustryNames = ["Banking"];
        private static readonly string[] ExpectedRangeNames = ["Large"];

        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<ISidebarService> _sidebarServiceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
        private Mock<IWebPageDataContextRetriever> _webPageDataContextRetrieverMock = null!;
        private DealPageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _heroServiceMock = new Mock<IHeroService>();
            _sidebarServiceMock = new Mock<ISidebarService>();
            _contentServiceMock = new Mock<IContentService>();
            _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
            _webPageDataContextRetrieverMock = new Mock<IWebPageDataContextRetriever>();

            _websiteChannelContextMock.SetupGet(x => x.WebsiteChannelID).Returns(1);

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HeroDto { Title = "Hero" });

            _service = new DealPageService(
                _heroServiceMock.Object,
                _sidebarServiceMock.Object,
                _contentServiceMock.Object,
                _websiteChannelContextMock.Object,
                _webPageDataContextRetrieverMock.Object);
        }

        [Test]
        public async Task GetDealPageDtoAsync_ReturnsEmptyDto_WhenNoDealPageFound()
        {
            SetupDealPages(Enumerable.Empty<Deal>());

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Title, Is.Null);
                Assert.That(result.ClientName, Is.Null);
                Assert.That(result.Content, Is.Null);
                Assert.That(result.Sidebar, Is.Null);
                Assert.That(result.DealTypeNames, Is.Empty);
                Assert.That(result.IndustryNames, Is.Empty);
                Assert.That(result.SubIndustryNames, Is.Empty);
                Assert.That(result.Range, Is.Empty);
            });

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _sidebarServiceMock.Verify(
                x => x.GetDealSidebarDtoAsync(It.IsAny<Deal>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetDealPageDtoAsync_ReturnsNullDto_WhenServiceReturnsNull()
        {
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Deal>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<System.Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<Deal>?)null!);

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Title, Is.Null);
            });
        }

        [Test]
        public async Task GetDealPageDtoAsync_MapsScalarFields_WhenDealPageExists()
        {
            var deal = CreateDeal(showTitle: true, showSidebar: false);
            SetupDealPages(new[] { deal });

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.EqualTo("Meta Title"));
                Assert.That(result.ClientName, Is.EqualTo("Acme Inc."));
                Assert.That(result.Content, Is.Not.Null);
                Assert.That(result.Hero, Is.Not.Null);
                Assert.That(result.Hero.Title, Is.EqualTo("Hero"));
                Assert.That(result.Sidebar, Is.Null);
            });
        }

        [Test]
        public async Task GetDealPageDtoAsync_TitleIsNull_WhenMetadataShowTitleFalse()
        {
            var deal = CreateDeal(showTitle: false, showSidebar: false);
            SetupDealPages(new[] { deal });

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.That(result.Title, Is.Null);
        }

        [Test]
        public async Task GetDealPageDtoAsync_MapsLinkedItemNames_ToStringLists()
        {
            var deal = CreateDeal(showTitle: true, showSidebar: false);
            deal.DealType = new[] { new DealCardDealType { Name = "M&A", CodeName = "ma", Language = "en" } };
            deal.Industry = new[]
            {
                new DealCardIndustry { Name = "Finance", CodeName = "fin", Language = "en" },
                new DealCardIndustry { Name = "Tech", CodeName = "tech", Language = "en" }
            };
            deal.SubIndustry = new[] { new DealCardSubIndustry { Name = "Banking", CodeName = "bank", Language = "en" } };
            deal.Size = new[] { new DealCardRange { Name = "Large", CodeName = "lg" } };

            SetupDealPages(new[] { deal });

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.DealTypeNames, Is.EqualTo(ExpectedDealTypeNames));
                Assert.That(result.IndustryNames, Is.EqualTo(ExpectedIndustryNames));
                Assert.That(result.SubIndustryNames, Is.EqualTo(ExpectedSubIndustryNames));
                Assert.That(result.Range, Is.EqualTo(ExpectedRangeNames));
            });
        }

        [Test]
        public async Task GetDealPageDtoAsync_ReturnsEmptyLists_WhenLinkedItemsAreNull()
        {
            var deal = CreateDeal(showTitle: true, showSidebar: false);
            deal.DealType = null!;
            deal.Industry = null!;
            deal.SubIndustry = null!;
            deal.Size = null!;

            SetupDealPages(new[] { deal });

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.DealTypeNames, Is.Empty);
                Assert.That(result.IndustryNames, Is.Empty);
                Assert.That(result.SubIndustryNames, Is.Empty);
                Assert.That(result.Range, Is.Empty);
            });
        }

        [Test]
        public async Task GetDealPageDtoAsync_PopulatesSidebar_WhenShowSidebarTrue()
        {
            var deal = CreateDeal(showTitle: true, showSidebar: true);
            SetupDealPages(new[] { deal });

            var expectedSidebar = new DealSidebarDto { ShowPrintButton = true };
            _sidebarServiceMock
                .Setup(x => x.GetDealSidebarDtoAsync(It.IsAny<Deal>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedSidebar);

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.That(result.Sidebar, Is.SameAs(expectedSidebar));
            _sidebarServiceMock.Verify(
                x => x.GetDealSidebarDtoAsync(deal, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetDealPageDtoAsync_DoesNotCallSidebarService_WhenShowSidebarFalse()
        {
            var deal = CreateDeal(showTitle: true, showSidebar: false);
            SetupDealPages(new[] { deal });

            await _service.GetDealPageDtoAsync(CancellationToken.None);

            _sidebarServiceMock.Verify(
                x => x.GetDealSidebarDtoAsync(It.IsAny<Deal>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetDealPageDtoAsync_CallsHeroService_WhenDealPageExists()
        {
            var deal = CreateDeal(showTitle: true, showSidebar: false);
            SetupDealPages(new[] { deal });

            await _service.GetDealPageDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private void SetupDealPages(IEnumerable<Deal> pages)
        {
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Deal>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<System.Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pages);
        }

        private static Deal CreateDeal(bool showTitle, bool showSidebar)
        {
            return new Deal
            {
                SystemFields = new WebPageFields
                {
                    ContentItemID = 1,
                    WebPageItemID = 1,
                    WebPageItemName = "deal-1",
                    WebPageUrlPath = "/deals/deal-1",
                    WebPageItemTreePath = "/deals/deal-1",
                    WebPageItemWebsiteChannelId = 1,
                },
                MetadataShowTitle = showTitle,
                MetadataTitle = "Meta Title",
                ClientName = "Acme Inc.",
                DealContent = "<p>Deal content</p>",
                ShowSidebar = showSidebar,
                HeroLayout = "default",
                HeroTitle = "Hero",
                HeroDescription = "Desc",
                HeroBackgroundImage = [],
                HeroBackgroundImage_Mobile = [],
                HeroCallToActionButtonUrl = [],
                DealType = [],
                Industry = [],
                SubIndustry = [],
                Size = [],
            };
        }
    }
}
