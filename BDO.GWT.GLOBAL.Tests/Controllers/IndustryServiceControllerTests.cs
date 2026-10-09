using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Core;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class IndustryServiceControllerTests
    {
        private Mock<IIndustryServicePageService> _industryServicePageServiceMock = null!;
        private IndustryServiceController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _industryServicePageServiceMock = new Mock<IIndustryServicePageService>();
            _controller = new IndustryServiceController(_industryServicePageServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithFullIndustryServiceDto()
        {
            var expectedDto = new IndustryServiceDto
            {
                Title = "Finance Audit",
                Description = "Audit service description",
                Hero = new HeroDto
                {
                    Layout = "default",
                    Title = "Finance Audit Hero",
                    Description = "Hero Description",
                    CtaButtonText = "Learn More",
                    CtaButtonUrl = "/industries/finance/audit"
                },
                BaseContent = new BaseContent
                {
                    ContentItemID = 1,
                    ContentItemGUID = Guid.NewGuid(),
                    ContentItemName = "Finance Audit Service",
                    ContentItemContentTypeID = 501,
                    ContentItemIsSecured = false,
                    ContentItemCommonDataContentLanguageID = 1,
                    ContentItemCommonDataFirstPublishedWhen = DateTime.UtcNow.AddDays(-5),
                    ContentItemCommonDataLastPublishedWhen = DateTime.UtcNow
                },
                BaseWebPage = new BaseWebPage
                {
                    WebPageItemGUID = Guid.NewGuid(),
                    WebPageItemID = 500,
                    WebPageItemName = "Finance Audit Service Page",
                    WebPageItemWebsiteChannelID = 2,
                    WebPageUrlPath = "/industries/finance/audit",
                    WebPageItemOrder = 1,
                    WebPageItemTreePath = "/industries/finance/audit",
                    WebPageUrlPathContentLanguageID = 1,
                    WebPageItemParentID = 20
                }
            };

            _industryServicePageServiceMock
                .Setup(s => s.GetIndustryServiceDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.Multiple(() =>
            {
                Assert.That(viewResult, Is.Not.Null);
                var model = viewResult?.Model as IndustryServiceDto;
                Assert.That(model, Is.Not.Null);
                Assert.That(model?.Title, Is.EqualTo(expectedDto.Title));
                Assert.That(model?.Description, Is.EqualTo(expectedDto.Description));
                Assert.That(model?.Hero?.Title, Is.EqualTo(expectedDto.Hero.Title));
                Assert.That(model?.BaseContent?.ContentItemName, Is.EqualTo(expectedDto.BaseContent.ContentItemName));
                Assert.That(model?.BaseWebPage?.WebPageUrlPath, Is.EqualTo(expectedDto.BaseWebPage.WebPageUrlPath));
            });

            _industryServicePageServiceMock.Verify(
                s => s.GetIndustryServiceDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task Index_WhenServiceReturnsEmptyDto_ReturnsViewWithEmptyModel()
        {
            _industryServicePageServiceMock
                .Setup(s => s.GetIndustryServiceDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IndustryServiceDto());

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult?.Model as IndustryServiceDto, Is.Not.Null);

            _industryServicePageServiceMock.Verify(
                s => s.GetIndustryServiceDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task Index_PassesCorrectCancellationToken_ToService()
        {
            var token = new CancellationTokenSource().Token;

            _industryServicePageServiceMock
                .Setup(s => s.GetIndustryServiceDtoAsync(token))
                .ReturnsAsync(new IndustryServiceDto());

            await _controller.Index(token);

            _industryServicePageServiceMock.Verify(
                s => s.GetIndustryServiceDtoAsync(token),
                Times.Once
            );
        }

        [Test]
        public void Index_WhenServiceThrows_ExceptionIsPropagated()
        {
            _industryServicePageServiceMock
                .Setup(s => s.GetIndustryServiceDtoAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Service error"));

            Assert.ThrowsAsync<Exception>(async () =>
                await _controller.Index(CancellationToken.None));
        }

        [Test]
        public async Task Index_AlwaysReturnsViewResult()
        {
            _industryServicePageServiceMock
                .Setup(s => s.GetIndustryServiceDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IndustryServiceDto());

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.TypeOf<ViewResult>());
        }

        [Test]
        public async Task Index_ReturnsCorrectViewName()
        {
            _industryServicePageServiceMock
                .Setup(s => s.GetIndustryServiceDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IndustryServiceDto());

            var result = await _controller.Index(CancellationToken.None) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ViewName, Is.EqualTo("~/Views/IndustryService/Index.cshtml"));
        }

        [Test]
        public async Task Index_ServiceIsCalledExactlyOnce()
        {
            _industryServicePageServiceMock
                .Setup(s => s.GetIndustryServiceDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IndustryServiceDto());

            await _controller.Index(CancellationToken.None);

            _industryServicePageServiceMock.Verify(
                s => s.GetIndustryServiceDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task Index_ModelContainsHeroDto()
        {
            var expectedDto = new IndustryServiceDto
            {
                Hero = new HeroDto
                {
                    Layout = "default",
                    Title = "Finance Audit Hero",
                    Description = "Hero Description",
                    CtaButtonText = "Learn More",
                    CtaButtonUrl = "/industries/finance/audit"
                }
            };

            _industryServicePageServiceMock
                .Setup(s => s.GetIndustryServiceDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None) as ViewResult;
            var model = result?.Model as IndustryServiceDto;

            Assert.Multiple(() =>
            {
                Assert.That(model?.Hero, Is.Not.Null);
                Assert.That(model?.Hero?.Title, Is.EqualTo("Finance Audit Hero"));
                Assert.That(model?.Hero?.CtaButtonText, Is.EqualTo("Learn More"));
                Assert.That(model?.Hero?.CtaButtonUrl, Is.EqualTo("/industries/finance/audit"));
            });
        }

        [Test]
        public async Task Index_ModelContainsTitleAndDescription()
        {
            var expectedDto = new IndustryServiceDto
            {
                Title = "Finance Audit",
                Description = "Detailed audit service description"
            };

            _industryServicePageServiceMock
                .Setup(s => s.GetIndustryServiceDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None) as ViewResult;
            var model = result?.Model as IndustryServiceDto;

            Assert.Multiple(() =>
            {
                Assert.That(model?.Title, Is.EqualTo("Finance Audit"));
                Assert.That(model?.Description, Is.EqualTo("Detailed audit service description"));
            });
        }

        [Test]
        public async Task Index_ModelRetainsSameReferenceAsServiceReturn()
        {
            var expectedDto = new IndustryServiceDto { Title = "Finance Audit" };

            _industryServicePageServiceMock
                .Setup(s => s.GetIndustryServiceDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None) as ViewResult;

            Assert.That(result?.Model, Is.SameAs(expectedDto));
        }
    }
}
