using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
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
    public class IndustryServicePageServiceTests
    {
        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private IndustryServicePageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _heroServiceMock = new Mock<IHeroService>();
            _contentServiceMock = new Mock<IContentService>();

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

            _service = new IndustryServicePageService(
                _contentServiceMock.Object,
                _heroServiceMock.Object);
        }

        private static IndustryService CreateDefaultPage() => new IndustryService
        {
            DescriptionTitle = "Finance Audit",
            DescriptionBody = "Audit service description",
            HeroTitle = "Finance Audit Hero",
            HeroLayout = "default",
            HeroDescription = "Hero Description",
            HeroCallToActionButtonText = "Learn More",
            HeroCallToActionButtonType = "primary",
            SystemFields = new WebPageFields
            {
                ContentItemID = 1,
                ContentItemGUID = Guid.NewGuid(),
                ContentItemName = "Finance Audit Service",
                ContentItemContentTypeID = 501,
                ContentItemIsSecured = false,
                ContentItemCommonDataContentLanguageID = 1,
                ContentItemCommonDataFirstPublishedWhen = new DateTime(2024, 1, 1),
                ContentItemCommonDataLastPublishedWhen = new DateTime(2024, 6, 1),
                WebPageItemGUID = Guid.NewGuid(),
                WebPageItemID = 500,
                WebPageItemName = "Finance Audit Service Page",
                WebPageItemWebsiteChannelId = 2,
                WebPageUrlPath = "/industries/finance/audit",
                WebPageItemOrder = 1,
                WebPageItemTreePath = "/industries/finance/audit",
                WebPageUrlPathContentLanguageID = 1,
                WebPageItemParentID = 20
            }
        };

        private void SetupContentService(IndustryService page)
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<IndustryService>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);
        }

        // ── Null / Empty ─────────────────────────────────────────────────────

        [Test]
        public async Task GetIndustryServiceDtoAsync_WhenPageIsNull_ReturnsEmptyDto()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<IndustryService>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IndustryService?)null);

            var result = await _service.GetIndustryServiceDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Hero, Is.Null);
                Assert.That(result.BaseContent, Is.Null);
                Assert.That(result.BaseWebPage, Is.Null);
            });
        }

        [Test]
        public async Task GetIndustryServiceDtoAsync_WhenPageIsNull_DoesNotCallHeroService()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<IndustryService>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IndustryService?)null);

            await _service.GetIndustryServiceDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Test]
        public async Task GetIndustryServiceDtoAsync_WhenPageExists_ReturnsMappedDto()
        {
            SetupContentService(CreateDefaultPage());

            var result = await _service.GetIndustryServiceDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Hero, Is.Not.Null);
                Assert.That(result.Title, Is.EqualTo("Finance Audit"));
                Assert.That(result.Description, Is.EqualTo("Audit service description"));
            });
        }

        [Test]
        public async Task GetIndustryServiceDtoAsync_MapsDescriptionTitleAndBody()
        {
            var page = CreateDefaultPage();
            page.DescriptionTitle = "Custom Service Title";
            page.DescriptionBody = "Custom Service Description";

            SetupContentService(page);

            var result = await _service.GetIndustryServiceDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.EqualTo("Custom Service Title"));
                Assert.That(result.Description, Is.EqualTo("Custom Service Description"));
            });
        }

        // ── Hero mapping ─────────────────────────────────────────────────────

        [Test]
        public async Task GetIndustryServiceDtoAsync_MapsHeroFieldsToHeroData()
        {
            var page = CreateDefaultPage();
            page.HeroTitle = "Finance Audit Hero Title";
            page.HeroDescription = "Finance Audit Hero Description";
            page.HeroLayout = "banner";
            page.HeroCallToActionButtonText = "Explore Audit";
            page.HeroCallToActionButtonType = "secondary";

            SetupContentService(page);

            HeroData? capturedHeroData = null;
            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((data, _) => capturedHeroData = data)
                .ReturnsAsync(new HeroDto { Layout = "banner" });

            await _service.GetIndustryServiceDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(capturedHeroData, Is.Not.Null);
                Assert.That(capturedHeroData!.Title, Is.EqualTo("Finance Audit Hero Title"));
                Assert.That(capturedHeroData.Description, Is.EqualTo("Finance Audit Hero Description"));
                Assert.That(capturedHeroData.Layout, Is.EqualTo("banner"));
                Assert.That(capturedHeroData.CallToActionButtonText, Is.EqualTo("Explore Audit"));
                Assert.That(capturedHeroData.CallToActionButtonType, Is.EqualTo("secondary"));
            });
        }

        [Test]
        public async Task GetIndustryServiceDtoAsync_WhenHeroImagesAreNull_HeroDataImagesAreNull()
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

            await _service.GetIndustryServiceDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(capturedHeroData, Is.Not.Null);
                Assert.That(capturedHeroData!.BackgroundImage, Is.Null);
                Assert.That(capturedHeroData.MobileBackgroundImage, Is.Null);
            });
        }

        [Test]
        public async Task GetIndustryServiceDtoAsync_WhenCtaUrlIsNull_HeroDataCtaButtonUrlIsNull()
        {
            var page = CreateDefaultPage();
            page.HeroCallToActionButtonUrl = null;

            SetupContentService(page);

            HeroData? capturedHeroData = null;
            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((data, _) => capturedHeroData = data)
                .ReturnsAsync(new HeroDto { Layout = "default" });

            await _service.GetIndustryServiceDtoAsync(CancellationToken.None);

            Assert.That(capturedHeroData, Is.Not.Null);
            Assert.That(capturedHeroData!.CallToActionButtonUrl, Is.Null);
        }

        [Test]
        public async Task GetIndustryServiceDtoAsync_CallsHeroService_Once()
        {
            SetupContentService(CreateDefaultPage());

            await _service.GetIndustryServiceDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        // ── BaseContent mapping ──────────────────────────────────────────────

        [Test]
        public async Task GetIndustryServiceDtoAsync_MapsAllBaseContentFields()
        {
            var contentGuid = Guid.NewGuid();
            var publishedOn = new DateTime(2024, 3, 15);
            var lastPublishedOn = new DateTime(2024, 9, 1);

            var page = CreateDefaultPage();
            page.SystemFields.ContentItemID = 99;
            page.SystemFields.ContentItemGUID = contentGuid;
            page.SystemFields.ContentItemName = "Finance Audit Service";
            page.SystemFields.ContentItemContentTypeID = 510;
            page.SystemFields.ContentItemIsSecured = true;
            page.SystemFields.ContentItemCommonDataContentLanguageID = 3;
            page.SystemFields.ContentItemCommonDataFirstPublishedWhen = publishedOn;
            page.SystemFields.ContentItemCommonDataLastPublishedWhen = lastPublishedOn;

            SetupContentService(page);

            var result = await _service.GetIndustryServiceDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.BaseContent, Is.Not.Null);
                Assert.That(result.BaseContent!.ContentItemID, Is.EqualTo(99));
                Assert.That(result.BaseContent.ContentItemGUID, Is.EqualTo(contentGuid));
                Assert.That(result.BaseContent.ContentItemName, Is.EqualTo("Finance Audit Service"));
                Assert.That(result.BaseContent.ContentItemContentTypeID, Is.EqualTo(510));
                Assert.That(result.BaseContent.ContentItemIsSecured, Is.True);
                Assert.That(result.BaseContent.ContentItemCommonDataContentLanguageID, Is.EqualTo(3));
                Assert.That(result.BaseContent.ContentItemCommonDataFirstPublishedWhen, Is.EqualTo(publishedOn));
                Assert.That(result.BaseContent.ContentItemCommonDataLastPublishedWhen, Is.EqualTo(lastPublishedOn));
            });
        }

        // ── BaseWebPage mapping ──────────────────────────────────────────────

        [Test]
        public async Task GetIndustryServiceDtoAsync_MapsAllBaseWebPageFields()
        {
            var webPageGuid = Guid.NewGuid();

            var page = CreateDefaultPage();
            page.SystemFields.WebPageItemGUID = webPageGuid;
            page.SystemFields.WebPageItemID = 550;
            page.SystemFields.WebPageItemName = "Finance Audit Service Page";
            page.SystemFields.WebPageItemWebsiteChannelId = 5;
            page.SystemFields.WebPageUrlPath = "/industries/finance/audit";
            page.SystemFields.WebPageItemOrder = 3;
            page.SystemFields.WebPageItemTreePath = "/industries/finance/audit";
            page.SystemFields.WebPageUrlPathContentLanguageID = 2;
            page.SystemFields.WebPageItemParentID = 20;

            SetupContentService(page);

            var result = await _service.GetIndustryServiceDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.BaseWebPage, Is.Not.Null);
                Assert.That(result.BaseWebPage!.WebPageItemGUID, Is.EqualTo(webPageGuid));
                Assert.That(result.BaseWebPage.WebPageItemID, Is.EqualTo(550));
                Assert.That(result.BaseWebPage.WebPageItemName, Is.EqualTo("Finance Audit Service Page"));
                Assert.That(result.BaseWebPage.WebPageItemWebsiteChannelID, Is.EqualTo(5));
                Assert.That(result.BaseWebPage.WebPageUrlPath, Is.EqualTo("/industries/finance/audit"));
                Assert.That(result.BaseWebPage.WebPageItemOrder, Is.EqualTo(3));
                Assert.That(result.BaseWebPage.WebPageItemTreePath, Is.EqualTo("/industries/finance/audit"));
                Assert.That(result.BaseWebPage.WebPageUrlPathContentLanguageID, Is.EqualTo(2));
                Assert.That(result.BaseWebPage.WebPageItemParentID, Is.EqualTo(20));
            });
        }

        // ── CancellationToken ────────────────────────────────────────────────

        [Test]
        public async Task GetIndustryServiceDtoAsync_PassesCancellationToken_ToHeroService()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<IndustryService>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    token))
                .ReturnsAsync(CreateDefaultPage());

            CancellationToken capturedToken = default;
            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((_, ct) => capturedToken = ct)
                .ReturnsAsync(new HeroDto { Layout = "default" });

            await _service.GetIndustryServiceDtoAsync(token);

            Assert.That(capturedToken, Is.EqualTo(token));
        }

        [Test]
        public async Task GetIndustryServiceDtoAsync_WhenPageIsNull_DoesNotThrow()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<IndustryService>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IndustryService?)null);

            Assert.DoesNotThrowAsync(async () =>
                await _service.GetIndustryServiceDtoAsync(CancellationToken.None));
        }
    }
}
