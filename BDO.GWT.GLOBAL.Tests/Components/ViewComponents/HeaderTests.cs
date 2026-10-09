using BDO.GWT.Core;
using BDO.GWT.Core.DTOs.Header;
using BDO.GWT.Core.Services.Header;
using BDO.GWT.Global.Components.ViewComponents;
using CMS.Core;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.ViewComponents
{
    public class HeaderTests
    {
        private Mock<IHeaderService> _headerServiceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IEventLogService> _eventLogServiceMock = null!;
        private Header _component = null!;

        [SetUp]
        public void Setup()
        {
            _headerServiceMock = new Mock<IHeaderService>();
            _contentServiceMock = new Mock<IContentService>();
            _eventLogServiceMock = new Mock<IEventLogService>();
            _component = new Header(_headerServiceMock.Object, _contentServiceMock.Object, _eventLogServiceMock.Object);
        }

        [Test]
        public async Task InvokeAsync_ReturnsViewResult_WithHeaderDto()
        {
            var expectedDto = new HeaderDto();

            _headerServiceMock
                .Setup(s => s.GetAsync(_contentServiceMock.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _component.InvokeAsync(CancellationToken.None);

            var viewResult = result as ViewViewComponentResult;

            Assert.Multiple(() =>
            {
                Assert.That(viewResult, Is.Not.Null);
                Assert.That(viewResult?.ViewData?.Model, Is.EqualTo(expectedDto));
            });

            _headerServiceMock.Verify(
                s => s.GetAsync(_contentServiceMock.Object, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task InvokeAsync_WhenServiceReturnsNull_ReturnsViewWithNullModel()
        {
            _headerServiceMock
                .Setup(s => s.GetAsync(_contentServiceMock.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync((HeaderDto?)null);

            var result = await _component.InvokeAsync(CancellationToken.None);

            var viewResult = result as ViewViewComponentResult;

            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult?.ViewData?.Model, Is.Null);

            _headerServiceMock.Verify(
                s => s.GetAsync(_contentServiceMock.Object, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }


        [Test]
        public async Task InvokeAsync_PassesCorrectCancellationToken()
        {
            var token = new CancellationTokenSource().Token;

            _headerServiceMock
                .Setup(s => s.GetAsync(_contentServiceMock.Object, token))
                .ReturnsAsync(new HeaderDto());

            await _component.InvokeAsync(token);

            _headerServiceMock.Verify(
                s => s.GetAsync(_contentServiceMock.Object, token),
                Times.Once
            );
        }

        [Test]
        public async Task InvokeAsync_PassesContentServiceCorrectly()
        {
            _headerServiceMock
                .Setup(s => s.GetAsync(_contentServiceMock.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HeaderDto());

            await _component.InvokeAsync(CancellationToken.None);

            _headerServiceMock.Verify(
                s => s.GetAsync(_contentServiceMock.Object, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public void InvokeAsync_WhenServiceThrows_ExceptionIsPropagated()
        {
            _headerServiceMock
                .Setup(s => s.GetAsync(_contentServiceMock.Object, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new System.Exception());

            Assert.ThrowsAsync<System.Exception>(async () =>
                await _component.InvokeAsync(CancellationToken.None));
        }

        [Test]
        public async Task InvokeAsync_AlwaysReturnsViewViewComponentResult()
        {
            _headerServiceMock
                .Setup(s => s.GetAsync(_contentServiceMock.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HeaderDto());

            var result = await _component.InvokeAsync(CancellationToken.None);

            Assert.That(result, Is.TypeOf<ViewViewComponentResult>());
        }
    }
}