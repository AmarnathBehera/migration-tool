using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.services;
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
    public class IndustryCategoryPageServiceTests
    {
        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IPageUtility> _pageUtilityMock = null!;
        private IndustryCategoryPageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _heroServiceMock = new Mock<IHeroService>();
            _contentServiceMock = new Mock<IContentService>();
            _pageUtilityMock = new Mock<IPageUtility>();

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((HeroData data, CancellationToken _) => new HeroDto
                {
                    Layout = data.Layout,
                    Title = data.Title,
                    Description = data.Description,
                    CtaButtonType = data.CallToActionButtonType,
                    CtaButtonText = data.CallToActionButtonText,
                });

            _pageUtilityMock
                .Setup(x => x.GetSideboxDtoAsync<Industry, IndustryCategory>(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Func<Industry, string>>(),
                    It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SideBoxDto { Title = "Default Sidebox" });

            _service = new IndustryCategoryPageService(
                _heroServiceMock.Object,
                _contentServiceMock.Object,
                _pageUtilityMock.Object);
        }

        private static IndustryCategory CreateDefaultPage() => new IndustryCategory
        {
            DescriptionTitle = "Finance",
            DescriptionBody = "Finance category description",
            ShowSideNavigation = false,
            SideboxTitle = "Industries",
            HeroTitle = "Finance Hero",
            HeroLayout = "default",
            HeroDescription = "Hero Description",
            HeroCallToActionButtonText = "Explore",
            HeroCallToActionButtonType = "primary",
            SystemFields = new WebPageFields
            {
                ContentItemID = 1,
                ContentItemGUID = Guid.NewGuid(),
                ContentItemName = "Finance Category",
                ContentItemContentTypeID = 301,
                ContentItemIsSecured = false,
                ContentItemCommonDataContentLanguageID = 1,
                ContentItemCommonDataFirstPublishedWhen = new DateTime(2024, 1, 1),
                ContentItemCommonDataLastPublishedWhen = new DateTime(2024, 6, 1),
                WebPageItemGUID = Guid.NewGuid(),
                WebPageItemID = 300,
                WebPageItemName = "Finance Industry Category",
                WebPageItemWebsiteChannelId = 2,
                WebPageUrlPath = "/industries/finance",
                WebPageItemOrder = 1,
                WebPageItemTreePath = "/industries/finance",
                WebPageUrlPathContentLanguageID = 1,
                WebPageItemParentID = 10
            }
        };

        private void SetupContentService(IndustryCategory page)
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<IndustryCategory>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);
        }

        // ── Null / Empty ─────────────────────────────────────────────────────

        [Test]
        public async Task GetIndustryCategoryPageDtoAsync_WhenPageIsNull_ReturnsEmptyDto()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<IndustryCategory>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IndustryCategory?)null);

            var result = await _service.GetIndustryCategoryPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Hero, Is.Null);
                Assert.That(result.Sidebox, Is.Null);
                Assert.That(result.BaseContent, Is.Null);
                Assert.That(result.BaseWebPage, Is.Null);
            });
        }

        [Test]
        public async Task GetIndustryCategoryPageDtoAsync_WhenPageIsNull_DoesNotCallHeroServiceOrPageUtility()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<IndustryCategory>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IndustryCategory?)null);

            await _service.GetIndustryCategoryPageDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
            _pageUtilityMock.Verify(
                x => x.GetSideboxDtoAsync<Industry, IndustryCategory>(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Func<Industry, string>>(), It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Test]
        public async Task GetIndustryCategoryPageDtoAsync_WhenPageExists_ReturnsMappedDto()
        {
            SetupContentService(CreateDefaultPage());

            var result = await _service.GetIndustryCategoryPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Hero, Is.Not.Null);
                Assert.That(result.Title, Is.EqualTo("Finance"));
                Assert.That(result.Description, Is.EqualTo("Finance category description"));
            });
        }

        [Test]
        public async Task GetIndustryCategoryPageDtoAsync_MapsHeroFieldsToHeroData()
        {
            var page = CreateDefaultPage();
            page.HeroTitle = "Finance Hero Title";
            page.HeroDescription = "Finance Hero Description";
            page.HeroLayout = "banner";
            page.HeroCallToActionButtonText = "Explore Finance";
            page.HeroCallToActionButtonType = "secondary";

            SetupContentService(page);

            HeroData? capturedHeroData = null;
            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((data, _) => capturedHeroData = data)
                .ReturnsAsync(new HeroDto { Layout = "banner" });

            await _service.GetIndustryCategoryPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(capturedHeroData, Is.Not.Null);
                Assert.That(capturedHeroData!.Title, Is.EqualTo("Finance Hero Title"));
                Assert.That(capturedHeroData.Description, Is.EqualTo("Finance Hero Description"));
                Assert.That(capturedHeroData.Layout, Is.EqualTo("banner"));
                Assert.That(capturedHeroData.CallToActionButtonText, Is.EqualTo("Explore Finance"));
                Assert.That(capturedHeroData.CallToActionButtonType, Is.EqualTo("secondary"));
            });
        }

        [Test]
        public async Task GetIndustryCategoryPageDtoAsync_WhenHeroImagesAreNull_HeroDataImagesAreNull()
        {
            var page = CreateDefaultPage();
            page.HeroBackgroundImage = null;
            page.HeroBackgroundImage_Mobile = null;

            SetupContentService(page);

            HeroData? capturedHeroData = null;
            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((data, _) => capturedHeroData = data)
                .ReturnsAsync(new HeroDto { Layout = "default" });

            await _service.GetIndustryCategoryPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(capturedHeroData, Is.Not.Null);
                Assert.That(capturedHeroData!.BackgroundImage, Is.Null);
                Assert.That(capturedHeroData.MobileBackgroundImage, Is.Null);
            });
        }

        [Test]
        public async Task GetIndustryCategoryPageDtoAsync_WhenCtaUrlIsNull_HeroDataCtaButtonUrlIsNull()
        {
            var page = CreateDefaultPage();
            page.HeroCallToActionButtonUrl = null;

            SetupContentService(page);

            HeroData? capturedHeroData = null;
            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((data, _) => capturedHeroData = data)
                .ReturnsAsync(new HeroDto { Layout = "default" });

            await _service.GetIndustryCategoryPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(capturedHeroData, Is.Not.Null);
                Assert.That(capturedHeroData!.CallToActionButtonUrl, Is.Null);
            });
        }

        [Test]
        public async Task GetIndustryCategoryPageDtoAsync_CallsHeroService_Once()
        {
            SetupContentService(CreateDefaultPage());

            await _service.GetIndustryCategoryPageDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        // ── Sidebox ──────────────────────────────────────────────────────────

        [Test]
        public async Task GetIndustryCategoryPageDtoAsync_WhenShowSideNavigationIsTrue_ReturnsSidebox()
        {
            var page = CreateDefaultPage();
            page.ShowSideNavigation = true;
            page.SideboxTitle = "Explore Industries";

            SetupContentService(page);

            var sideboxDto = new SideBoxDto
            {
                Title = "Explore Industries",
                Links = new[] { new LinkItemDto { Title = "Finance", URL = "/industries/finance" } }
            };

            _pageUtilityMock
                .Setup(x => x.GetSideboxDtoAsync<Industry, IndustryCategory>(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Func<Industry, string>>(),
                    It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(sideboxDto);

            var result = await _service.GetIndustryCategoryPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Sidebox, Is.Not.Null);
                Assert.That(result.Sidebox!.Title, Is.EqualTo("Explore Industries"));
            });
        }

        [Test]
        public async Task GetIndustryCategoryPageDtoAsync_WhenShowSideNavigationIsFalse_SideboxIsNull()
        {
            var page = CreateDefaultPage();
            page.ShowSideNavigation = false;

            SetupContentService(page);

            var result = await _service.GetIndustryCategoryPageDtoAsync(CancellationToken.None);

            Assert.That(result.Sidebox, Is.Null);
        }

        [Test]
        public async Task GetIndustryCategoryPageDtoAsync_WhenShowSideNavigationIsFalse_DoesNotCallPageUtility()
        {
            var page = CreateDefaultPage();
            page.ShowSideNavigation = false;

            SetupContentService(page);

            await _service.GetIndustryCategoryPageDtoAsync(CancellationToken.None);

            _pageUtilityMock.Verify(
                x => x.GetSideboxDtoAsync<Industry, IndustryCategory>(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Func<Industry, string>>(), It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Test]
        public async Task GetIndustryCategoryPageDtoAsync_WhenShowSideNavigationIsTrue_CallsPageUtilityOnce()
        {
            var page = CreateDefaultPage();
            page.ShowSideNavigation = true;

            SetupContentService(page);

            await _service.GetIndustryCategoryPageDtoAsync(CancellationToken.None);

            _pageUtilityMock.Verify(
                x => x.GetSideboxDtoAsync<Industry, IndustryCategory>(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Func<Industry, string>>(), It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task GetIndustryCategoryPageDtoAsync_PassesSideboxTitleAndIds_ToPageUtility()
        {
            var page = CreateDefaultPage();
            page.ShowSideNavigation = true;
            page.SideboxTitle = "My Sidebox";
            page.SystemFields.WebPageItemID = 500;
            page.SystemFields.WebPageItemParentID = 100;

            SetupContentService(page);

            string? capturedTitle = null;
            int capturedParentId = 0;
            int capturedSiblingParentId = 0;

            _pageUtilityMock
                .Setup(x => x.GetSideboxDtoAsync<Industry, IndustryCategory>(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Func<Industry, string>>(), It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, int, int, Func<Industry, string>, Func<IndustryCategory, string>, CancellationToken>(
                    (title, parentId, siblingParentId, _, _, _) =>
                    {
                        capturedTitle = title;
                        capturedParentId = parentId;
                        capturedSiblingParentId = siblingParentId;
                    })
                .ReturnsAsync(new SideBoxDto());

            await _service.GetIndustryCategoryPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(capturedTitle, Is.EqualTo("My Sidebox"));
                Assert.That(capturedParentId, Is.EqualTo(500));
                Assert.That(capturedSiblingParentId, Is.EqualTo(100));
            });
        }

        // ── BaseContent mapping ──────────────────────────────────────────────

        [Test]
        public async Task GetIndustryCategoryPageDtoAsync_MapsAllBaseContentFields()
        {
            var contentGuid = Guid.NewGuid();
            var publishedOn = new DateTime(2024, 3, 15);
            var lastPublishedOn = new DateTime(2024, 9, 1);

            var page = CreateDefaultPage();
            page.SystemFields.ContentItemID = 77;
            page.SystemFields.ContentItemGUID = contentGuid;
            page.SystemFields.ContentItemName = "Finance Category";
            page.SystemFields.ContentItemContentTypeID = 305;
            page.SystemFields.ContentItemIsSecured = true;
            page.SystemFields.ContentItemCommonDataContentLanguageID = 3;
            page.SystemFields.ContentItemCommonDataFirstPublishedWhen = publishedOn;
            page.SystemFields.ContentItemCommonDataLastPublishedWhen = lastPublishedOn;

            SetupContentService(page);

            var result = await _service.GetIndustryCategoryPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.BaseContent, Is.Not.Null);
                Assert.That(result.BaseContent!.ContentItemID, Is.EqualTo(77));
                Assert.That(result.BaseContent.ContentItemGUID, Is.EqualTo(contentGuid));
                Assert.That(result.BaseContent.ContentItemName, Is.EqualTo("Finance Category"));
                Assert.That(result.BaseContent.ContentItemContentTypeID, Is.EqualTo(305));
                Assert.That(result.BaseContent.ContentItemIsSecured, Is.True);
                Assert.That(result.BaseContent.ContentItemCommonDataContentLanguageID, Is.EqualTo(3));
                Assert.That(result.BaseContent.ContentItemCommonDataFirstPublishedWhen, Is.EqualTo(publishedOn));
                Assert.That(result.BaseContent.ContentItemCommonDataLastPublishedWhen, Is.EqualTo(lastPublishedOn));
            });
        }

        // ── BaseWebPage mapping ──────────────────────────────────────────────

        [Test]
        public async Task GetIndustryCategoryPageDtoAsync_MapsAllBaseWebPageFields()
        {
            var webPageGuid = Guid.NewGuid();

            var page = CreateDefaultPage();
            page.SystemFields.WebPageItemGUID = webPageGuid;
            page.SystemFields.WebPageItemID = 321;
            page.SystemFields.WebPageItemName = "Finance Category Page";
            page.SystemFields.WebPageItemWebsiteChannelId = 5;
            page.SystemFields.WebPageUrlPath = "/industries/finance";
            page.SystemFields.WebPageItemOrder = 2;
            page.SystemFields.WebPageItemTreePath = "/industries/finance";
            page.SystemFields.WebPageUrlPathContentLanguageID = 2;
            page.SystemFields.WebPageItemParentID = 10;

            SetupContentService(page);

            var result = await _service.GetIndustryCategoryPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.BaseWebPage, Is.Not.Null);
                Assert.That(result.BaseWebPage!.WebPageItemGUID, Is.EqualTo(webPageGuid));
                Assert.That(result.BaseWebPage.WebPageItemID, Is.EqualTo(321));
                Assert.That(result.BaseWebPage.WebPageItemName, Is.EqualTo("Finance Category Page"));
                Assert.That(result.BaseWebPage.WebPageItemWebsiteChannelID, Is.EqualTo(5));
                Assert.That(result.BaseWebPage.WebPageUrlPath, Is.EqualTo("/industries/finance"));
                Assert.That(result.BaseWebPage.WebPageItemOrder, Is.EqualTo(2));
                Assert.That(result.BaseWebPage.WebPageItemTreePath, Is.EqualTo("/industries/finance"));
                Assert.That(result.BaseWebPage.WebPageUrlPathContentLanguageID, Is.EqualTo(2));
                Assert.That(result.BaseWebPage.WebPageItemParentID, Is.EqualTo(10));
            });
        }

        // ── CancellationToken ────────────────────────────────────────────────

        [Test]
        public async Task GetIndustryCategoryPageDtoAsync_PassesCancellationToken_ToHeroService()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<IndustryCategory>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    token))
                .ReturnsAsync(CreateDefaultPage());

            CancellationToken capturedToken = default;
            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((_, ct) => capturedToken = ct)
                .ReturnsAsync(new HeroDto { Layout = "default" });

            await _service.GetIndustryCategoryPageDtoAsync(token);

            Assert.That(capturedToken, Is.EqualTo(token));
        }

        [Test]
        public async Task GetIndustryCategoryPageDtoAsync_WhenShowSideNavigation_PassesCancellationToken_ToPageUtility()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            var page = CreateDefaultPage();
            page.ShowSideNavigation = true;

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<IndustryCategory>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    token))
                .ReturnsAsync(page);

            CancellationToken capturedToken = default;
            _pageUtilityMock
                .Setup(x => x.GetSideboxDtoAsync<Industry, IndustryCategory>(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Func<Industry, string>>(), It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, int, int, Func<Industry, string>, Func<IndustryCategory, string>, CancellationToken>(
                    (_, _, _, _, _, ct) => capturedToken = ct)
                .ReturnsAsync(new SideBoxDto());

            await _service.GetIndustryCategoryPageDtoAsync(token);

            Assert.That(capturedToken, Is.EqualTo(token));
        }
    }
}
