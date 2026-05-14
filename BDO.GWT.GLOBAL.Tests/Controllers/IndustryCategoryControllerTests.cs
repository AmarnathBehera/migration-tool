using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
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
    public class IndustryCategoryControllerTests
    {
        private Mock<IIndustryCategoryPageService> _industryCategoryPageServiceMock = null!;
        private IndustryCategoryController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _industryCategoryPageServiceMock = new Mock<IIndustryCategoryPageService>();
            _controller = new IndustryCategoryController(_industryCategoryPageServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithFullIndustryCategoryDto()
        {
            var expectedDto = new IndustryCategoryDto
            {
                Title = "Finance",
                Description = "Finance category description",
                Hero = new HeroDto
                {
                    Layout = "default",
                    Title = "Finance Hero",
                    Description = "Hero Description",
                    CtaButtonText = "Explore Finance"
                },
                Sidebox = new SideBoxDto
                {
                    Title = "Related Industries",
                    Links = new[]
                    {
                        new LinkItemDto { Title = "Technology", URL = "/industries/technology" }
                    }
                },
                BaseContent = new BaseContent
                {
                    ContentItemID = 1,
                    ContentItemGUID = Guid.NewGuid(),
                    ContentItemName = "Finance Category",
                    ContentItemContentTypeID = 301,
                    ContentItemIsSecured = false,
                    ContentItemCommonDataContentLanguageID = 1,
                    ContentItemCommonDataFirstPublishedWhen = DateTime.UtcNow.AddDays(-5),
                    ContentItemCommonDataLastPublishedWhen = DateTime.UtcNow
                },
                BaseWebPage = new BaseWebPage
                {
                    WebPageItemGUID = Guid.NewGuid(),
                    WebPageItemID = 300,
                    WebPageItemName = "Finance Industry Category",
                    WebPageItemWebsiteChannelID = 2,
                    WebPageUrlPath = "/industries/finance",
                    WebPageItemOrder = 1,
                    WebPageItemTreePath = "/industries/finance",
                    WebPageUrlPathContentLanguageID = 1,
                    WebPageItemParentID = 10
                }
            };

            _industryCategoryPageServiceMock
                .Setup(s => s.GetIndustryCategoryPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.Multiple(() =>
            {
                Assert.That(viewResult, Is.Not.Null);
                var model = viewResult?.Model as IndustryCategoryDto;
                Assert.That(model, Is.Not.Null);
                Assert.That(model?.Title, Is.EqualTo(expectedDto.Title));
                Assert.That(model?.Description, Is.EqualTo(expectedDto.Description));
                Assert.That(model?.Hero?.Title, Is.EqualTo(expectedDto.Hero.Title));
                Assert.That(model?.Sidebox?.Title, Is.EqualTo(expectedDto.Sidebox.Title));
                Assert.That(model?.BaseContent?.ContentItemName, Is.EqualTo(expectedDto.BaseContent.ContentItemName));
                Assert.That(model?.BaseWebPage?.WebPageUrlPath, Is.EqualTo(expectedDto.BaseWebPage.WebPageUrlPath));
            });

            _industryCategoryPageServiceMock.Verify(
                s => s.GetIndustryCategoryPageDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task Index_WhenServiceReturnsEmptyDto_ReturnsViewWithEmptyModel()
        {
            var emptyDto = new IndustryCategoryDto();

            _industryCategoryPageServiceMock
                .Setup(s => s.GetIndustryCategoryPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyDto);

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult?.Model as IndustryCategoryDto, Is.Not.Null);

            _industryCategoryPageServiceMock.Verify(
                s => s.GetIndustryCategoryPageDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task Index_PassesCorrectCancellationToken_ToService()
        {
            var token = new CancellationTokenSource().Token;

            _industryCategoryPageServiceMock
                .Setup(s => s.GetIndustryCategoryPageDtoAsync(token))
                .ReturnsAsync(new IndustryCategoryDto());

            await _controller.Index(token);

            _industryCategoryPageServiceMock.Verify(
                s => s.GetIndustryCategoryPageDtoAsync(token),
                Times.Once
            );
        }

        [Test]
        public void Index_WhenServiceThrows_ExceptionIsPropagated()
        {
            _industryCategoryPageServiceMock
                .Setup(s => s.GetIndustryCategoryPageDtoAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Service error"));

            Assert.ThrowsAsync<Exception>(async () =>
                await _controller.Index(CancellationToken.None));
        }

        [Test]
        public async Task Index_AlwaysReturnsViewResult()
        {
            _industryCategoryPageServiceMock
                .Setup(s => s.GetIndustryCategoryPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IndustryCategoryDto());

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.TypeOf<ViewResult>());
        }

        [Test]
        public async Task Index_ReturnsCorrectViewName()
        {
            _industryCategoryPageServiceMock
                .Setup(s => s.GetIndustryCategoryPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IndustryCategoryDto());

            var result = await _controller.Index(CancellationToken.None) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ViewName, Is.EqualTo("~/Views/IndustryCategory/Index.cshtml"));
        }

        [Test]
        public async Task Index_ServiceIsCalledExactlyOnce()
        {
            _industryCategoryPageServiceMock
                .Setup(s => s.GetIndustryCategoryPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IndustryCategoryDto());

            await _controller.Index(CancellationToken.None);

            _industryCategoryPageServiceMock.Verify(
                s => s.GetIndustryCategoryPageDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task Index_ModelContainsHeroDto()
        {
            var expectedDto = new IndustryCategoryDto
            {
                Hero = new HeroDto
                {
                    Layout = "default",
                    Title = "Finance Hero",
                    Description = "Hero Description",
                    CtaButtonText = "Explore",
                    CtaButtonUrl = "/industries/finance"
                }
            };

            _industryCategoryPageServiceMock
                .Setup(s => s.GetIndustryCategoryPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None) as ViewResult;
            var model = result?.Model as IndustryCategoryDto;

            Assert.Multiple(() =>
            {
                Assert.That(model?.Hero, Is.Not.Null);
                Assert.That(model?.Hero?.Title, Is.EqualTo("Finance Hero"));
                Assert.That(model?.Hero?.CtaButtonText, Is.EqualTo("Explore"));
                Assert.That(model?.Hero?.CtaButtonUrl, Is.EqualTo("/industries/finance"));
            });
        }

        [Test]
        public async Task Index_ModelContainsSidebox_WhenSideboxIsNotNull()
        {
            var expectedDto = new IndustryCategoryDto
            {
                Sidebox = new SideBoxDto
                {
                    Title = "Industries",
                    Links = new[]
                    {
                        new LinkItemDto { Title = "Finance", URL = "/industries/finance" },
                        new LinkItemDto { Title = "Technology", URL = "/industries/technology" }
                    }
                }
            };

            _industryCategoryPageServiceMock
                .Setup(s => s.GetIndustryCategoryPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None) as ViewResult;
            var model = result?.Model as IndustryCategoryDto;

            Assert.Multiple(() =>
            {
                Assert.That(model?.Sidebox, Is.Not.Null);
                Assert.That(model?.Sidebox?.Title, Is.EqualTo("Industries"));
                Assert.That(model?.Sidebox?.Links, Has.Exactly(2).Items);
            });
        }

        [Test]
        public async Task Index_ModelSideboxIsNull_WhenShowSideNavigationIsFalse()
        {
            var expectedDto = new IndustryCategoryDto { Sidebox = null };

            _industryCategoryPageServiceMock
                .Setup(s => s.GetIndustryCategoryPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None) as ViewResult;
            var model = result?.Model as IndustryCategoryDto;

            Assert.That(model?.Sidebox, Is.Null);
        }

        [Test]
        public async Task Index_ModelRetainsSameReferenceAsServiceReturn()
        {
            var expectedDto = new IndustryCategoryDto { Title = "Finance" };

            _industryCategoryPageServiceMock
                .Setup(s => s.GetIndustryCategoryPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None) as ViewResult;

            Assert.That(result?.Model, Is.SameAs(expectedDto));
        }
    }
}
