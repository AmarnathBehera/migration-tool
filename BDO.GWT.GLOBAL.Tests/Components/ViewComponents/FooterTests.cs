using BDO.GWT.Core.DTOs.Footer;
using BDO.GWT.Core.Services.Footer;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.ViewComponents
{
    public class FooterTests
    {
        private Mock<IFooterService> _footerServiceMock = null!;
        private BDO.GWT.Global.Components.ViewComponents.Footer _component = null!;

        [SetUp]
        public void Setup()
        {
            _footerServiceMock = new Mock<IFooterService>();
            _component = new BDO.GWT.Global.Components.ViewComponents.Footer(_footerServiceMock.Object);
        }

        [Test]
        public async Task InvokeAsync_ReturnsViewResult_WithFooterDto()
        {
            var expectedDto = new FooterDto();

            _footerServiceMock
                .Setup(s => s.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _component.InvokeAsync(CancellationToken.None);

            var viewResult = result as ViewViewComponentResult;

            Assert.Multiple(() =>
            {
                Assert.That(viewResult, Is.Not.Null);
                Assert.That(viewResult!.ViewData!.Model, Is.EqualTo(expectedDto));
            });

            _footerServiceMock.Verify(
                s => s.GetAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task InvokeAsync_WhenServiceReturnsNull_ReturnsViewWithNullModel()
        {
            _footerServiceMock
                .Setup(s => s.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((FooterDto?)null);

            var result = await _component.InvokeAsync(CancellationToken.None);

            var viewResult = result as ViewViewComponentResult;

            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.ViewData!.Model, Is.Null);

            _footerServiceMock.Verify(
                s => s.GetAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task InvokeAsync_PassesCorrectCancellationToken()
        {
            var token = new CancellationTokenSource().Token;

            _footerServiceMock
                .Setup(s => s.GetAsync(token))
                .ReturnsAsync(new FooterDto());

            await _component.InvokeAsync(token);

            _footerServiceMock.Verify(
                s => s.GetAsync(token),
                Times.Once
            );
        }

        [Test]
        public void InvokeAsync_WhenServiceThrows_ExceptionIsPropagated()
        {
            _footerServiceMock
                .Setup(s => s.GetAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            Assert.ThrowsAsync<Exception>(async () =>
                await _component.InvokeAsync(CancellationToken.None));
        }

        [Test]
        public async Task InvokeAsync_AlwaysReturnsViewViewComponentResult()
        {
            _footerServiceMock
                .Setup(s => s.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FooterDto());

            var result = await _component.InvokeAsync(CancellationToken.None);

            Assert.That(result, Is.TypeOf<ViewViewComponentResult>());
        }
    }
}