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
    public class IndustryControllerTests
    {
        private Mock<IIndustryPageService> _industryPageServiceMock = null!;
        private IndustryController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _industryPageServiceMock = new Mock<IIndustryPageService>();
            _controller = new IndustryController(_industryPageServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithFullIndustryDto()
        {
            var expectedDto = new IndustryDto
            {
                Title = "Finance",
                Description = "Finance industry description",
                Hero = new HeroDto
                {
                    Layout = "default",
                    Title = "Finance Industry Hero",
                    Description = "Hero Description",
                    CtaButtonText = "Explore Finance",
                    CtaButtonUrl = "/industries/finance"
                },
                Sidebox = new SideBoxDto
                {
                    Title = "Finance Services",
                    Links = new[]
                    {
                        new LinkItemDto { Title = "Audit", URL = "/industries/finance/audit" }
                    }
                },
                BaseContent = new BaseContent
                {
                    ContentItemID = 1,
                    ContentItemGUID = Guid.NewGuid(),
                    ContentItemName = "Finance Industry",
                    ContentItemContentTypeID = 401,
                    ContentItemIsSecured = false,
                    ContentItemCommonDataContentLanguageID = 1,
                    ContentItemCommonDataFirstPublishedWhen = DateTime.UtcNow.AddDays(-5),
                    ContentItemCommonDataLastPublishedWhen = DateTime.UtcNow
                },
                BaseWebPage = new BaseWebPage
                {
                    WebPageItemGUID = Guid.NewGuid(),
                    WebPageItemID = 400,
                    WebPageItemName = "Finance Industry Page",
                    WebPageItemWebsiteChannelID = 2,
                    WebPageUrlPath = "/industries/finance",
                    WebPageItemOrder = 1,
                    WebPageItemTreePath = "/industries/finance",
                    WebPageUrlPathContentLanguageID = 1,
                    WebPageItemParentID = 10
                }
            };

            _industryPageServiceMock
                .Setup(s => s.GetIndustryPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.Multiple(() =>
            {
                Assert.That(viewResult, Is.Not.Null);
                var model = viewResult?.Model as IndustryDto;
                Assert.That(model, Is.Not.Null);
                Assert.That(model?.Title, Is.EqualTo(expectedDto.Title));
                Assert.That(model?.Description, Is.EqualTo(expectedDto.Description));
                Assert.That(model?.Hero?.Title, Is.EqualTo(expectedDto.Hero.Title));
                Assert.That(model?.Sidebox?.Title, Is.EqualTo(expectedDto.Sidebox.Title));
                Assert.That(model?.BaseContent?.ContentItemName, Is.EqualTo(expectedDto.BaseContent.ContentItemName));
                Assert.That(model?.BaseWebPage?.WebPageUrlPath, Is.EqualTo(expectedDto.BaseWebPage.WebPageUrlPath));
            });

            _industryPageServiceMock.Verify(
                s => s.GetIndustryPageDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task Index_WhenServiceReturnsEmptyDto_ReturnsViewWithEmptyModel()
        {
            _industryPageServiceMock
                .Setup(s => s.GetIndustryPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IndustryDto());

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult?.Model as IndustryDto, Is.Not.Null);

            _industryPageServiceMock.Verify(
                s => s.GetIndustryPageDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task Index_PassesCorrectCancellationToken_ToService()
        {
            var token = new CancellationTokenSource().Token;

            _industryPageServiceMock
                .Setup(s => s.GetIndustryPageDtoAsync(token))
                .ReturnsAsync(new IndustryDto());

            await _controller.Index(token);

            _industryPageServiceMock.Verify(
                s => s.GetIndustryPageDtoAsync(token),
                Times.Once
            );
        }

        [Test]
        public void Index_WhenServiceThrows_ExceptionIsPropagated()
        {
            _industryPageServiceMock
                .Setup(s => s.GetIndustryPageDtoAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Service error"));

            Assert.ThrowsAsync<Exception>(async () =>
                await _controller.Index(CancellationToken.None));
        }

        [Test]
        public async Task Index_AlwaysReturnsViewResult()
        {
            _industryPageServiceMock
                .Setup(s => s.GetIndustryPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IndustryDto());

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.TypeOf<ViewResult>());
        }

        [Test]
        public async Task Index_ReturnsCorrectViewName()
        {
            _industryPageServiceMock
                .Setup(s => s.GetIndustryPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IndustryDto());

            var result = await _controller.Index(CancellationToken.None) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ViewName, Is.EqualTo("~/Views/Industry/Index.cshtml"));
        }

        [Test]
        public async Task Index_ServiceIsCalledExactlyOnce()
        {
            _industryPageServiceMock
                .Setup(s => s.GetIndustryPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IndustryDto());

            await _controller.Index(CancellationToken.None);

            _industryPageServiceMock.Verify(
                s => s.GetIndustryPageDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task Index_ModelContainsHeroDto()
        {
            var expectedDto = new IndustryDto
            {
                Hero = new HeroDto
                {
                    Layout = "default",
                    Title = "Finance Industry Hero",
                    Description = "Hero Description",
                    CtaButtonText = "Explore",
                    CtaButtonUrl = "/industries/finance"
                }
            };

            _industryPageServiceMock
                .Setup(s => s.GetIndustryPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None) as ViewResult;
            var model = result?.Model as IndustryDto;

            Assert.Multiple(() =>
            {
                Assert.That(model?.Hero, Is.Not.Null);
                Assert.That(model?.Hero?.Title, Is.EqualTo("Finance Industry Hero"));
                Assert.That(model?.Hero?.CtaButtonText, Is.EqualTo("Explore"));
                Assert.That(model?.Hero?.CtaButtonUrl, Is.EqualTo("/industries/finance"));
            });
        }

        [Test]
        public async Task Index_ModelContainsSidebox_WhenSideboxIsNotNull()
        {
            var expectedDto = new IndustryDto
            {
                Sidebox = new SideBoxDto
                {
                    Title = "Finance Services",
                    Links = new[]
                    {
                        new LinkItemDto { Title = "Audit", URL = "/industries/finance/audit" },
                        new LinkItemDto { Title = "Tax", URL = "/industries/finance/tax" }
                    }
                }
            };

            _industryPageServiceMock
                .Setup(s => s.GetIndustryPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None) as ViewResult;
            var model = result?.Model as IndustryDto;

            Assert.Multiple(() =>
            {
                Assert.That(model?.Sidebox, Is.Not.Null);
                Assert.That(model?.Sidebox?.Title, Is.EqualTo("Finance Services"));
                Assert.That(model?.Sidebox?.Links, Has.Exactly(2).Items);
            });
        }

        [Test]
        public async Task Index_ModelSideboxIsNull_WhenShowSideNavigationIsFalse()
        {
            _industryPageServiceMock
                .Setup(s => s.GetIndustryPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IndustryDto { Sidebox = null });

            var result = await _controller.Index(CancellationToken.None) as ViewResult;
            var model = result?.Model as IndustryDto;

            Assert.That(model?.Sidebox, Is.Null);
        }

        [Test]
        public async Task Index_ModelRetainsSameReferenceAsServiceReturn()
        {
            var expectedDto = new IndustryDto { Title = "Finance" };

            _industryPageServiceMock
                .Setup(s => s.GetIndustryPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None) as ViewResult;

            Assert.That(result?.Model, Is.SameAs(expectedDto));
        }
    }
}
