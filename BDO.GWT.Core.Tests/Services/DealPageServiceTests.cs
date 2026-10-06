using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs.Pages;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.LiveSite.Services;
using BDO.GWT.Web.Core.Services;
using BDO;
using CMS.ContentEngine;
using CMS.Websites.Routing;
using CMS.Websites;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.Content.Web.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;

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

        //[Test]
        //public async Task GetDealPageDtoAsync_MapsScalarFields_WhenDealPageExists()
        //{
        //    var deal = CreateDeal(showTitle: true, showSidebar: false);
        //    SetupDealPages(new[] { deal });

        //    var result = await _service.GetDealPageDtoAsync(CancellationToken.None);
        //    Assert.Multiple(() =>
        //    {
        //        Assert.That(result.Title, Is.EqualTo("Meta Title"));
        //        Assert.That(result.ClientName, Is.EqualTo("Acme Inc."));
        //        Assert.That(result.Content, Is.Not.Null);
        //        Assert.That(result.Hero, Is.Not.Null);
        //        Assert.That(result.Hero.Title, Is.EqualTo("Hero"));
        //        Assert.That(result.Sidebar, Is.Null);
        //    });
        //}

        [Test]
        public async Task GetDealPageDtoAsync_TitleIsNull_WhenMetadataShowTitleFalse()
        {
            var deal = CreateDeal(showTitle: false, showSidebar: false);
            SetupDealPages(new[] { deal });

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.That(result.Title, Is.Null);
        }

        //[Test]
        //public async Task GetDealPageDtoAsync_MapsLinkedItemNames_ToStringLists()
        //{
        //    var deal = CreateDeal(showTitle: true, showSidebar: false);
        //    deal.DealType = new[] { new DealCardDealType { Name = "M&A", CodeName = "ma", Language = "en" } };
        //    deal.Industry = new[]
        //    {
        //        new DealCardIndustry { Name = "Finance", CodeName = "fin", Language = "en" },
        //        new DealCardIndustry { Name = "Tech", CodeName = "tech", Language = "en" }
        //    };
        //    deal.SubIndustry = new[] { new DealCardSubIndustry { Name = "Banking", CodeName = "bank", Language = "en" } };
        //    deal.Size = new[] { new DealCardRange { Name = "Large", CodeName = "lg" } };

        //    SetupDealPages(new[] { deal });

        //    var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

        //    Assert.Multiple(() =>
        //    {
        //        Assert.That(result.DealTypeNames, Is.EqualTo(ExpectedDealTypeNames));
        //        Assert.That(result.IndustryNames, Is.EqualTo(ExpectedIndustryNames));
        //        Assert.That(result.SubIndustryNames, Is.EqualTo(ExpectedSubIndustryNames));
        //        Assert.That(result.Range, Is.EqualTo(ExpectedRangeNames));
        //    });
        //}

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

        //[Test]
        //public async Task GetDealPageDtoAsync_PopulatesSidebar_WhenShowSidebarTrue()
        //{
        //    var deal = CreateDeal(showTitle: true, showSidebar: true);
        //    SetupDealPages(new[] { deal });

        //    var expectedSidebar = new DealSidebarDto { ShowPrintButton = true };
        //    _sidebarServiceMock
        //        .Setup(x => x.GetDealSidebarDtoAsync(It.IsAny<Deal>(), It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(expectedSidebar);

        //    var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

        //    Assert.That(result.Sidebar, Is.SameAs(expectedSidebar));
        //    _sidebarServiceMock.Verify(
        //        x => x.GetDealSidebarDtoAsync(deal, It.IsAny<CancellationToken>()),
        //        Times.Once);
        //}

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

        //[Test]
        //public async Task GetDealPageDtoAsync_CallsHeroService_WhenDealPageExists()
        //{
        //    var deal = CreateDeal(showTitle: true, showSidebar: false);
        //    SetupDealPages(new[] { deal });

        //    await _service.GetDealPageDtoAsync(CancellationToken.None);

        //    _heroServiceMock.Verify(
        //        x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
        //        Times.Once);
        //}

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


namespace BDO.GWT.Core.Tests.Services
{
    public class DealPageServiceCoverageTests
    {
        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<ISidebarService> _sidebarServiceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
        private Mock<IWebPageDataContextRetriever> _webPageDataContextRetrieverMock = null!;
        private DealPageService _service = null!;
        private HeroData? _capturedHeroData;

