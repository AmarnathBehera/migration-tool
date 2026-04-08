using BDO.GWT.Core.DTOs.Cookieboot;
using BDO.GWT.Web.Core.LiveSite.Components.ViewComponents;
using CMS.DataEngine;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.ViewComponents
{
    public class CookiebotTests
    {
        private Mock<IInfoProvider<CookiebotInfo>> _providerMock;
        private Cookiebot _component;

        [SetUp]
        public void Setup()
        {
            _providerMock = new Mock<IInfoProvider<CookiebotInfo>>();
            _component = new Cookiebot(_providerMock.Object);
        }
        [Test]
        public async Task InvokeAsync_ReturnsDefaultDto_WhenNoData()
        {
            var mockQuery = new Mock<ObjectQuery<CookiebotInfo>>();

            mockQuery.Setup(x => x.TopN(It.IsAny<int>()))
                     .Returns(mockQuery.Object);

            mockQuery.As<IEnumerable<CookiebotInfo>>()
                     .Setup(x => x.GetEnumerator())
                     .Returns(new List<CookiebotInfo>().GetEnumerator());

            _providerMock.Setup(x => x.Get())
                         .Returns(mockQuery.Object);

            var result = await _component.InvokeAsync(CancellationToken.None) as ViewViewComponentResult;

            Assert.That(result, Is.Not.Null);

            var model = result?.ViewData?.Model as CookiebotDTO;

            Assert.That(model, Is.Not.Null);
            Assert.That(model.EnableCookiebot, Is.False);
            Assert.That(model.CookieID, Is.Null);
            Assert.That(model.CustomCookiebotScript, Is.Null);
        }
    }
}