using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Moq;

namespace BDO.GWT.Core.Tests.Services
{
    public class SectionEventsServiceTests
    {
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<IPeopleCardsService> _peopleCardsServiceMock = null!;
        private Mock<ICultureContext> _cultureContextMock = null!;
        private SectionEventsService _service = null!;

        [SetUp]
        public void Setup()
        {
            _contentServiceMock = new Mock<IContentService>();
            _heroServiceMock = new Mock<IHeroService>();
            _peopleCardsServiceMock = new Mock<IPeopleCardsService>();
            _cultureContextMock = new Mock<ICultureContext>();

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((HeroData data, CancellationToken _) => new HeroDto
                {
                    Layout = data.Layout,
                    Title = data.Title,
                    Description = data.Description,
                });

            _service = new SectionEventsService(
                _contentServiceMock.Object,
                _heroServiceMock.Object,
                _peopleCardsServiceMock.Object,
                _cultureContextMock.Object);
        }

        [Test]
        public async Task GetSectionEventsDtoAsync_MapsAllScalarFields()
        {
            var page = new SectionEvents
            {
                EventsWebinarsListTitle = "Events Title",
                EventsWebinarsListDescription = "Events Desc",
                EventsWebinarsListAdvancedSearchDescription = "Search prelink",
                EventsWebinarsListAdvancedSearchLinkText = "Search link",
                HeroLayout = "Default",
                HeroTitle = "Hero",
                HeroDescription = "HeroDesc",
                HeroCallToActionButtonType = "Primary",
                HeroCallToActionButtonText = "Go",
                HeroBackgroundImageAltText = "BgAlt",
                HeroBackgroundImage_MobileAltText = "MobileAlt",
            };

            var result = await _service.GetSectionEventsDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Title, Is.EqualTo("Events Title"));
                Assert.That(result.Description, Is.EqualTo("Events Desc"));
                Assert.That(result.AdvancedSearchPrelinkText, Is.EqualTo("Search prelink"));
                Assert.That(result.AdvancedSearchLinkText, Is.EqualTo("Search link"));
                Assert.That(result.FilteringTooltipEnabled, Is.True);
                Assert.That(result.PageSize, Is.EqualTo(20));
            });
        }

        [Test]
        public async Task GetSectionEventsDtoAsync_PassesHeroData_ToHeroService()
        {
            var page = new SectionEvents
            {
                HeroLayout = "FullWidth",
                HeroTitle = "Welcome",
                HeroDescription = "Desc",
                HeroCallToActionButtonType = "Primary",
                HeroCallToActionButtonText = "Click",
                HeroBackgroundImageAltText = "Alt",
                HeroBackgroundImage_MobileAltText = "MobileAlt",
            };

            HeroData? captured = null;
            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((d, _) => captured = d)
                .ReturnsAsync(new HeroDto());

            await _service.GetSectionEventsDtoAsync(page, CancellationToken.None);

            Assert.That(captured, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(captured!.Layout, Is.EqualTo("FullWidth"));
                Assert.That(captured.Title, Is.EqualTo("Welcome"));
                Assert.That(captured.Description, Is.EqualTo("Desc"));
                Assert.That(captured.CallToActionButtonType, Is.EqualTo("Primary"));
                Assert.That(captured.CallToActionButtonText, Is.EqualTo("Click"));
                Assert.That(captured.BackgroundImageAltText, Is.EqualTo("Alt"));
                Assert.That(captured.MobileBackgroundImageAltText, Is.EqualTo("MobileAlt"));
            });
        }

        [Test]
        public async Task GetSectionEventsDtoAsync_ReturnsHeroDto_FromHeroService()
        {
            var page = new SectionEvents
            {
                HeroTitle = "Hero",
                HeroDescription = "Desc",
                HeroLayout = "L",
            };

            var result = await _service.GetSectionEventsDtoAsync(page, CancellationToken.None);

            Assert.That(result.Hero, Is.Not.Null);
            Assert.That(result.Hero.Title, Is.EqualTo("Hero"));
        }

        [Test]
        public async Task GetSectionEventsDtoAsync_PassesCancellationToken()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            var page = new SectionEvents();

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), token))
                .ReturnsAsync(new HeroDto());

            await _service.GetSectionEventsDtoAsync(page, token);

            _heroServiceMock.Verify(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), token), Times.Once);
        }

        [Test]
        public void GetSectionEventsDtoAsync_PropagatesException_FromHeroService()
        {
            var page = new SectionEvents();
            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("hero fail"));

            Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetSectionEventsDtoAsync(page, CancellationToken.None));
        }

        [Test]
        public async Task GetSectionEventsDtoAsync_BackgroundImages_NullWhenAttachmentsEmpty()
        {
            var page = new SectionEvents
            {
                HeroBackgroundImage = null!,
                HeroBackgroundImage_Mobile = null!,
            };

            HeroData? captured = null;
            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((d, _) => captured = d)
                .ReturnsAsync(new HeroDto());

            await _service.GetSectionEventsDtoAsync(page, CancellationToken.None);

            Assert.That(captured, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(captured!.BackgroundImage, Is.Null);
                Assert.That(captured.MobileBackgroundImage, Is.Null);
            });
        }
    }
}
