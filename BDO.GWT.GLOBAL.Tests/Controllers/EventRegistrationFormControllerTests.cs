using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Web.Core.LiveSite.Controllers;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class EventRegistrationFormControllerTests
    {
        private Mock<IEventRegistrationFormPageService> _serviceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private EventRegistrationFormController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<IEventRegistrationFormPageService>();
            _contentServiceMock = new Mock<IContentService>();
            _controller = new EventRegistrationFormController(
                _serviceMock.Object,
                _contentServiceMock.Object);
        }

        [TearDown]
        public void TearDown() => _controller.Dispose();

        [Test]
        public async Task Index_ReturnsViewResult_WithCorrectViewName()
        {
            _serviceMock
                .Setup(s => s.GetEventRegistrationFormPageDtoAsync(null!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FormPageDto());

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ViewResult>());
            var view = (ViewResult)result;
            Assert.That(view.ViewName, Is.EqualTo("~/Views/FormPage/Index.cshtml"));
        }

        [Test]
        public async Task Index_ReturnsServiceDtoAsModel()
        {
            var dto = new FormPageDto { Title = "Register" };
            _serviceMock
                .Setup(s => s.GetEventRegistrationFormPageDtoAsync(null!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var view = (ViewResult)await _controller.Index(CancellationToken.None);

            Assert.That(view.Model, Is.SameAs(dto));
        }

        [Test]
        public async Task Index_PassesCancellationToken_ToService()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            _serviceMock
                .Setup(s => s.GetEventRegistrationFormPageDtoAsync(null!, token))
                .ReturnsAsync(new FormPageDto());

            await _controller.Index(token);

            _serviceMock.Verify(s => s.GetEventRegistrationFormPageDtoAsync(null!, token), Times.Once);
        }

        [Test]
        public void Index_PropagatesException_FromService()
        {
            _serviceMock
                .Setup(s => s.GetEventRegistrationFormPageDtoAsync(null!, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Index(CancellationToken.None));
        }
    }
}
