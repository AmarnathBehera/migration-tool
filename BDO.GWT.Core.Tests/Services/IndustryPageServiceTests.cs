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
    public class IndustryPageServiceTests
    {
        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IPageUtility> _pageUtilityMock = null!;
        private IndustryPageService _service = null!;

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
                .Setup(x => x.GetSideboxDtoAsync<IndustryService, Industry>(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Func<IndustryService, string>>(),
                    It.IsAny<Func<Industry, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SideBoxDto { Title = "Default Sidebox" });

            _service = new IndustryPageService(
                _heroServiceMock.Object,
                _contentServiceMock.Object,
                _pageUtilityMock.Object);
        }

        private static Industry CreateDefaultPage() => new Industry
        {
            DescriptionTitle = "Finance",
            DescriptionBody = "Finance industry description",
            ShowSideNavigation = false,
            SideboxTitle = "Finance Services",
            HeroTitle = "Finance Hero",
            HeroLayout = "default",
            HeroDescription = "Hero Description",
            HeroCallToActionButtonText = "Explore",
            HeroCallToActionButtonType = "primary",
            SystemFields = new WebPageFields
            {
                ContentItemID = 1,
                ContentItemGUID = Guid.NewGuid(),
                ContentItemName = "Finance Industry",
                ContentItemContentTypeID = 401,
                ContentItemIsSecured = false,
                ContentItemCommonDataContentLanguageID = 1,
                ContentItemCommonDataFirstPublishedWhen = new DateTime(2024, 1, 1),
                ContentItemCommonDataLastPublishedWhen = new DateTime(2024, 6, 1),
                WebPageItemGUID = Guid.NewGuid(),
                WebPageItemID = 400,
                WebPageItemName = "Finance Industry Page",
                WebPageItemWebsiteChannelId = 2,
                WebPageUrlPath = "/industries/finance",
                WebPageItemOrder = 1,
                WebPageItemTreePath = "/industries/finance",
                WebPageUrlPathContentLanguageID = 1,
                WebPageItemParentID = 10
            }
        };

        private void SetupContentService(Industry page)
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Industry>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);
        }

        // ── Null / Empty ─────────────────────────────────────────────────────

        [Test]
        public async Task GetIndustryPageDtoAsync_WhenPageIsNull_ReturnsEmptyDto()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Industry>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Industry?)null);

            var result = await _service.GetIndustryPageDtoAsync(CancellationToken.None);

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
        public async Task GetIndustryPageDtoAsync_WhenPageIsNull_DoesNotCallHeroServiceOrPageUtility()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Industry>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Industry?)null);

            await _service.GetIndustryPageDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
            _pageUtilityMock.Verify(
                x => x.GetSideboxDtoAsync<IndustryService, Industry>(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Func<IndustryService, string>>(), It.IsAny<Func<Industry, string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Test]
        public async Task GetIndustryPageDtoAsync_WhenPageExists_ReturnsMappedDto()
        {
            SetupContentService(CreateDefaultPage());

            var result = await _service.GetIndustryPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Hero, Is.Not.Null);
                Assert.That(result.Title, Is.EqualTo("Finance"));
                Assert.That(result.Description, Is.EqualTo("Finance industry description"));
            });
        }

        [Test]
        public async Task GetIndustryPageDtoAsync_MapsHeroFieldsToHeroData()
        {
            var page = CreateDefaultPage();
            page.HeroTitle = "Finance Industry Hero";
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

            await _service.GetIndustryPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(capturedHeroData, Is.Not.Null);
                Assert.That(capturedHeroData!.Title, Is.EqualTo("Finance Industry Hero"));
                Assert.That(capturedHeroData.Description, Is.EqualTo("Finance Hero Description"));
                Assert.That(capturedHeroData.Layout, Is.EqualTo("banner"));
                Assert.That(capturedHeroData.CallToActionButtonText, Is.EqualTo("Explore Finance"));
                Assert.That(capturedHeroData.CallToActionButtonType, Is.EqualTo("secondary"));
            });
        }

        [Test]
        public async Task GetIndustryPageDtoAsync_WhenHeroImagesAreNull_HeroDataImagesAreNull()
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

            await _service.GetIndustryPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(capturedHeroData, Is.Not.Null);
                Assert.That(capturedHeroData!.BackgroundImage, Is.Null);
                Assert.That(capturedHeroData.MobileBackgroundImage, Is.Null);
            });
        }

        [Test]
        public async Task GetIndustryPageDtoAsync_WhenCtaUrlIsNull_HeroDataCtaButtonUrlIsNull()
        {
            var page = CreateDefaultPage();
            page.HeroCallToActionButtonUrl = null;

            SetupContentService(page);

            HeroData? capturedHeroData = null;
            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((data, _) => capturedHeroData = data)
                .ReturnsAsync(new HeroDto { Layout = "default" });

            await _service.GetIndustryPageDtoAsync(CancellationToken.None);

            Assert.That(capturedHeroData, Is.Not.Null);
            Assert.That(capturedHeroData!.CallToActionButtonUrl, Is.Null);
        }

        [Test]
        public async Task GetIndustryPageDtoAsync_CallsHeroService_Once()
        {
            SetupContentService(CreateDefaultPage());

            await _service.GetIndustryPageDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        // ── Sidebox ──────────────────────────────────────────────────────────

        [Test]
        public async Task GetIndustryPageDtoAsync_WhenShowSideNavigationIsTrue_ReturnsSidebox()
        {
            var page = CreateDefaultPage();
            page.ShowSideNavigation = true;
            page.SideboxTitle = "Finance Services";

            SetupContentService(page);

            var sideboxDto = new SideBoxDto
            {
                Title = "Finance Services",
                Links = new[] { new LinkItemDto { Title = "Audit", URL = "/industries/finance/audit" } }
            };

            _pageUtilityMock
                .Setup(x => x.GetSideboxDtoAsync<IndustryService, Industry>(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Func<IndustryService, string>>(), It.IsAny<Func<Industry, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(sideboxDto);

            var result = await _service.GetIndustryPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Sidebox, Is.Not.Null);
                Assert.That(result.Sidebox!.Title, Is.EqualTo("Finance Services"));
            });
        }

        [Test]
        public async Task GetIndustryPageDtoAsync_WhenShowSideNavigationIsFalse_SideboxIsNull()
        {
            var page = CreateDefaultPage();
            page.ShowSideNavigation = false;

            SetupContentService(page);

            var result = await _service.GetIndustryPageDtoAsync(CancellationToken.None);

            Assert.That(result.Sidebox, Is.Null);
        }

        [Test]
        public async Task GetIndustryPageDtoAsync_WhenShowSideNavigationIsFalse_DoesNotCallPageUtility()
        {
            var page = CreateDefaultPage();
            page.ShowSideNavigation = false;

            SetupContentService(page);

            await _service.GetIndustryPageDtoAsync(CancellationToken.None);

            _pageUtilityMock.Verify(
                x => x.GetSideboxDtoAsync<IndustryService, Industry>(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Func<IndustryService, string>>(), It.IsAny<Func<Industry, string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Test]
        public async Task GetIndustryPageDtoAsync_WhenShowSideNavigationIsTrue_CallsPageUtilityOnce()
        {
            var page = CreateDefaultPage();
            page.ShowSideNavigation = true;

            SetupContentService(page);

            await _service.GetIndustryPageDtoAsync(CancellationToken.None);

            _pageUtilityMock.Verify(
                x => x.GetSideboxDtoAsync<IndustryService, Industry>(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Func<IndustryService, string>>(), It.IsAny<Func<Industry, string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task GetIndustryPageDtoAsync_PassesSideboxTitleAndIds_ToPageUtility()
        {
            var page = CreateDefaultPage();
            page.ShowSideNavigation = true;
            page.SideboxTitle = "My Finance Services";
            page.SystemFields.WebPageItemID = 600;
            page.SystemFields.WebPageItemParentID = 200;

            SetupContentService(page);

            string? capturedTitle = null;
            int capturedParentId = 0;
            int capturedSiblingParentId = 0;

            _pageUtilityMock
                .Setup(x => x.GetSideboxDtoAsync<IndustryService, Industry>(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Func<IndustryService, string>>(), It.IsAny<Func<Industry, string>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, int, int, Func<IndustryService, string>, Func<Industry, string>, CancellationToken>(
                    (title, parentId, siblingParentId, _, _, _) =>
                    {
                        capturedTitle = title;
                        capturedParentId = parentId;
                        capturedSiblingParentId = siblingParentId;
                    })
                .ReturnsAsync(new SideBoxDto());

            await _service.GetIndustryPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(capturedTitle, Is.EqualTo("My Finance Services"));
                Assert.That(capturedParentId, Is.EqualTo(600));
                Assert.That(capturedSiblingParentId, Is.EqualTo(200));
            });
        }

        // ── BaseContent mapping ──────────────────────────────────────────────

        [Test]
        public async Task GetIndustryPageDtoAsync_MapsAllBaseContentFields()
        {
            var contentGuid = Guid.NewGuid();
            var publishedOn = new DateTime(2024, 3, 15);
            var lastPublishedOn = new DateTime(2024, 9, 1);

            var page = CreateDefaultPage();
            page.SystemFields.ContentItemID = 88;
            page.SystemFields.ContentItemGUID = contentGuid;
            page.SystemFields.ContentItemName = "Finance Industry";
            page.SystemFields.ContentItemContentTypeID = 410;
            page.SystemFields.ContentItemIsSecured = true;
            page.SystemFields.ContentItemCommonDataContentLanguageID = 3;
            page.SystemFields.ContentItemCommonDataFirstPublishedWhen = publishedOn;
            page.SystemFields.ContentItemCommonDataLastPublishedWhen = lastPublishedOn;

            SetupContentService(page);

            var result = await _service.GetIndustryPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.BaseContent, Is.Not.Null);
                Assert.That(result.BaseContent!.ContentItemID, Is.EqualTo(88));
                Assert.That(result.BaseContent.ContentItemGUID, Is.EqualTo(contentGuid));
                Assert.That(result.BaseContent.ContentItemName, Is.EqualTo("Finance Industry"));
                Assert.That(result.BaseContent.ContentItemContentTypeID, Is.EqualTo(410));
                Assert.That(result.BaseContent.ContentItemIsSecured, Is.True);
                Assert.That(result.BaseContent.ContentItemCommonDataContentLanguageID, Is.EqualTo(3));
                Assert.That(result.BaseContent.ContentItemCommonDataFirstPublishedWhen, Is.EqualTo(publishedOn));
                Assert.That(result.BaseContent.ContentItemCommonDataLastPublishedWhen, Is.EqualTo(lastPublishedOn));
            });
        }

        // ── BaseWebPage mapping ──────────────────────────────────────────────

        [Test]
        public async Task GetIndustryPageDtoAsync_MapsAllBaseWebPageFields()
        {
            var webPageGuid = Guid.NewGuid();

            var page = CreateDefaultPage();
            page.SystemFields.WebPageItemGUID = webPageGuid;
            page.SystemFields.WebPageItemID = 450;
            page.SystemFields.WebPageItemName = "Finance Industry Page";
            page.SystemFields.WebPageItemWebsiteChannelId = 5;
            page.SystemFields.WebPageUrlPath = "/industries/finance";
            page.SystemFields.WebPageItemOrder = 2;
            page.SystemFields.WebPageItemTreePath = "/industries/finance";
            page.SystemFields.WebPageUrlPathContentLanguageID = 2;
            page.SystemFields.WebPageItemParentID = 10;

            SetupContentService(page);

            var result = await _service.GetIndustryPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.BaseWebPage, Is.Not.Null);
                Assert.That(result.BaseWebPage!.WebPageItemGUID, Is.EqualTo(webPageGuid));
                Assert.That(result.BaseWebPage.WebPageItemID, Is.EqualTo(450));
                Assert.That(result.BaseWebPage.WebPageItemName, Is.EqualTo("Finance Industry Page"));
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
        public async Task GetIndustryPageDtoAsync_PassesCancellationToken_ToHeroService()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Industry>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    token))
                .ReturnsAsync(CreateDefaultPage());

            CancellationToken capturedToken = default;
            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((_, ct) => capturedToken = ct)
                .ReturnsAsync(new HeroDto { Layout = "default" });

            await _service.GetIndustryPageDtoAsync(token);

            Assert.That(capturedToken, Is.EqualTo(token));
        }

        [Test]
        public async Task GetIndustryPageDtoAsync_WhenShowSideNavigation_PassesCancellationToken_ToPageUtility()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            var page = CreateDefaultPage();
            page.ShowSideNavigation = true;

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Industry>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    token))
                .ReturnsAsync(page);

            CancellationToken capturedToken = default;
            _pageUtilityMock
                .Setup(x => x.GetSideboxDtoAsync<IndustryService, Industry>(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Func<IndustryService, string>>(), It.IsAny<Func<Industry, string>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, int, int, Func<IndustryService, string>, Func<Industry, string>, CancellationToken>(
                    (_, _, _, _, _, ct) => capturedToken = ct)
                .ReturnsAsync(new SideBoxDto());

            await _service.GetIndustryPageDtoAsync(token);

            Assert.That(capturedToken, Is.EqualTo(token));
        }
    }
}
