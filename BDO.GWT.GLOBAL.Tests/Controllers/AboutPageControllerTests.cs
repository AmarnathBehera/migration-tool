using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class AboutPageControllerTests
    {
        private Mock<IPageContext> _pageContextMock = null!;
        private Mock<IAboutPageService<AboutPage>> _aboutPageServiceMock = null!;
        private AboutPageController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _pageContextMock = new Mock<IPageContext>();
            _aboutPageServiceMock = new Mock<IAboutPageService<AboutPage>>();
            _controller = new AboutPageController(_pageContextMock.Object, _aboutPageServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithFullAboutDtoMapping()
        {
            var aboutPage = new AboutPage();

            var expectedDto = new AboutDto
            {
                Hero = new HeroDto
                {
                    Layout = "Default",
                    Title = "About Us",
                    Description = "About Us Description",
                    BackgroundImage = "/images/hero.jpg",
                    BackgroundImageAltText = "Hero Alt",
                    MobileBackgroundImage = "/images/hero_mobile.jpg",
                    MobileBackgroundImageAltText = "Hero Mobile Alt",
                    CtaButtonType = "Primary",
                    CtaButtonText = "Learn More",
                    CtaButtonUrl = "/learn-more"
                },
                MetaData = new MetaDataDto
                {
                    MetaTitle = "About Meta Title",
                    MetaDescription = "About Meta Description",
                    MetaTeaserImageAltText = "Alt Text",
                    MetaCanonical = "https://example.com/about"
                },
                SearchAndSEO = new SearchAndSEODto
                {
                    IncludeInSitemap = true,
                    MetadataNofollow = false,
                    MetadataNoindex = false
                },
                OGMetaData = new OGMetaDataDto
                {
                    MetadataOGTitle = "OG About Title",
                    MetadataOGDescription = "OG About Description",
                    MetadataOGTwitterImageSizeCheckbox = false
                },
                BaseContent = new BaseContent
                {
                    ContentItemID = 2,
                    ContentItemGUID = Guid.NewGuid(),
                    ContentItemName = "About Content",
                    ContentItemContentTypeID = 102,
                    ContentItemIsSecured = false,
                    ContentItemCommonDataContentLanguageID = 1,
                    ContentItemCommonDataFirstPublishedWhen = DateTime.UtcNow.AddDays(-5),
                    ContentItemCommonDataLastPublishedWhen = DateTime.UtcNow
                },
                BaseWebPage = new BaseWebPage
                {
                    WebPageItemGUID = Guid.NewGuid(),
                    WebPageItemID = 200,
                    WebPageItemName = "About Page",
                    WebPageItemWebsiteChannelID = 2,
                    WebPageUrlPath = "/about",
                    WebPageItemOrder = 2,
                    WebPageItemTreePath = "/about",
                    WebPageUrlPathContentLanguageID = 1,
                    WebPageItemParentID = 0
                }
            };

            _pageContextMock
                .Setup(s => s.GetCurrentPageAsync<AboutPage>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(aboutPage);

            _aboutPageServiceMock
                .Setup(s => s.GetAboutDtoAsync(aboutPage, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.Multiple(() =>
            {
                Assert.That(viewResult, Is.Not.Null);
                var model = viewResult?.Model as AboutDto;
                Assert.That(model, Is.Not.Null);
                Assert.That(model?.Hero?.Title, Is.EqualTo(expectedDto.Hero.Title));
                Assert.That(model?.Hero?.Layout, Is.EqualTo(expectedDto.Hero.Layout));
                Assert.That(model?.Hero?.Description, Is.EqualTo(expectedDto.Hero.Description));
                Assert.That(model?.Hero?.CtaButtonText, Is.EqualTo(expectedDto.Hero.CtaButtonText));
                Assert.That(model?.MetaData?.MetaTitle, Is.EqualTo(expectedDto.MetaData.MetaTitle));
                Assert.That(model?.MetaData?.MetaDescription, Is.EqualTo(expectedDto.MetaData.MetaDescription));
                Assert.That(model?.SearchAndSEO?.IncludeInSitemap, Is.EqualTo(expectedDto.SearchAndSEO.IncludeInSitemap));
                Assert.That(model?.OGMetaData?.MetadataOGTitle, Is.EqualTo(expectedDto.OGMetaData.MetadataOGTitle));
                Assert.That(model?.BaseContent?.ContentItemName, Is.EqualTo(expectedDto.BaseContent.ContentItemName));
                Assert.That(model?.BaseWebPage?.WebPageItemName, Is.EqualTo(expectedDto.BaseWebPage.WebPageItemName));
            });

            _aboutPageServiceMock.Verify(
                s => s.GetAboutDtoAsync(aboutPage, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task Index_WhenPageContextReturnsNull_ReturnsNotFound()
        {
            _pageContextMock
                .Setup(s => s.GetCurrentPageAsync<AboutPage>(It.IsAny<CancellationToken>()))
                .ReturnsAsync((AboutPage?)null);

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.TypeOf<NotFoundResult>());

            _aboutPageServiceMock.Verify(
                s => s.GetAboutDtoAsync(It.IsAny<AboutPage>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Test]
        public async Task Index_PassesCorrectCancellationToken_ToService()
        {
            var token = new CancellationTokenSource().Token;
            var aboutPage = new AboutPage();

            _pageContextMock
                .Setup(s => s.GetCurrentPageAsync<AboutPage>(token))
                .ReturnsAsync(aboutPage);

            _aboutPageServiceMock
                .Setup(s => s.GetAboutDtoAsync(aboutPage, token))
                .ReturnsAsync(new AboutDto { Hero = new HeroDto { Layout = "Default" } });

            await _controller.Index(token);

            _aboutPageServiceMock.Verify(
                s => s.GetAboutDtoAsync(aboutPage, token),
                Times.Once
            );
        }

        [Test]
        public async Task Index_AlwaysReturnsViewResult_WhenPageExists()
        {
            var aboutPage = new AboutPage();

            _pageContextMock
                .Setup(s => s.GetCurrentPageAsync<AboutPage>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(aboutPage);

            _aboutPageServiceMock
                .Setup(s => s.GetAboutDtoAsync(aboutPage, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AboutDto { Hero = new HeroDto { Layout = "Default" } });

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.TypeOf<ViewResult>());
        }

        [Test]
        public async Task Index_ReturnsCorrectViewName()
        {
            var aboutPage = new AboutPage();

            _pageContextMock
                .Setup(s => s.GetCurrentPageAsync<AboutPage>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(aboutPage);

            _aboutPageServiceMock
                .Setup(s => s.GetAboutDtoAsync(aboutPage, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AboutDto { Hero = new HeroDto { Layout = "Default" } });

            var result = await _controller.Index(CancellationToken.None) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("~/Views/About/Index.cshtml"));
        }

        [Test]
        public void Index_WhenServiceThrows_ExceptionIsPropagated()
        {
            var aboutPage = new AboutPage();

            _pageContextMock
                .Setup(s => s.GetCurrentPageAsync<AboutPage>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(aboutPage);

            _aboutPageServiceMock
                .Setup(s => s.GetAboutDtoAsync(aboutPage, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            Assert.ThrowsAsync<Exception>(async () =>
                await _controller.Index(CancellationToken.None));
        }

        [Test]
        public async Task Index_PassesCorrectPageInstance_ToService()
        {
            var aboutPage = new AboutPage();

            _pageContextMock
                .Setup(s => s.GetCurrentPageAsync<AboutPage>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(aboutPage);

            _aboutPageServiceMock
                .Setup(s => s.GetAboutDtoAsync(aboutPage, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AboutDto { Hero = new HeroDto { Layout = "Default" } });

            await _controller.Index(CancellationToken.None);

            _aboutPageServiceMock.Verify(
                s => s.GetAboutDtoAsync(aboutPage, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }
}