        [SetUp]
        public void Setup()
        {
            _heroServiceMock = new Mock<IHeroService>();
            _sidebarServiceMock = new Mock<ISidebarService>();
            _contentServiceMock = new Mock<IContentService>();
            _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
            _webPageDataContextRetrieverMock = new Mock<IWebPageDataContextRetriever>();
            _capturedHeroData = null;

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((d, _) => _capturedHeroData = d)
                .ReturnsAsync(new HeroDto { Title = "Hero" });

            _service = new DealPageService(
                _heroServiceMock.Object,
                _sidebarServiceMock.Object,
                _contentServiceMock.Object,
                _websiteChannelContextMock.Object,
                _webPageDataContextRetrieverMock.Object);
        }

        #region Helpers

        private void SetupPageContext(Guid pageGuid)
        {
            var context = CreateWebPageDataContext(new RoutedWebPage { WebPageItemGUID = pageGuid });
            _webPageDataContextRetrieverMock
                .Setup(x => x.TryRetrieve(out context))
                .Returns(true);
        }

        private static WebPageDataContext CreateWebPageDataContext(RoutedWebPage? webPage)
        {
            var context = (WebPageDataContext)RuntimeHelpers.GetUninitializedObject(typeof(WebPageDataContext));
            var property = typeof(WebPageDataContext).GetProperty(nameof(WebPageDataContext.WebPage),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

            if (property.CanWrite)
            {
                property.SetValue(context, webPage);
            }
            else
            {
                var backingField = typeof(WebPageDataContext).GetField($"<{nameof(WebPageDataContext.WebPage)}>k__BackingField",
                    BindingFlags.Instance | BindingFlags.NonPublic)!;
                backingField.SetValue(context, webPage);
            }

            return context;
        }

        private void SetupDealPages(IEnumerable<Deal> pages)
        {
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Deal>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pages);
        }

