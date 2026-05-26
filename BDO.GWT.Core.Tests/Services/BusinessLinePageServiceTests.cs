using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.services;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Tests.Services
{
    public class BusinessLinePageServiceTests
    {
        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IPageUtility> _pageUtilityMock = null!;
        private Mock<IWebPageUrlResolver> _urlResolverMock = null!;
        private BusinessLinePageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _heroServiceMock = new Mock<IHeroService>();
            _contentServiceMock = new Mock<IContentService>();
            _pageUtilityMock = new Mock<IPageUtility>();
            _urlResolverMock = new Mock<IWebPageUrlResolver>();

            _urlResolverMock
                .Setup(x => x.GetAbsoluteUrl(It.IsAny<IWebPageFieldsSource>()))
                .Returns("https://example.com/test");

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HeroDto { Title = "Hero Title" });

            _service = new BusinessLinePageService(
                _heroServiceMock.Object,
                _contentServiceMock.Object,
                _pageUtilityMock.Object,
                _urlResolverMock.Object);
        }

        [Test]
        public async Task GetBusinessLinePageDtoAsync_ReturnsEmptyDto_WhenPageIsNull()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<BusinessLine>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((BusinessLine?)null);

            var result = await _service.GetBusinessLinePageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Hero, Is.Null);
                Assert.That(result.Title, Is.Null);
                Assert.That(result.BodyContent, Is.Null);
                Assert.That(result.Sidebox, Is.Null);
            });
        }

        [Test]
        public async Task GetBusinessLinePageDtoAsync_ReturnsDto_WhenPageExists()
        {
            var page = CreateBusinessLinePage(showSideNavigation: false);

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<BusinessLine>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            var result = await _service.GetBusinessLinePageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Title, Is.EqualTo("Test Title"));
                Assert.That(result.Hero, Is.Not.Null);
                Assert.That(result.ShowSideNavigation, Is.False);
                Assert.That(result.Sidebox, Is.Null);
            });
        }

        [Test]
        public async Task GetBusinessLinePageDtoAsync_ReturnsSidebox_WhenSideNavigationEnabled()
        {
            var page = CreateBusinessLinePage(showSideNavigation: true);
            var expectedSidebox = new SideBoxDto
            {
                Title = "Side Nav",
                Links = []
            };

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<BusinessLine>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            _pageUtilityMock
                .Setup(x => x.GetSideboxDtoAsync<ServiceArea, BusinessLine>(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Func<ServiceArea, string>>(),
                    It.IsAny<Func<BusinessLine, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedSidebox);

            var result = await _service.GetBusinessLinePageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.ShowSideNavigation, Is.True);
                Assert.That(result.Sidebox, Is.Not.Null);
                Assert.That(result.Sidebox!.Title, Is.EqualTo("Side Nav"));
            });
        }

        [Test]
        public async Task GetBusinessLinePageDtoAsync_DoesNotCallGetSidebox_WhenSideNavigationDisabled()
        {
            var page = CreateBusinessLinePage(showSideNavigation: false);

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<BusinessLine>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            await _service.GetBusinessLinePageDtoAsync(CancellationToken.None);

            _pageUtilityMock.Verify(
                x => x.GetSideboxDtoAsync<ServiceArea, BusinessLine>(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Func<ServiceArea, string>>(),
                    It.IsAny<Func<BusinessLine, string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetBusinessLinePageDtoAsync_CallsHeroService_WhenPageExists()
        {
            var page = CreateBusinessLinePage(showSideNavigation: false);

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<BusinessLine>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            await _service.GetBusinessLinePageDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetBusinessLinePageDtoAsync_DoesNotCallHeroService_WhenPageIsNull()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<BusinessLine>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((BusinessLine?)null);

            await _service.GetBusinessLinePageDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetServicesPrimarySchemaMarkupDto_ReturnsSchemaMarkup()
        {
            _contentServiceMock
                .Setup(x => x.GetCurrentSiteLegalNameAsync())
                .Returns("BDO Legal Name");
            _contentServiceMock
                .Setup(x => x.GetCurrentSiteLogoImageUrlAsync())
                .Returns("https://example.com/logo.png");

            var pageDto = new BusinessLinePageDto
            {
                Url = "https://example.com/services/tax",
                MetaData = new DTOs.ReusableFieldsDTOs.MetaDataDto
                {
                    MetaTitle = "Tax Services",
                    MetaDescription = "Tax services description"
                }
            };

            var result = await _service.GetServicesPrimarySchemaMarkupDto(pageDto, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Name, Is.EqualTo("BDO Legal Name"));
                Assert.That(result.AlternateName, Is.EqualTo("BDO"));
                Assert.That(result.Url, Is.EqualTo("https://example.com/services/tax"));
                Assert.That(result.Logo, Is.EqualTo("https://example.com/logo.png"));
                Assert.That(result.Description, Is.EqualTo("Tax services description"));
                Assert.That(result.DisambiguatingDescription, Is.EqualTo("Tax Services"));
            });
        }

        private static BusinessLine CreateBusinessLinePage(bool showSideNavigation)
        {
            return new BusinessLine
            {
                SystemFields = new WebPageFields
                {
                    ContentItemID = 1,
                    ContentItemGUID = Guid.NewGuid(),
                    ContentItemName = "Test Business Line",
                    ContentItemContentTypeID = 100,
                    ContentItemIsSecured = false,
                    ContentItemCommonDataContentLanguageID = 1,
                    ContentItemCommonDataFirstPublishedWhen = new DateTime(2024, 1, 1),
                    ContentItemCommonDataLastPublishedWhen = new DateTime(2024, 6, 1),
                    WebPageItemGUID = Guid.NewGuid(),
                    WebPageItemID = 1,
                    WebPageItemName = "Test Business Line Page",
                    WebPageItemWebsiteChannelId = 1,
                    WebPageUrlPath = "/services/test",
                    WebPageItemOrder = 1,
                    WebPageItemTreePath = "/services/test",
                    WebPageUrlPathContentLanguageID = 1,
                    WebPageItemParentID = 2
                },
                ContentTitle = "Test Title",
                ContentBody = "<p>Body</p>",
                HeroLayout = "default",
                HeroTitle = "Hero",
                HeroDescription = "Hero Desc",
                HeroBackgroundImageAltText = "Alt",
                HeroBackgroundImage_MobileAltText = "Mobile Alt",
                HeroCallToActionButtonType = "link",
                HeroCallToActionButtonText = "Click",
                HeroBackgroundImage = [],
                HeroBackgroundImage_Mobile = [],
                HeroCallToActionButtonUrl = [],
                ShowSideNavigation = showSideNavigation,
                SideNavigationTitle = "Side Nav Title",
                TileIcon = []
            };
        }
    }
}
