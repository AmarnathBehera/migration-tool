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
    public class SectionIndustriesControllerTests
    {
        private Mock<ISectionIndustriesService> _sectionIndustriesServiceMock = null!;
        private SectionIndustriesController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _sectionIndustriesServiceMock = new Mock<ISectionIndustriesService>();
            _controller = new SectionIndustriesController(_sectionIndustriesServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithSectionIndustriesDto()
        {
            var expectedDto = new SectionIndustriesPageDto
            {
                MultiLinksCardsGrid = new MultiLinkCardsGridDto
                {
                    Title = "Industries Grid",
                    Description = "Grid Description",
                    IsMultiLinks = true
                },
                BaseContent = new BaseContent
                {
                    ContentItemID = 1,
                    ContentItemGUID = Guid.NewGuid(),
                    ContentItemName = "Section Industries",
                    ContentItemContentTypeID = 201,
                    ContentItemIsSecured = false,
                    ContentItemCommonDataContentLanguageID = 1,
                    ContentItemCommonDataFirstPublishedWhen = DateTime.UtcNow.AddDays(-5),
                    ContentItemCommonDataLastPublishedWhen = DateTime.UtcNow
                },
                BaseWebPage = new BaseWebPage
                {
                    WebPageItemGUID = Guid.NewGuid(),
                    WebPageItemID = 200,
                    WebPageItemName = "Section Industries Page",
                    WebPageItemWebsiteChannelID = 2,
                    WebPageUrlPath = "/industries",
                    WebPageItemOrder = 1,
                    WebPageItemTreePath = "/industries",
                    WebPageUrlPathContentLanguageID = 1,
                    WebPageItemParentID = 0
                }
            };

            _sectionIndustriesServiceMock
                .Setup(s => s.GetSectionIndustriesDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.Multiple(() =>
            {
                Assert.That(viewResult, Is.Not.Null);
                var model = viewResult?.Model as SectionIndustriesPageDto;
                Assert.That(model, Is.Not.Null);
                Assert.That(model?.MultiLinksCardsGrid?.Title, Is.EqualTo(expectedDto.MultiLinksCardsGrid.Title));
                Assert.That(model?.MultiLinksCardsGrid?.Description, Is.EqualTo(expectedDto.MultiLinksCardsGrid.Description));
                Assert.That(model?.BaseContent?.ContentItemName, Is.EqualTo(expectedDto.BaseContent.ContentItemName));
                Assert.That(model?.BaseWebPage?.WebPageItemName, Is.EqualTo(expectedDto.BaseWebPage.WebPageItemName));
            });

            _sectionIndustriesServiceMock.Verify(
                s => s.GetSectionIndustriesDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task Index_WhenServiceReturnsEmptyDto_ReturnsViewWithEmptyModel()
        {
            var emptyDto = new SectionIndustriesPageDto();

            _sectionIndustriesServiceMock
                .Setup(s => s.GetSectionIndustriesDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyDto);

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            var model = viewResult?.Model as SectionIndustriesPageDto;
            Assert.That(model, Is.Not.Null);

            _sectionIndustriesServiceMock.Verify(
                s => s.GetSectionIndustriesDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task Index_PassesCorrectCancellationToken_ToService()
        {
            var token = new CancellationTokenSource().Token;

            _sectionIndustriesServiceMock
                .Setup(s => s.GetSectionIndustriesDtoAsync(token))
                .ReturnsAsync(new SectionIndustriesPageDto());

            await _controller.Index(token);

            _sectionIndustriesServiceMock.Verify(
                s => s.GetSectionIndustriesDtoAsync(token),
                Times.Once
            );
        }

        [Test]
        public void Index_WhenServiceThrows_ExceptionIsPropagated()
        {
            _sectionIndustriesServiceMock
                .Setup(s => s.GetSectionIndustriesDtoAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Service error"));

            Assert.ThrowsAsync<Exception>(async () =>
                await _controller.Index(CancellationToken.None));
        }

        [Test]
        public async Task Index_AlwaysReturnsViewResult()
        {
            _sectionIndustriesServiceMock
                .Setup(s => s.GetSectionIndustriesDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SectionIndustriesPageDto());

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.TypeOf<ViewResult>());
        }

        [Test]
        public async Task Index_ReturnsCorrectViewName()
        {
            _sectionIndustriesServiceMock
                .Setup(s => s.GetSectionIndustriesDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SectionIndustriesPageDto());

            var result = await _controller.Index(CancellationToken.None) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("~/Views/SectionIndustries/Index.cshtml"));
        }

        [Test]
        public async Task Index_ServiceIsCalledExactlyOnce()
        {
            _sectionIndustriesServiceMock
                .Setup(s => s.GetSectionIndustriesDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SectionIndustriesPageDto());

            await _controller.Index(CancellationToken.None);

            _sectionIndustriesServiceMock.Verify(
                s => s.GetSectionIndustriesDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task Index_ModelContainsHeroDto()
        {
            var expectedDto = new SectionIndustriesPageDto
            {
                Hero = new HeroDto
                {
                    Layout = "default",
                    Title = "Industries Hero",
                    Description = "Hero Description",
                    CtaButtonText = "Explore"
                }
            };

            _sectionIndustriesServiceMock
                .Setup(s => s.GetSectionIndustriesDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None) as ViewResult;
            var model = result?.Model as SectionIndustriesPageDto;

            Assert.Multiple(() =>
            {
                Assert.That(model?.Hero, Is.Not.Null);
                Assert.That(model?.Hero?.Title, Is.EqualTo("Industries Hero"));
                Assert.That(model?.Hero?.Description, Is.EqualTo("Hero Description"));
                Assert.That(model?.Hero?.CtaButtonText, Is.EqualTo("Explore"));
            });
        }

        [Test]
        public async Task Index_ModelContainsMultiLinksCardsGrid_WithCards()
        {
            var expectedDto = new SectionIndustriesPageDto
            {
                MultiLinksCardsGrid = new MultiLinkCardsGridDto
                {
                    Title = "Industries",
                    Cards = new[]
                    {
                        new MultiLinkCardDto { Title = "Finance" },
                        new MultiLinkCardDto { Title = "Technology" }
                    },
                    IsMultiLinks = true
                }
            };

            _sectionIndustriesServiceMock
                .Setup(s => s.GetSectionIndustriesDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None) as ViewResult;
            var model = result?.Model as SectionIndustriesPageDto;

            Assert.Multiple(() =>
            {
                Assert.That(model?.MultiLinksCardsGrid?.Cards, Is.Not.Null);
                Assert.That(model?.MultiLinksCardsGrid?.Cards, Has.Length.EqualTo(2));
                Assert.That(model?.MultiLinksCardsGrid?.Cards![0].Title, Is.EqualTo("Finance"));
                Assert.That(model?.MultiLinksCardsGrid?.IsMultiLinks, Is.True);
            });
        }

        [Test]
        public async Task Index_ModelMultiLinksCardsGrid_WhenIsMultiLinksIsFalse()
        {
            var expectedDto = new SectionIndustriesPageDto
            {
                MultiLinksCardsGrid = new MultiLinkCardsGridDto { IsMultiLinks = false }
            };

            _sectionIndustriesServiceMock
                .Setup(s => s.GetSectionIndustriesDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None) as ViewResult;
            var model = result?.Model as SectionIndustriesPageDto;

            Assert.That(model?.MultiLinksCardsGrid?.IsMultiLinks, Is.False);
        }

        [Test]
        public async Task Index_ModelRetainsSameReferenceAsServiceReturn()
        {
            var expectedDto = new SectionIndustriesPageDto
            {
                MultiLinksCardsGrid = new MultiLinkCardsGridDto { Title = "Industries" }
            };

            _sectionIndustriesServiceMock
                .Setup(s => s.GetSectionIndustriesDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None) as ViewResult;

            Assert.That(result?.Model, Is.SameAs(expectedDto));
        }
    }
}
