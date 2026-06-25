using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Kentico.Xperience.Headless.Internal;
using Moq;

namespace BDO.GWT.Core.Tests.Services
{
    public class EventRegistrationFormPageServiceTests
    {
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<IWebsiteChannelContext> _channelMock = null!;
        private EventRegistrationFormPageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _contentServiceMock = new Mock<IContentService>();
            _heroServiceMock = new Mock<IHeroService>();
            _channelMock = new Mock<IWebsiteChannelContext>();
            _channelMock.Setup(x => x.WebsiteChannelID).Returns(1);

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((HeroData data, CancellationToken _) => new HeroDto
                {
                    Layout = data.Layout,
                    Title = data.Title,
                    Description = data.Description,
                });

            _service = new EventRegistrationFormPageService(
                _contentServiceMock.Object,
                _heroServiceMock.Object,
                _channelMock.Object);
        }

        private void SetupContent(EventRegistrationForm? page)
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<EventRegistrationForm>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);
        }

        // Helper: mock a page with scalar fields via SetupGet
        private Mock<EventRegistrationForm> CreatePageMock(
            string? title = null,
            string? description = null,
            string? formType = null,
            string? form = null,
            string? embedCode = null)
        {
            var mock = new Mock<EventRegistrationForm>();
            mock.SetupGet(p => p.PageTitle).Returns(title!);
            mock.SetupGet(p => p.PageDescription).Returns(description!);
            mock.SetupGet(p => p.FormType).Returns(formType!);
            mock.SetupGet(p => p.Form).Returns(form!);
            mock.SetupGet(p => p.EmbedCode).Returns(embedCode!);
            mock.SetupGet(p => p.HeroLayout).Returns((string?)null!);
            mock.SetupGet(p => p.HeroTitle).Returns((string?)null!);
            mock.SetupGet(p => p.HeroDescription).Returns((string?)null!);
            mock.SetupGet(p => p.HeroBackgroundImage).Returns((IEnumerable<ContentAttachment>?)null!);
            mock.SetupGet(p => p.HeroBackgroundImageAltText).Returns((string?)null!);
            mock.SetupGet(p => p.HeroBackgroundImage_Mobile).Returns((IEnumerable<ContentAttachment>?)null!);
            mock.SetupGet(p => p.HeroBackgroundImage_MobileAltText).Returns((string?)null!);
            mock.SetupGet(p => p.HeroCallToActionButtonType).Returns((string?)null!);
            mock.SetupGet(p => p.HeroCallToActionButtonText).Returns((string?)null!);
           // mock.SetupGet(p => p.HeroCallToActionButtonUrl).Returns((IEnumerable<WebPage>?)null!);
            mock.SetupGet(p => p.SystemFields).Returns((WebPageFields?)null!);
            return mock;
        }

        [Test]
        public async Task GetEventRegistrationFormPageDtoAsync_ReturnsEmptyDto_WhenPageIsNull()
        {
            SetupContent(null);

            var result = await _service.GetEventRegistrationFormPageDtoAsync(null!, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.Null);
                Assert.That(result.Hero, Is.Null);
                Assert.That(result.BaseContent, Is.Null);
            });
        }

        [Test]
        public async Task GetEventRegistrationFormPageDtoAsync_MapsScalarFields()
        {
            var pageMock = CreatePageMock(
                title: "Reg Title",
                description: "Reg Desc",
                formType: "Custom",
                form: "form-id",
                embedCode: "<embed/>");
            SetupContent(pageMock.Object);

            var result = await _service.GetEventRegistrationFormPageDtoAsync(null!, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.EqualTo("Reg Title"));
                Assert.That(result.Description, Is.EqualTo("Reg Desc"));
                Assert.That(result.FormType, Is.EqualTo("Custom"));
                Assert.That(result.Form, Is.EqualTo("form-id"));
                Assert.That(result.EmbedCode, Is.EqualTo("<embed/>"));
            });
        }

        [Test]
        public async Task GetEventRegistrationFormPageDtoAsync_MapsBaseContent_FromSystemFields()
        {
            var guid = Guid.NewGuid();

            var sysMock = new Mock<WebPageFields>();
            sysMock.SetupGet(s => s.ContentItemID).Returns(17);
            sysMock.SetupGet(s => s.ContentItemGUID).Returns(guid);
            sysMock.SetupGet(s => s.ContentItemName).Returns("Reg");
            sysMock.SetupGet(s => s.ContentItemContentTypeID).Returns(0);
            sysMock.SetupGet(s => s.ContentItemIsSecured).Returns(false);
            sysMock.SetupGet(s => s.ContentItemCommonDataContentLanguageID).Returns(0);
            sysMock.SetupGet(s => s.ContentItemCommonDataFirstPublishedWhen).Returns(default(DateTime));
            sysMock.SetupGet(s => s.ContentItemCommonDataLastPublishedWhen).Returns(default(DateTime));
            sysMock.SetupGet(s => s.WebPageItemGUID).Returns(Guid.Empty);
            sysMock.SetupGet(s => s.WebPageItemID).Returns(7);
            sysMock.SetupGet(s => s.WebPageItemName).Returns((string?)null!);
            sysMock.SetupGet(s => s.WebPageItemWebsiteChannelId).Returns(0);
            sysMock.SetupGet(s => s.WebPageUrlPath).Returns("/reg");
            sysMock.SetupGet(s => s.WebPageItemOrder).Returns(0);
            sysMock.SetupGet(s => s.WebPageItemTreePath).Returns((string?)null!);
            sysMock.SetupGet(s => s.WebPageUrlPathContentLanguageID).Returns(0);
            sysMock.SetupGet(s => s.WebPageItemParentID).Returns(0);

            var pageMock = CreatePageMock();
            pageMock.SetupGet(p => p.SystemFields).Returns(sysMock.Object);
            SetupContent(pageMock.Object);

            var result = await _service.GetEventRegistrationFormPageDtoAsync(null!, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.BaseContent!.ContentItemID, Is.EqualTo(17));
                Assert.That(result.BaseContent.ContentItemGUID, Is.EqualTo(guid));
                Assert.That(result.BaseContent.ContentItemName, Is.EqualTo("Reg"));
                Assert.That(result.BaseWebPage!.WebPageItemID, Is.EqualTo(7));
                Assert.That(result.BaseWebPage.WebPageUrlPath, Is.EqualTo("/reg"));
            });
        }

        [Test]
        public async Task GetEventRegistrationFormPageDtoAsync_PassesHeroData_ToHeroService()
        {
            var pageMock = new Mock<EventRegistrationForm>();
            pageMock.SetupGet(p => p.HeroLayout).Returns("Default");
            pageMock.SetupGet(p => p.HeroTitle).Returns("Hero");
            pageMock.SetupGet(p => p.HeroDescription).Returns("Desc");
            pageMock.SetupGet(p => p.HeroCallToActionButtonType).Returns("Primary");
            pageMock.SetupGet(p => p.HeroCallToActionButtonText).Returns("Go");
            pageMock.SetupGet(p => p.HeroBackgroundImageAltText).Returns("Alt");
            pageMock.SetupGet(p => p.HeroBackgroundImage_MobileAltText).Returns("MAlt");
            pageMock.SetupGet(p => p.HeroBackgroundImage).Returns((IEnumerable<ContentAttachment>?)null!);
            pageMock.SetupGet(p => p.HeroBackgroundImage_Mobile).Returns((IEnumerable<ContentAttachment>?)null!);
           // pageMock.SetupGet(p => p.HeroCallToActionButtonUrl).Returns((IEnumerable<WebPage>?)null!);
            pageMock.SetupGet(p => p.PageTitle).Returns((string?)null!);
            pageMock.SetupGet(p => p.PageDescription).Returns((string?)null!);
            pageMock.SetupGet(p => p.FormType).Returns((string?)null!);
            pageMock.SetupGet(p => p.Form).Returns((string?)null!);
            pageMock.SetupGet(p => p.EmbedCode).Returns((string?)null!);
            pageMock.SetupGet(p => p.SystemFields).Returns((WebPageFields?)null!);
            SetupContent(pageMock.Object);

            HeroData? captured = null;
            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((d, _) => captured = d)
                .ReturnsAsync(new HeroDto());

            await _service.GetEventRegistrationFormPageDtoAsync(null!, CancellationToken.None);

            Assert.That(captured, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(captured!.Layout, Is.EqualTo("Default"));
                Assert.That(captured.Title, Is.EqualTo("Hero"));
                Assert.That(captured.Description, Is.EqualTo("Desc"));
                Assert.That(captured.CallToActionButtonType, Is.EqualTo("Primary"));
                Assert.That(captured.CallToActionButtonText, Is.EqualTo("Go"));
                Assert.That(captured.BackgroundImageAltText, Is.EqualTo("Alt"));
                Assert.That(captured.MobileBackgroundImageAltText, Is.EqualTo("MAlt"));
            });
        }

        [Test]
        public async Task GetEventRegistrationFormPageDtoAsync_PassesCancellationToken()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            var pageMock = CreatePageMock();
            SetupContent(pageMock.Object);

            await _service.GetEventRegistrationFormPageDtoAsync(null!, token);

            _contentServiceMock.Verify(x => x.RetrieveCurrentPageAsync<EventRegistrationForm>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                token), Times.Once);
        }

        [Test]
        public void GetEventRegistrationFormPageDtoAsync_PropagatesException_FromContentService()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<EventRegistrationForm>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB fail"));

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.GetEventRegistrationFormPageDtoAsync(null!, CancellationToken.None));
        }
    }
}