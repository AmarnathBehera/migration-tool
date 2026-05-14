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
    public class SectionIndustriesServiceTests
    {
        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IPageUtility> _pageUtilityMock = null!;
        private SectionIndustriesService _service = null!;

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
                .Setup(x => x.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
                    It.IsAny<int>(),
                    It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<Func<IndustryCategory, string?>>(),
                    It.IsAny<Func<IndustryCategory, string?>>(),
                    It.IsAny<Func<Industry, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<MultiLinkCardDto>());

            _service = new SectionIndustriesService(
                _heroServiceMock.Object,
                _contentServiceMock.Object,
                _pageUtilityMock.Object);
        }

        private static SectionIndustries CreateDefaultPage() => new SectionIndustries
        {
            GridTitle = "Industries",
            GridSummary = "Grid Summary",
            ShowMultiLinks = true,
            HeroTitle = "Hero Title",
            HeroLayout = "default",
            SystemFields = new WebPageFields
            {
                ContentItemID = 1,
                ContentItemGUID = Guid.NewGuid(),
                ContentItemName = "Section Industries",
                ContentItemContentTypeID = 201,
                ContentItemIsSecured = false,
                ContentItemCommonDataContentLanguageID = 1,
                ContentItemCommonDataFirstPublishedWhen = new DateTime(2024, 1, 1),
                ContentItemCommonDataLastPublishedWhen = new DateTime(2024, 6, 1),
                WebPageItemGUID = Guid.NewGuid(),
                WebPageItemID = 200,
                WebPageItemName = "Section Industries Page",
                WebPageItemWebsiteChannelId = 2,
                WebPageUrlPath = "/industries",
                WebPageItemOrder = 1,
                WebPageItemTreePath = "/industries",
                WebPageUrlPathContentLanguageID = 1,
                WebPageItemParentID = 0
            }
        };

        private void SetupContentService(SectionIndustries page)
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<SectionIndustries>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);
        }

        [Test]
        public async Task GetSectionIndustriesDtoAsync_WhenPageIsNull_ReturnsEmptyDto()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<SectionIndustries>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((SectionIndustries?)null);

            var result = await _service.GetSectionIndustriesDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Hero, Is.Null);
                Assert.That(result.MultiLinksCardsGrid, Is.Null);
            });
        }

        [Test]
        public async Task GetSectionIndustriesDtoAsync_WhenPageExists_ReturnsMappedDto()
        {
            var page = CreateDefaultPage();
            var cards = new[]
            {
                new MultiLinkCardDto { Title = "Category 1", Description = "Desc 1" }
            };

            SetupContentService(page);

            _pageUtilityMock
                .Setup(x => x.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
                    It.IsAny<int>(),
                    It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<Func<IndustryCategory, string?>>(),
                    It.IsAny<Func<IndustryCategory, string?>>(),
                    It.IsAny<Func<Industry, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cards);

            var result = await _service.GetSectionIndustriesDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Hero, Is.Not.Null);
                Assert.That(result.Hero.Title, Is.EqualTo(page.HeroTitle));
                Assert.That(result.MultiLinksCardsGrid, Is.Not.Null);
                Assert.That(result.MultiLinksCardsGrid.Cards, Is.EqualTo(cards));
            });
        }

        [Test]
        public async Task GetSectionIndustriesDtoAsync_WhenPageExists_MapsBaseContentCorrectly()
        {
            var contentGuid = Guid.NewGuid();
            var page = CreateDefaultPage();
            page.SystemFields.ContentItemID = 42;
            page.SystemFields.ContentItemGUID = contentGuid;
            page.SystemFields.ContentItemName = "Section Industries";

            SetupContentService(page);

            var result = await _service.GetSectionIndustriesDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.BaseContent, Is.Not.Null);
                Assert.That(result.BaseContent!.ContentItemID, Is.EqualTo(42));
                Assert.That(result.BaseContent.ContentItemGUID, Is.EqualTo(contentGuid));
                Assert.That(result.BaseContent.ContentItemName, Is.EqualTo("Section Industries"));
            });
        }

        [Test]
        public async Task GetSectionIndustriesDtoAsync_WhenPageExists_MapsBaseWebPageCorrectly()
        {
            var webPageGuid = Guid.NewGuid();
            var page = CreateDefaultPage();
            page.SystemFields.WebPageItemID = 200;
            page.SystemFields.WebPageItemGUID = webPageGuid;
            page.SystemFields.WebPageUrlPath = "/industries";

            SetupContentService(page);

            var result = await _service.GetSectionIndustriesDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.BaseWebPage, Is.Not.Null);
                Assert.That(result.BaseWebPage!.WebPageItemID, Is.EqualTo(200));
                Assert.That(result.BaseWebPage.WebPageItemGUID, Is.EqualTo(webPageGuid));
                Assert.That(result.BaseWebPage.WebPageUrlPath, Is.EqualTo("/industries"));
            });
        }

        [Test]
        public async Task GetSectionIndustriesDtoAsync_WhenPageExists_MapsGridTitleAndDescription()
        {
            var page = CreateDefaultPage();
            page.GridTitle = "Industries Grid";
            page.GridSummary = "Grid Description";
            page.ShowMultiLinks = true;

            SetupContentService(page);

            var result = await _service.GetSectionIndustriesDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.MultiLinksCardsGrid, Is.Not.Null);
                Assert.That(result.MultiLinksCardsGrid!.Title, Is.EqualTo("Industries Grid"));
                Assert.That(result.MultiLinksCardsGrid.Description, Is.EqualTo("Grid Description"));
                Assert.That(result.MultiLinksCardsGrid.IsMultiLinks, Is.True);
            });
        }

        [Test]
        public async Task GetSectionIndustriesDtoAsync_CallsHeroService_Once()
        {
            SetupContentService(CreateDefaultPage());

            await _service.GetSectionIndustriesDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task GetSectionIndustriesDtoAsync_WhenPageIsNull_DoesNotCallHeroServiceOrPageUtility()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<SectionIndustries>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((SectionIndustries?)null);

            await _service.GetSectionIndustriesDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Never
            );

            _pageUtilityMock.Verify(
                x => x.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
                    It.IsAny<int>(),
                    It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<Func<IndustryCategory, string?>>(),
                    It.IsAny<Func<IndustryCategory, string?>>(),
                    It.IsAny<Func<Industry, string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Test]
        public async Task GetSectionIndustriesDtoAsync_MapsHeroFieldsToHeroData()
        {
            var page = CreateDefaultPage();
            page.HeroTitle = "Industries Hero";
            page.HeroDescription = "Hero description";
            page.HeroLayout = "banner";
            page.HeroCallToActionButtonText = "Learn More";
            page.HeroCallToActionButtonType = "primary";

            SetupContentService(page);

            HeroData? capturedHeroData = null;
            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((data, _) => capturedHeroData = data)
                .ReturnsAsync(new HeroDto { Layout = "banner" });

            await _service.GetSectionIndustriesDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(capturedHeroData, Is.Not.Null);
                Assert.That(capturedHeroData!.Title, Is.EqualTo("Industries Hero"));
                Assert.That(capturedHeroData.Description, Is.EqualTo("Hero description"));
                Assert.That(capturedHeroData.Layout, Is.EqualTo("banner"));
                Assert.That(capturedHeroData.CallToActionButtonText, Is.EqualTo("Learn More"));
                Assert.That(capturedHeroData.CallToActionButtonType, Is.EqualTo("primary"));
            });
        }

        [Test]
        public async Task GetSectionIndustriesDtoAsync_WhenHeroImagesAreNull_HeroDataImagesAreNull()
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

            await _service.GetSectionIndustriesDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(capturedHeroData, Is.Not.Null);
                Assert.That(capturedHeroData!.BackgroundImage, Is.Null);
                Assert.That(capturedHeroData.MobileBackgroundImage, Is.Null);
            });
        }

        [Test]
        public async Task GetSectionIndustriesDtoAsync_WhenCtaUrlIsNull_HeroDataCtaButtonUrlIsNull()
        {
            var page = CreateDefaultPage();
            page.HeroCallToActionButtonUrl = null;

            SetupContentService(page);

            HeroData? capturedHeroData = null;
            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((data, _) => capturedHeroData = data)
                .ReturnsAsync(new HeroDto { Layout = "default" });

            await _service.GetSectionIndustriesDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(capturedHeroData, Is.Not.Null);
                Assert.That(capturedHeroData!.CallToActionButtonUrl, Is.Null);
            });
        }

        [Test]
        public async Task GetSectionIndustriesDtoAsync_WhenShowMultiLinksIsFalse_GridIsMultiLinksIsFalse()
        {
            var page = CreateDefaultPage();
            page.ShowMultiLinks = false;

            SetupContentService(page);

            var result = await _service.GetSectionIndustriesDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.MultiLinksCardsGrid, Is.Not.Null);
                Assert.That(result.MultiLinksCardsGrid!.IsMultiLinks, Is.False);
            });
        }

        [Test]
        public async Task GetSectionIndustriesDtoAsync_WhenCardsReturnsNull_GridCardsIsNull()
        {
            SetupContentService(CreateDefaultPage());

            _pageUtilityMock
                .Setup(x => x.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
                    It.IsAny<int>(),
                    It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<Func<IndustryCategory, string?>>(),
                    It.IsAny<Func<IndustryCategory, string?>>(),
                    It.IsAny<Func<Industry, string>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<MultiLinkCardDto[]>(null!));

            var result = await _service.GetSectionIndustriesDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.MultiLinksCardsGrid, Is.Not.Null);
                Assert.That(result.MultiLinksCardsGrid!.Cards, Is.Null);
            });
        }

        [Test]
        public async Task GetSectionIndustriesDtoAsync_WhenMultipleCardsReturned_AllCardsInGrid()
        {
            var cards = new[]
            {
                new MultiLinkCardDto { Title = "Finance" },
                new MultiLinkCardDto { Title = "Technology" },
                new MultiLinkCardDto { Title = "Healthcare" }
            };

            SetupContentService(CreateDefaultPage());

            _pageUtilityMock
                .Setup(x => x.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
                    It.IsAny<int>(),
                    It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<Func<IndustryCategory, string?>>(),
                    It.IsAny<Func<IndustryCategory, string?>>(),
                    It.IsAny<Func<Industry, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cards);

            var result = await _service.GetSectionIndustriesDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.MultiLinksCardsGrid!.Cards, Has.Length.EqualTo(3));
                Assert.That(result.MultiLinksCardsGrid.Cards![1].Title, Is.EqualTo("Technology"));
            });
        }

        [Test]
        public async Task GetSectionIndustriesDtoAsync_PassesCorrectWebPageItemId_ToPageUtility()
        {
            var page = CreateDefaultPage();
            page.SystemFields.WebPageItemID = 999;

            SetupContentService(page);

            int capturedParentId = 0;
            _pageUtilityMock
                .Setup(x => x.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
                    It.IsAny<int>(),
                    It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<Func<IndustryCategory, string?>>(),
                    It.IsAny<Func<IndustryCategory, string?>>(),
                    It.IsAny<Func<Industry, string>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<int, Func<IndustryCategory, string>, Func<IndustryCategory, string>, Func<IndustryCategory, string?>, Func<IndustryCategory, string?>, Func<Industry, string>, CancellationToken>(
                    (id, _, _, _, _, _, _) => capturedParentId = id)
                .ReturnsAsync(Array.Empty<MultiLinkCardDto>());

            await _service.GetSectionIndustriesDtoAsync(CancellationToken.None);

            Assert.That(capturedParentId, Is.EqualTo(999));
        }

        [Test]
        public async Task GetSectionIndustriesDtoAsync_PassesCancellationToken_ToPageUtility()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            SetupContentService(CreateDefaultPage());

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<SectionIndustries>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    token))
                .ReturnsAsync(CreateDefaultPage());

            CancellationToken capturedToken = default;
            _pageUtilityMock
                .Setup(x => x.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
                    It.IsAny<int>(),
                    It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<Func<IndustryCategory, string?>>(),
                    It.IsAny<Func<IndustryCategory, string?>>(),
                    It.IsAny<Func<Industry, string>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<int, Func<IndustryCategory, string>, Func<IndustryCategory, string>, Func<IndustryCategory, string?>, Func<IndustryCategory, string?>, Func<Industry, string>, CancellationToken>(
                    (_, _, _, _, _, _, ct) => capturedToken = ct)
                .ReturnsAsync(Array.Empty<MultiLinkCardDto>());

            await _service.GetSectionIndustriesDtoAsync(token);

            Assert.That(capturedToken, Is.EqualTo(token));
        }

        [Test]
        public async Task GetSectionIndustriesDtoAsync_MapsAllBaseContentFields()
        {
            var contentGuid = Guid.NewGuid();
            var publishedOn = new DateTime(2024, 3, 15);
            var lastPublishedOn = new DateTime(2024, 9, 1);

            var page = CreateDefaultPage();
            page.SystemFields.ContentItemID = 77;
            page.SystemFields.ContentItemGUID = contentGuid;
            page.SystemFields.ContentItemName = "Full Industries";
            page.SystemFields.ContentItemContentTypeID = 305;
            page.SystemFields.ContentItemIsSecured = true;
            page.SystemFields.ContentItemCommonDataContentLanguageID = 3;
            page.SystemFields.ContentItemCommonDataFirstPublishedWhen = publishedOn;
            page.SystemFields.ContentItemCommonDataLastPublishedWhen = lastPublishedOn;

            SetupContentService(page);

            var result = await _service.GetSectionIndustriesDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.BaseContent!.ContentItemID, Is.EqualTo(77));
                Assert.That(result.BaseContent.ContentItemGUID, Is.EqualTo(contentGuid));
                Assert.That(result.BaseContent.ContentItemName, Is.EqualTo("Full Industries"));
                Assert.That(result.BaseContent.ContentItemContentTypeID, Is.EqualTo(305));
                Assert.That(result.BaseContent.ContentItemIsSecured, Is.True);
                Assert.That(result.BaseContent.ContentItemCommonDataContentLanguageID, Is.EqualTo(3));
                Assert.That(result.BaseContent.ContentItemCommonDataFirstPublishedWhen, Is.EqualTo(publishedOn));
                Assert.That(result.BaseContent.ContentItemCommonDataLastPublishedWhen, Is.EqualTo(lastPublishedOn));
            });
        }

        [Test]
        public async Task GetSectionIndustriesDtoAsync_MapsAllBaseWebPageFields()
        {
            var webPageGuid = Guid.NewGuid();

            var page = CreateDefaultPage();
            page.SystemFields.WebPageItemGUID = webPageGuid;
            page.SystemFields.WebPageItemID = 321;
            page.SystemFields.WebPageItemName = "Industries Section";
            page.SystemFields.WebPageItemWebsiteChannelId = 5;
            page.SystemFields.WebPageUrlPath = "/global/industries";
            page.SystemFields.WebPageItemOrder = 3;
            page.SystemFields.WebPageItemTreePath = "/global/industries";
            page.SystemFields.WebPageUrlPathContentLanguageID = 2;
            page.SystemFields.WebPageItemParentID = 10;

            SetupContentService(page);

            var result = await _service.GetSectionIndustriesDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.BaseWebPage!.WebPageItemGUID, Is.EqualTo(webPageGuid));
                Assert.That(result.BaseWebPage.WebPageItemID, Is.EqualTo(321));
                Assert.That(result.BaseWebPage.WebPageItemName, Is.EqualTo("Industries Section"));
                Assert.That(result.BaseWebPage.WebPageItemWebsiteChannelID, Is.EqualTo(5));
                Assert.That(result.BaseWebPage.WebPageUrlPath, Is.EqualTo("/global/industries"));
                Assert.That(result.BaseWebPage.WebPageItemOrder, Is.EqualTo(3));
                Assert.That(result.BaseWebPage.WebPageItemTreePath, Is.EqualTo("/global/industries"));
                Assert.That(result.BaseWebPage.WebPageUrlPathContentLanguageID, Is.EqualTo(2));
                Assert.That(result.BaseWebPage.WebPageItemParentID, Is.EqualTo(10));
            });
        }

        [Test]
        public async Task GetSectionIndustriesDtoAsync_WhenPageIsNull_ReturnsEmptyDto_WithNullBaseContentAndWebPage()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<SectionIndustries>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((SectionIndustries?)null);

            var result = await _service.GetSectionIndustriesDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.BaseContent, Is.Null);
                Assert.That(result.BaseWebPage, Is.Null);
            });
        }

        [Test]
        public async Task GetSectionIndustriesDtoAsync_WhenCardsReturnsEmpty_GridCardsIsEmpty()
        {
            SetupContentService(CreateDefaultPage());

            _pageUtilityMock
                .Setup(x => x.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
                    It.IsAny<int>(),
                    It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<Func<IndustryCategory, string>>(),
                    It.IsAny<Func<IndustryCategory, string?>>(),
                    It.IsAny<Func<IndustryCategory, string?>>(),
                    It.IsAny<Func<Industry, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<MultiLinkCardDto>());

            var result = await _service.GetSectionIndustriesDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.MultiLinksCardsGrid, Is.Not.Null);
                Assert.That(result.MultiLinksCardsGrid!.Cards, Is.Empty);
            });
        }
    }
}
