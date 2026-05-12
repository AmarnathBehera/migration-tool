using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using BDO.GWT.Core;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class SectionServicesControllerTests
    {
        private Mock<ISectionServicesService> _sectionServicesServiceMock = null!;
        private SectionServicesController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _sectionServicesServiceMock = new Mock<ISectionServicesService>();
            _controller = new SectionServicesController(_sectionServicesServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithCorrectViewPath()
        {
            _sectionServicesServiceMock
                .Setup(x => x.GetSectionServicesDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SectionServicesPageDto());

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.ViewName, Is.EqualTo("~/Views/SectionServices/Index.cshtml"));
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithMappedModel()
        {
            var expectedDto = new SectionServicesPageDto
            {
                Hero = new Web.Core.DTOs.HeroDto { Title = "Services Hero" },
                MetaData = new MetaDataDto { MetaTitle = "Meta Title" },
                BaseContent = new BaseContent
                {
                    ContentItemID = 1,
                    ContentItemName = "SectionServices"
                },
                BaseWebPage = new BaseWebPage
                {
                    WebPageItemID = 10,
                    WebPageUrlPath = "/services"
                }
            };

            _sectionServicesServiceMock
                .Setup(x => x.GetSectionServicesDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);

            var model = viewResult!.Model as SectionServicesPageDto;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Hero.Title, Is.EqualTo("Services Hero"));
            Assert.That(model.MetaData?.MetaTitle, Is.EqualTo("Meta Title"));
            Assert.That(model.BaseContent?.ContentItemID, Is.EqualTo(1));
            Assert.That(model.BaseWebPage?.WebPageUrlPath, Is.EqualTo("/services"));
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithNullOptionalFields()
        {
            var expectedDto = new SectionServicesPageDto
            {
                Hero = new HeroDto(),
                MetaData = null,
                SearchAndSEO = null,
                OGMetaData = null,
                BaseContent = null,
                BaseWebPage = null
            };

            _sectionServicesServiceMock
                .Setup(x => x.GetSectionServicesDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);

            var model = viewResult!.Model as SectionServicesPageDto;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.MetaData, Is.Null);
            Assert.That(model.SearchAndSEO, Is.Null);
            Assert.That(model.OGMetaData, Is.Null);
            Assert.That(model.BaseContent, Is.Null);
            Assert.That(model.BaseWebPage, Is.Null);
        }

        [Test]
        public async Task Index_CallsServiceExactlyOnce()
        {
            _sectionServicesServiceMock
                .Setup(x => x.GetSectionServicesDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SectionServicesPageDto());

            await _controller.Index(CancellationToken.None);

            _sectionServicesServiceMock.Verify(
                x => x.GetSectionServicesDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Index_PassesCancellationToken_ToService()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            _sectionServicesServiceMock
                .Setup(x => x.GetSectionServicesDtoAsync(token))
                .ReturnsAsync(new SectionServicesPageDto());

            await _controller.Index(token);

            _sectionServicesServiceMock.Verify(
                x => x.GetSectionServicesDtoAsync(token),
                Times.Once);
        }
    }
}