        private void SetupSidebar(DealSidebarDto? sidebar)
        {
            _sidebarServiceMock
                .Setup(x => x.GetDealSidebarDtoAsync(It.IsAny<Deal>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sidebar!);
        }

        private static Deal CreateDeal(bool showTitle = true, bool showSidebar = false, string? displayedContacts = "0")
        {
            return new Deal
            {
                SystemFields = new WebPageFields
                {
                    ContentItemID = 1,
                    WebPageItemID = 1,
                    WebPageItemGUID = Guid.NewGuid(),
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
                DisplayedContacts = displayedContacts!,
                HeroLayout = "default",
                HeroTitle = "Hero Title",
                HeroDescription = "Hero Desc",
                HeroBackgroundImageAltText = "bg alt",
                HeroBackgroundImage_MobileAltText = "mobile alt",
                HeroCallToActionButtonType = "primary",
                HeroCallToActionButtonText = "Click",
                HeroBackgroundImage = [],
                HeroBackgroundImage_Mobile = [],
                HeroCallToActionButtonUrl = [],
                DealType = [],
                Industry = [],
                SubIndustry = [],
                Size = [],
            };
        }

        private static SidebarPersonDto Contact(string name, string? imageUrl) => new()
        {
            FullName = name,
            Image = imageUrl is null ? null : new ImageDto { Url = imageUrl },
        };

        #endregion

        #region Context handling

        [Test]
        public async Task GetDealPageDtoAsync_ReturnsEmptyDto_WhenContextNotAvailable()
        {
            WebPageDataContext? context = null;
            _webPageDataContextRetrieverMock
                .Setup(x => x.TryRetrieve(out context))
                .Returns(false);

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Title, Is.Null);
            _contentServiceMock.Verify(x => x.GetPagesAsync<Deal>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetDealPageDtoAsync_ReturnsEmptyDto_WhenContextWebPageIsNull()
        {
            var context = CreateWebPageDataContext(null);
            _webPageDataContextRetrieverMock
                .Setup(x => x.TryRetrieve(out context))
                .Returns(true);

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.That(result.Hero, Is.Null);
            _heroServiceMock.Verify(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetDealPageDtoAsync_ReturnsEmptyDto_WhenContextPresentButNoDealFound()
        {
            SetupPageContext(Guid.NewGuid());
            SetupDealPages([]);

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.That(result.Title, Is.Null);
            _heroServiceMock.Verify(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetDealPageDtoAsync_RetrievesPages_WithLinkedItemsMaxLevel5()
        {
            SetupPageContext(Guid.NewGuid());
            SetupDealPages([CreateDeal()]);

            await _service.GetDealPageDtoAsync(CancellationToken.None);

            _contentServiceMock.Verify(x => x.GetPagesAsync<Deal>(
                It.Is<RetrievePagesParameters>(p => p.LinkedItemsMaxLevel == 5),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Mapping

        [Test]
        public async Task GetDealPageDtoAsync_MapsScalarFields_WhenDealFound()
        {
            SetupPageContext(Guid.NewGuid());
            SetupDealPages([CreateDeal(showTitle: true)]);

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.EqualTo("Meta Title"));
                Assert.That(result.ClientName, Is.EqualTo("Acme Inc."));
                Assert.That(result.Content, Does.Contain("Deal content"));
                Assert.That(result.Hero, Is.Not.Null);
                Assert.That(result.Hero!.Title, Is.EqualTo("Hero"));
                Assert.That(result.Sidebar, Is.Null);
                Assert.That(result.DealTypeNames, Is.Empty);
                Assert.That(result.IndustryNames, Is.Empty);
                Assert.That(result.SubIndustryNames, Is.Empty);
                Assert.That(result.Range, Is.Empty);
            });
        }

        [Test]
        public async Task GetDealPageDtoAsync_TitleIsNull_WhenShowTitleFalse_AndDealFound()
        {
            SetupPageContext(Guid.NewGuid());
            SetupDealPages([CreateDeal(showTitle: false)]);

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.Null);
                Assert.That(result.ClientName, Is.EqualTo("Acme Inc."));
            });
        }

        [Test]
        public async Task GetDealPageDtoAsync_RemovesTildeFromContentUrls()
        {
            SetupPageContext(Guid.NewGuid());
            var deal = CreateDeal();
            deal.DealContent = "<a href=\"~/about\">About</a>";
            SetupDealPages([deal]);

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.That(result.Content, Does.Not.Contain("~/"));
        }

        [Test]
        public async Task GetDealPageDtoAsync_ReturnsEmptyNameLists_WhenLinkedItemsNull_AndDealFound()
        {
            SetupPageContext(Guid.NewGuid());
            var deal = CreateDeal();
            deal.DealType = null!;
            deal.Industry = null!;
            deal.SubIndustry = null!;
            deal.Size = null!;
            SetupDealPages([deal]);

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
        public void GetDealPageDtoAsync_Throws_WhenCancelledAfterRetrieval()
        {
            SetupPageContext(Guid.NewGuid());
            SetupDealPages([CreateDeal()]);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.ThrowsAsync<OperationCanceledException>(() => _service.GetDealPageDtoAsync(cts.Token));
        }

        #endregion

        #region Hero

        [Test]
        public async Task GetDealPageDtoAsync_PassesHeroData_FromDealFields()
        {
            SetupPageContext(Guid.NewGuid());
            SetupDealPages([CreateDeal()]);

            await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.That(_capturedHeroData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(_capturedHeroData!.Layout, Is.EqualTo("default"));
                Assert.That(_capturedHeroData.Title, Is.EqualTo("Hero Title"));
                Assert.That(_capturedHeroData.Description, Is.EqualTo("Hero Desc"));
                Assert.That(_capturedHeroData.BackgroundImageAltText, Is.EqualTo("bg alt"));
                Assert.That(_capturedHeroData.MobileBackgroundImageAltText, Is.EqualTo("mobile alt"));
                Assert.That(_capturedHeroData.CallToActionButtonType, Is.EqualTo("primary"));
                Assert.That(_capturedHeroData.CallToActionButtonText, Is.EqualTo("Click"));
                Assert.That(_capturedHeroData.BackgroundImage, Is.Null);
                Assert.That(_capturedHeroData.MobileBackgroundImage, Is.Null);
                Assert.That(_capturedHeroData.CallToActionButtonUrl, Is.Null);
            });
        }

        [Test]
        public async Task GetDealPageDtoAsync_ParsesHeroImageLinks_WhenImagesPresent()
        {
            SetupPageContext(Guid.NewGuid());
            var deal = CreateDeal();
            deal.HeroBackgroundImage = [new ContentAttachment { ContentAttachmentAsset = new ContentItemAsset { Url = "~/media/bg.jpg" } }];
            deal.HeroBackgroundImage_Mobile = [new ContentAttachment { ContentAttachmentAsset = new ContentItemAsset { Url = "/media/mobile.jpg" } }];
            SetupDealPages([deal]);

            await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(_capturedHeroData!.BackgroundImage, Is.Not.Null);
                Assert.That(_capturedHeroData.BackgroundImage!.Path, Does.Contain("media/bg.jpg"));
                Assert.That(_capturedHeroData.MobileBackgroundImage, Is.Not.Null);
                Assert.That(_capturedHeroData.MobileBackgroundImage!.Path, Does.Contain("media/mobile.jpg"));
            });
        }

        [Test]
        public async Task GetDealPageDtoAsync_HeroImageIsNull_WhenAssetUrlIsWhitespace()
        {
            SetupPageContext(Guid.NewGuid());
            var deal = CreateDeal();
            deal.HeroBackgroundImage = [new ContentAttachment { ContentAttachmentAsset = new ContentItemAsset { Url = "   " } }];
            SetupDealPages([deal]);

            await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.That(_capturedHeroData!.BackgroundImage, Is.Null);
        }

        [Test]
        public async Task GetDealPageDtoAsync_HeroImagesNull_WhenCollectionsNull()
        {
            SetupPageContext(Guid.NewGuid());
            var deal = CreateDeal();
            deal.HeroBackgroundImage = null!;
            deal.HeroBackgroundImage_Mobile = null!;
            deal.HeroCallToActionButtonUrl = null!;
            SetupDealPages([deal]);

            await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(_capturedHeroData!.BackgroundImage, Is.Null);
                Assert.That(_capturedHeroData.MobileBackgroundImage, Is.Null);
                Assert.That(_capturedHeroData.CallToActionButtonUrl, Is.Null);
            });
        }

