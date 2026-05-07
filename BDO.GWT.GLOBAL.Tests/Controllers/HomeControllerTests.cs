using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class HomeControllerTests
    {
        private Mock<IHomePageService> _homePageServiceMock = null!;
        private HomeController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _homePageServiceMock = new Mock<IHomePageService>();
            _controller = new HomeController(_homePageServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithFullHomeDtoMapping()
        {
            var homeHeroSlides = new List<HomeHeroSlideDto>
            {
                new HomeHeroSlideDto
                {
                    Title = "Slide 1",
                    ImageURL = "slide1.jpg",
                    ImageAltText = "Alt Slide 1",
                    MobileImageURL = "slide1_mobile.jpg",
                    MobileImageAltText = "Alt Slide Mobile 1",
                    ThumbnailImageURL = "slide1_thumb.jpg",
                    ThumbnailImageAltText = "Alt Thumb 1",
                    Button = new LinkItemDto { Title = "Click Me", URL = "/action" }
                }
            };

            var expectedDto = new HomeDto
            {
                HomeHeroSlides = homeHeroSlides,
                MetaData = new MetaDataDto
                {
                    MetaTitle = "Test Meta Title",
                    MetaDescription = "Test Meta Description",
                    MetadataTeaserImage = "https://example.com/image.jpg",
                    MetaTeaserImageAltText = "Alt Text",
                    MetaCanonical = "https://example.com"
                },
                SearchAndSEO = new SearchAndSEODto
                {
                    IncludeInSitemap = true,
                    MetadataNofollow = true,
                    MetadataNoindex = false
                },
                OGMetaData = new OGMetaDataDto
                {
                    MetadataOGTitle = "OG Title",
                    MetadataOGDescription = "OG Description",
                    MetadataOGImage = "https://example.com/og-image.jpg",
                    MetadataOGTwitterImage = "https://example.com/twitter-image.jpg",
                    MetadataOGTwitterImageSizeCheckbox = true
                },
                BaseContent = new BaseContent
                {
                    ContentItemID = 1,
                    ContentItemGUID = Guid.NewGuid(),
                    ContentItemName = "Base Content",
                    ContentItemContentTypeID = 101,
                    ContentItemIsSecured = false,
                    ContentItemCommonDataContentLanguageID = 1,
                    ContentItemCommonDataFirstPublishedWhen = DateTime.UtcNow.AddDays(-10),
                    ContentItemCommonDataLastPublishedWhen = DateTime.UtcNow
                },
                BaseWebPage = new BaseWebPage
                {
                    WebPageItemGUID = Guid.NewGuid(),
                    WebPageItemID = 100,
                    WebPageItemName = "Home Page",
                    WebPageItemWebsiteChannelID = 2,
                    WebPageUrlPath = "/home",
                    WebPageItemOrder = 1,
                    WebPageItemTreePath = "/",
                    WebPageUrlPathContentLanguageID = 1,
                    WebPageItemParentID = 0
                }
            };

            _homePageServiceMock
                .Setup(s => s.GetHomePageDtoAsync(It.IsAny<HomePage?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.Multiple(() =>
            {
                Assert.That(viewResult, Is.Not.Null);
                var model = viewResult?.Model as HomeDto;
                Assert.That(model, Is.Not.Null);
                Assert.That(model?.HomeHeroSlides?.Count(), Is.EqualTo(homeHeroSlides.Count));
                Assert.That(model?.MetaData?.MetaTitle, Is.EqualTo(expectedDto.MetaData.MetaTitle));
                Assert.That(model?.MetaData?.MetaDescription, Is.EqualTo(expectedDto.MetaData.MetaDescription));
                Assert.That(model?.SearchAndSEO?.IncludeInSitemap, Is.EqualTo(expectedDto.SearchAndSEO.IncludeInSitemap));
                Assert.That(model?.OGMetaData?.MetadataOGTitle, Is.EqualTo(expectedDto.OGMetaData.MetadataOGTitle));
                Assert.That(model?.BaseContent?.ContentItemName, Is.EqualTo(expectedDto.BaseContent.ContentItemName));
                Assert.That(model?.BaseWebPage?.WebPageItemName, Is.EqualTo(expectedDto.BaseWebPage.WebPageItemName));
            });

            _homePageServiceMock.Verify(
                s => s.GetHomePageDtoAsync(It.IsAny<HomePage?>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task Index_WhenHomePageIsNull_ReturnsViewWithNullModel()
        {
            _homePageServiceMock
                .Setup(s => s.GetHomePageDtoAsync(It.IsAny<HomePage?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((HomeDto?)null);

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult.Model, Is.Null);

            _homePageServiceMock.Verify(
                s => s.GetHomePageDtoAsync(It.IsAny<HomePage?>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task Index_PassesCorrectCancellationToken_ToService()
        {
            var token = new CancellationTokenSource().Token;

            _homePageServiceMock
                .Setup(s => s.GetHomePageDtoAsync(It.IsAny<HomePage?>(), token))
                .ReturnsAsync(new HomeDto());

            await _controller.Index(token);

            _homePageServiceMock.Verify(
                s => s.GetHomePageDtoAsync(It.IsAny<HomePage?>(), token),
                Times.Once
            );
        }

        [Test]
        public async Task Index_PassesNullHomePage_ToService()
        {
            _homePageServiceMock
                .Setup(s => s.GetHomePageDtoAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HomeDto());

            await _controller.Index(CancellationToken.None);

            _homePageServiceMock.Verify(
                s => s.GetHomePageDtoAsync(null, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public void Index_WhenServiceThrows_ExceptionIsPropagated()
        {
            _homePageServiceMock
                .Setup(s => s.GetHomePageDtoAsync(It.IsAny<HomePage?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            Assert.ThrowsAsync<Exception>(async () =>
                await _controller.Index(CancellationToken.None));
        }

        [Test]
        public async Task Index_AlwaysReturnsViewResult()
        {
            _homePageServiceMock
                .Setup(s => s.GetHomePageDtoAsync(It.IsAny<HomePage?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HomeDto());

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.TypeOf<ViewResult>());
        }

        [Test]
        public async Task Index_ReturnsDefaultViewName()
        {
            _homePageServiceMock
                .Setup(s => s.GetHomePageDtoAsync(It.IsAny<HomePage?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HomeDto());

            var result = await _controller.Index(CancellationToken.None) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.Null);
        }
    }
}