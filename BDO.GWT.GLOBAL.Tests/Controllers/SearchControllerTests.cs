using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;

using GWTXbyKWeb.Controllers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class SearchControllerTests
    {
        private Mock<ISearchPageService> _searchPageServiceMock = null!;
        private Mock<ICultureContext> _cultureContextMock = null!;
        private SearchController _controller = null!;
        private const string LANGUAGE_TAG = "en-US";

        [SetUp]
        public void Setup()
        {
            _searchPageServiceMock = new Mock<ISearchPageService>();
            _searchPageServiceMock
                .Setup(x => x.GetSearchPageDtoAsync(It.IsAny<string>(), LANGUAGE_TAG, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SearchPageDto());

            _cultureContextMock = new Mock<ICultureContext>();
            _cultureContextMock.SetupGet(x => x.CurrentLanguageTag).Returns(LANGUAGE_TAG);

            _controller = new SearchController(_searchPageServiceMock.Object, _cultureContextMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext(),
                    RouteData = new RouteData()
                }
            };
        }


        [TearDown]
        public void TearDown() => _controller.Dispose();


        [Test]
        public async Task Index_ReturnsViewResult_WithDtoFromService()
        {
            var expected = new SearchPageDto { InitialSearchText = "audit" };
            _searchPageServiceMock
                .Setup(x => x.GetSearchPageDtoAsync("audit", LANGUAGE_TAG, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.Index("audit", CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.Multiple(() =>
            {
                Assert.That(viewResult, Is.Not.Null);
                Assert.That(viewResult!.Model, Is.SameAs(expected));
            });
        }


        [Test]
        public async Task Index_WithNullSearchText_PassesEmptyStringToService()
        {
            await _controller.Index(null, CancellationToken.None);

            _searchPageServiceMock.Verify(x => x.GetSearchPageDtoAsync(
                string.Empty, LANGUAGE_TAG, It.IsAny<CancellationToken>()), Times.Once);
        }


        [Test]
        public async Task Index_ForwardsCancellationTokenToService()
        {
            using var cts = new CancellationTokenSource();

            await _controller.Index("audit", cts.Token);

            _searchPageServiceMock.Verify(x => x.GetSearchPageDtoAsync("audit", LANGUAGE_TAG, cts.Token), Times.Once);
        }

        [TestCase("")]
        [TestCase("   ")]
        public async Task Index_WithEmptyOrWhitespaceSearchText_PassesValueUnchanged(string searchText)
        {
            await _controller.Index(searchText, CancellationToken.None);

            _searchPageServiceMock.Verify(x => x.GetSearchPageDtoAsync(
                searchText, LANGUAGE_TAG, It.IsAny<CancellationToken>()), Times.Once);
        }


        [Test]
        public void Index_WhenServiceThrows_PropagatesException()
        {
            _searchPageServiceMock
                .Setup(x => x.GetSearchPageDtoAsync(It.IsAny<string>(), LANGUAGE_TAG, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Search unavailable"));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _controller.Index("audit", CancellationToken.None));
        }


        [Test]
        public async Task Index_DoesNotSetExplicitViewName()
        {
            var result = await _controller.Index("audit", CancellationToken.None);

            Assert.That(((ViewResult)result).ViewName, Is.Null,
                "Convention-based view resolution is expected (~/Views/Search/Index.cshtml).");
        }
    }
}