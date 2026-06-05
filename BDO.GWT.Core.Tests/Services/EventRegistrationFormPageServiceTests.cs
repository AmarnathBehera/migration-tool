using BDO;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
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

        private void SetupContent(IEnumerable<EventRegistrationForm>? pages)
        {
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<EventRegistrationForm>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pages!);
        }

        [Test]
        public async Task GetEventRegistrationFormPageDtoAsync_ReturnsEmpty_WhenNoPage()
        {
            SetupContent(new List<EventRegistrationForm>());

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
        public async Task GetEventRegistrationFormPageDtoAsync_ReturnsEmpty_WhenResultsAreNull()
        {
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<EventRegistrationForm>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<EventRegistrationForm>?)null!);

            var result = await _service.GetEventRegistrationFormPageDtoAsync(null!, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Hero, Is.Null);
            });
        }

        [Test]
        public async Task GetEventRegistrationFormPageDtoAsync_MapsScalarFields()
        {
            var page = new EventRegistrationForm
            {
                PageTitle = "Reg Title",
                PageDescription = "Reg Desc",
                FormType = "Custom",
                Form = "form-id",
                EmbedCode = "<embed/>",
            };
            SetupContent(new List<EventRegistrationForm> { page });

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
            var page = new EventRegistrationForm
            {
                SystemFields = new WebPageFields
                {
                    ContentItemID = 17,
                    ContentItemGUID = guid,
                    ContentItemName = "Reg",
                    WebPageItemID = 7,
                    WebPageUrlPath = "/reg"
                }
            };
            SetupContent(new List<EventRegistrationForm> { page });

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
            var page = new EventRegistrationForm
            {
                HeroLayout = "Default",
                HeroTitle = "Hero",
                HeroDescription = "Desc",
                HeroCallToActionButtonType = "Primary",
                HeroCallToActionButtonText = "Go",
                HeroBackgroundImageAltText = "Alt",
                HeroBackgroundImage_MobileAltText = "MAlt",
            };
            SetupContent(new List<EventRegistrationForm> { page });

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

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<EventRegistrationForm>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    token))
                .ReturnsAsync(new List<EventRegistrationForm>());

            await _service.GetEventRegistrationFormPageDtoAsync(null!, token);

            _contentServiceMock.Verify(x => x.GetPagesAsync<EventRegistrationForm>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                token), Times.Once);
        }

        [Test]
        public void GetEventRegistrationFormPageDtoAsync_PropagatesException_FromContentService()
        {
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<EventRegistrationForm>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB fail"));

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.GetEventRegistrationFormPageDtoAsync(null!, CancellationToken.None));
        }
    }
}
