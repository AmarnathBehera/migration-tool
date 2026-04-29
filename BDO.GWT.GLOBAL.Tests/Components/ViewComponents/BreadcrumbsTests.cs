using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ViewComponents;
using BDO.GWT.Web.Core.LiveSite.Components.ViewComponents;
using BDO.GWT.Web.Core.LiveSite.Services;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Components.ViewComponents
{
    public class BreadcrumbsTests
    {
        private Mock<IBreadcrumbsService> _breadcrumbsServiceMock = null!;
        private Breadcrumbs _component = null!;

        [SetUp]
        public void SetUp()
        {
            _breadcrumbsServiceMock = new Mock<IBreadcrumbsService>();
            _component = new Breadcrumbs(_breadcrumbsServiceMock.Object);
        }

        [Test]
        public async Task InvokeAsync_ReturnsViewResult_WithBreadcrumbsDto()
        {
            var expectedDto = new BreadcrumbsDto
            {
                Links = new List<LinkItemDto>
                {
                    new LinkItemDto { Title = "Home", URL = "/", Target = "_self" },
                    new LinkItemDto { Title = "About", URL = "/about", Target = "_self" }
                }
            };

            _breadcrumbsServiceMock
                .Setup(s => s.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _component.InvokeAsync(CancellationToken.None);

            var viewResult = result as ViewViewComponentResult;

            Assert.Multiple(() =>
            {
                Assert.That(viewResult, Is.Not.Null);
                Assert.That(viewResult?.ViewData?.Model, Is.EqualTo(expectedDto));
            });

            _breadcrumbsServiceMock.Verify(
                s => s.GetAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task InvokeAsync_WhenServiceReturnsEmptyLinks_ReturnsViewWithEmptyLinks()
        {
            var expectedDto = new BreadcrumbsDto { Links = new List<LinkItemDto>() };

            _breadcrumbsServiceMock
                .Setup(s => s.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _component.InvokeAsync(CancellationToken.None);

            var viewResult = result as ViewViewComponentResult;
            var model = viewResult?.ViewData?.Model as BreadcrumbsDto;

            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Links, Is.Empty);
        }

        [Test]
        public async Task InvokeAsync_PassesCorrectCancellationToken()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            _breadcrumbsServiceMock
                .Setup(s => s.GetAsync(token))
                .ReturnsAsync(new BreadcrumbsDto());

            await _component.InvokeAsync(token);

            _breadcrumbsServiceMock.Verify(
                s => s.GetAsync(token),
                Times.Once
            );
        }
    }
}
