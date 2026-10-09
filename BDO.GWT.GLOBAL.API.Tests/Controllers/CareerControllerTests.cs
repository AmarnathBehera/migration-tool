using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.GLOBAL.API.Controllers;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers
{
    [TestFixture]
    public class CareerControllerTests
    {
        private const int ChannelId = 1;
        private const int MemberFirmId = 100;

        private Mock<ICareerCrudService> _careerCrudServiceMock;
        private CareerController _controller;

        [SetUp]
        public void Setup()
        {
            _careerCrudServiceMock = new Mock<ICareerCrudService>();
            _controller = new CareerController(_careerCrudServiceMock.Object);

            // The controller reads HttpContext.Items["ChannelId"]; provide it here.
            var httpContext = new DefaultHttpContext();
            httpContext.Items["ChannelId"] = ChannelId;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        private static PageOutput<CareerPageContent> CreatePageOutput(Guid? pageGuid = null) =>
            new()
            {
                General = new PageGeneralOutput
                {
                    PageGuid = pageGuid ?? Guid.NewGuid()
                },
                Content = new CareerPageContent()
            };

        #region Get (single)

        [Test]
        public async Task Get_Single_ReturnsOkWithPage()
        {
            var pageGuid = Guid.NewGuid();
            var expected = CreatePageOutput(pageGuid);
            _careerCrudServiceMock
                .Setup(x => x.GetCareerPageAsync(ChannelId, pageGuid, "en-US", It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.Get(MemberFirmId, pageGuid, "en-US", CancellationToken.None);

            var okResult = result.Result as OkObjectResult;
            Assert.Multiple(() =>
            {
                Assert.That(okResult, Is.Not.Null);
                Assert.That(okResult!.Value, Is.EqualTo(expected));
            });
          
        }

        [Test]
        public async Task Get_Single_PassesChannelIdFromHttpContext()
        {
            var pageGuid = Guid.NewGuid();
            _careerCrudServiceMock
                .Setup(x => x.GetCareerPageAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreatePageOutput());

            await _controller.Get(MemberFirmId, pageGuid, "en-US", CancellationToken.None);

            _careerCrudServiceMock.Verify(
                x => x.GetCareerPageAsync(ChannelId, pageGuid, "en-US", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public void Get_Single_ServiceThrowsNotFound_PropagatesException()
        {
            _careerCrudServiceMock
                .Setup(x => x.GetCareerPageAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException("not found"));

            Assert.ThrowsAsync<NotFoundException>(() =>
                _controller.Get(MemberFirmId, Guid.NewGuid(), "en-US", CancellationToken.None));
        }

        #endregion

        #region Get (list)

        [Test]
        public async Task Get_List_ReturnsOkWithPages()
        {
            var expected = new List<PageOutput<CareerPageContent>> { CreatePageOutput(), CreatePageOutput() };
            _careerCrudServiceMock
                .Setup(x => x.GetCareerPagesAsync(ChannelId, "en-US", true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.Get(MemberFirmId, "en-US", true, CancellationToken.None);

            var okResult = result.Result as OkObjectResult;
            
            Assert.Multiple(() =>
            {
                Assert.That(okResult, Is.Not.Null);
                Assert.That(okResult!.Value, Is.EqualTo(expected));
            });
        }

        [Test]
        public async Task Get_List_NullCultureAndOnlyPublished_PassesNullsToService()
        {
            _careerCrudServiceMock
                .Setup(x => x.GetCareerPagesAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PageOutput<CareerPageContent>>());

            await _controller.Get(MemberFirmId, null, null, CancellationToken.None);

            _careerCrudServiceMock.Verify(
                x => x.GetCareerPagesAsync(ChannelId, null, null, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region Post

        [Test]
        public async Task Post_ValidRequest_ReturnsCreatedAtActionWithPageValue()
        {
            var expected = CreatePageOutput();
            _careerCrudServiceMock
                .Setup(x => x.CreateCareerPageAsync(ChannelId, 100, "en-US", It.IsAny<CareerPageContent>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.Post(MemberFirmId, 100, "en-US", new CareerPageContent(), CancellationToken.None);

            var createdResult = result.Result as CreatedAtActionResult;


            Assert.Multiple(() =>
            {
                Assert.That(createdResult, Is.Not.Null);
                Assert.That(createdResult!.ActionName, Is.EqualTo(nameof(CareerController.Get)));
                Assert.That(createdResult.Value, Is.EqualTo(expected));
                Assert.That(createdResult.RouteValues!["PageGuid"], Is.EqualTo(expected.General!.PageGuid));
                Assert.That(createdResult.RouteValues["memberFirmID"], Is.EqualTo(MemberFirmId));
            });
        }

        [Test]
        public async Task Post_ServiceReturnsBadRequest_ResultValueIsNull()
        {
            // When the service returns a non-value ActionResult (e.g. BadRequestResult),
            // pageDto.Value is null and CreatedAtAction is created with a null value.
            _careerCrudServiceMock
                .Setup(x => x.CreateCareerPageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CareerPageContent>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BadRequestResult());

            var result = await _controller.Post(MemberFirmId, 100, "en-US", new CareerPageContent(), CancellationToken.None);

            var createdResult = result.Result as CreatedAtActionResult;
            Assert.Multiple(() =>
            {
                Assert.That(createdResult, Is.Not.Null);
                Assert.That(createdResult!.Value, Is.Null);
            });
        }

        #endregion

        #region CreateNewCultureVersion

        [Test]
        public async Task CreateNewCultureVersion_Valid_ReturnsCreatedAtAction()
        {
            var pageGuid = Guid.NewGuid();
            var expected = CreatePageOutput();
            _careerCrudServiceMock
                .Setup(x => x.CreateCareerPageCultureVersionAsync(ChannelId, pageGuid, "en-US", "fr-FR", It.IsAny<CareerPageContent>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.CreateNewCultureVersion(MemberFirmId, pageGuid, "en-US", "fr-FR", new CareerPageContent(), CancellationToken.None);

            var createdResult = result.Result as CreatedAtActionResult;
            Assert.That(createdResult, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(createdResult!.ActionName, Is.EqualTo(nameof(CareerController.Get)));
                Assert.That(createdResult.Value, Is.EqualTo(expected));
            });
        }

        #endregion

        #region Put

        [Test]
        public async Task Put_Valid_ReturnsOkWithUpdatedPage()
        {
            var pageGuid = Guid.NewGuid();
            var expected = CreatePageOutput();
            _careerCrudServiceMock
                .Setup(x => x.UpdateCareerPageAsync(ChannelId, pageGuid, "en-US", It.IsAny<CareerPageContent>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.Put(MemberFirmId, pageGuid, "en-US", new CareerPageContent(), CancellationToken.None);

            var okResult = result.Result as OkObjectResult;
            Assert.Multiple(() =>
            {
                Assert.That(okResult, Is.Not.Null);
                Assert.That(okResult!.Value, Is.EqualTo(expected));
            });
        }

        [Test]
        public async Task Put_ServiceReturnsNonValueResult_ReturnsOkWithNull()
        {
            _careerCrudServiceMock
                .Setup(x => x.UpdateCareerPageAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CareerPageContent>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await _controller.Put(MemberFirmId, Guid.NewGuid(), "en-US", new CareerPageContent(), CancellationToken.None);

            var okResult = result.Result as OkObjectResult;
            Assert.Multiple(() =>
            {
                Assert.That(okResult, Is.Not.Null);
                Assert.That(okResult!.Value, Is.Null);
            });
        }

        #endregion

        #region Delete

        [Test]
        public async Task Delete_Success_ReturnsOk()
        {
            _careerCrudServiceMock
                .Setup(x => x.DeleteCareerPageAsync(ChannelId, It.IsAny<Guid>(), "en-US", false, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.Delete(MemberFirmId, Guid.NewGuid(), "en-US");

            var okResult = result as OkObjectResult;
            Assert.Multiple(() =>
            {
                Assert.That(okResult, Is.Not.Null);
                Assert.That(okResult!.Value, Is.EqualTo("Successfully deleted."));
            });
        }

        [Test]
        public async Task Delete_DeleteAllCultures_PassesFlagToService()
        {
            var pageGuid = Guid.NewGuid();
            _careerCrudServiceMock
                .Setup(x => x.DeleteCareerPageAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _controller.Delete(MemberFirmId, pageGuid, "en-US", deleteAllCultures: true);

            _careerCrudServiceMock.Verify(
                x => x.DeleteCareerPageAsync(ChannelId, pageGuid, "en-US", true, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Delete_NotFoundException_ReturnsNotFound()
        {
            _careerCrudServiceMock
                .Setup(x => x.DeleteCareerPageAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException("page not found"));

            var result = await _controller.Delete(MemberFirmId, Guid.NewGuid(), "en-US");

            var notFound = result as NotFoundObjectResult;
            Assert.Multiple(() =>
            {
                Assert.That(notFound, Is.Not.Null);
                Assert.That(notFound!.Value, Is.EqualTo("page not found"));
            });
        }

        [Test]
        public async Task Delete_UnprocessableEntityException_ReturnsUnprocessableEntity()
        {
            _careerCrudServiceMock
                .Setup(x => x.DeleteCareerPageAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new UnprocessableEntityException("invalid"));

            var result = await _controller.Delete(MemberFirmId, Guid.NewGuid(), "en-US");

            var unprocessable = result as UnprocessableEntityObjectResult;
            Assert.Multiple(() =>
            {
                Assert.That(unprocessable, Is.Not.Null);
                Assert.That(unprocessable!.Value, Is.EqualTo("One or more validation errors occurred."));
            });
        }

        [Test]
        public async Task Delete_UnexpectedException_Returns500()
        {
            _careerCrudServiceMock
                .Setup(x => x.DeleteCareerPageAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var result = await _controller.Delete(MemberFirmId, Guid.NewGuid(), "en-US");

            var objectResult = result as ObjectResult;
            Assert.Multiple(() =>
            {
                Assert.That(objectResult, Is.Not.Null);
                Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
                Assert.That(objectResult.Value, Is.EqualTo("boom"));
            });
        }

        #endregion
    }
}