        #endregion

        #region Sidebar

        [Test]
        public async Task GetDealPageDtoAsync_CallsSidebarService_WhenShowSidebarTrue()
        {
            SetupPageContext(Guid.NewGuid());
            var deal = CreateDeal(showSidebar: true);
            SetupDealPages([deal]);
            var sidebar = new DealSidebarDto { ShowPrintButton = true };
            SetupSidebar(sidebar);

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.That(result.Sidebar, Is.SameAs(sidebar));
            _sidebarServiceMock.Verify(x => x.GetDealSidebarDtoAsync(deal, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetDealPageDtoAsync_SidebarNull_WhenSidebarServiceReturnsNull()
        {
            SetupPageContext(Guid.NewGuid());
            SetupDealPages([CreateDeal(showSidebar: true)]);
            SetupSidebar(null);

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.That(result.Sidebar, Is.Null);
        }
        private static readonly string[] ExpectedContactNames = ["A", "B"];
        [Test]
        public async Task GetDealPageDtoAsync_LimitsContacts_ToDisplayedContactsCount()
        {
            SetupPageContext(Guid.NewGuid());
            SetupDealPages([CreateDeal(showSidebar: true, displayedContacts: "2")]);
            SetupSidebar(new DealSidebarDto
            {
                Contacts = [Contact("A", null), Contact("B", null), Contact("C", null)],
            });

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.That(result.Sidebar!.Contacts.Select(c => c.FullName), Is.EqualTo(ExpectedContactNames));
        }

        [TestCase("abc")]
        [TestCase("")]
        [TestCase(null)]
        public async Task GetDealPageDtoAsync_ReturnsNoContacts_WhenDisplayedContactsNotNumeric(string? displayed)
        {
            SetupPageContext(Guid.NewGuid());
            SetupDealPages([CreateDeal(showSidebar: true, displayedContacts: displayed)]);
            SetupSidebar(new DealSidebarDto { Contacts = [Contact("A", null), Contact("B", null)] });

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.That(result.Sidebar!.Contacts, Is.Empty);
        }

        [Test]
        public async Task GetDealPageDtoAsync_ReturnsAllContacts_WhenDisplayedCountExceedsAvailable()
        {
            SetupPageContext(Guid.NewGuid());
            SetupDealPages([CreateDeal(showSidebar: true, displayedContacts: "10")]);
            SetupSidebar(new DealSidebarDto { Contacts = [Contact("A", null), Contact("B", null)] });

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.That(result.Sidebar!.Contacts, Has.Length.EqualTo(2));
        }

        [Test]
        public async Task GetDealPageDtoAsync_TrimsTildeFromContactImageUrls()
        {
            SetupPageContext(Guid.NewGuid());
            SetupDealPages([CreateDeal(showSidebar: true, displayedContacts: "3")]);
            SetupSidebar(new DealSidebarDto
            {
                Contacts =
                [
                    Contact("A", "~/media/a.jpg"),
                    Contact("B", "/media/b.jpg"),
                    Contact("C", null),
                ],
            });

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);
            var contacts = result.Sidebar!.Contacts;

            Assert.Multiple(() =>
            {
                Assert.That(contacts[0].Image!.Url, Is.EqualTo("/media/a.jpg"));
                Assert.That(contacts[1].Image!.Url, Is.EqualTo("/media/b.jpg"));
                Assert.That(contacts[2].Image, Is.Null);
            });
        }

        [Test]
        public async Task GetDealPageDtoAsync_LeavesContactImageUntouched_WhenImageUrlNull()
        {
            SetupPageContext(Guid.NewGuid());
            SetupDealPages([CreateDeal(showSidebar: true, displayedContacts: "1")]);
            SetupSidebar(new DealSidebarDto { Contacts = [new SidebarPersonDto { FullName = "A", Image = new ImageDto { Url = null } }] });

            var result = await _service.GetDealPageDtoAsync(CancellationToken.None);

            Assert.That(result.Sidebar!.Contacts[0].Image!.Url, Is.Null);
        }

        #endregion
    }
}
