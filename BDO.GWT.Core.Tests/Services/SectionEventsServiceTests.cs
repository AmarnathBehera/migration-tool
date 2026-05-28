using BDO;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services
{
    public class SectionEventsServiceTests
    {
        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<IEventWebinarCardsService> _eventWebinarCardsServiceMock = null!;
        private Mock<IFilteringHelper> _filteringHelperMock = null!;
        private Mock<IContentRetriever> _contentRetrieverMock = null!;
        private SectionEventsService _service = null!;

        [SetUp]
        public void Setup()
        {
            _heroServiceMock = new Mock<IHeroService>();
            _eventWebinarCardsServiceMock = new Mock<IEventWebinarCardsService>();
            _filteringHelperMock = new Mock<IFilteringHelper>();
            _contentRetrieverMock = new Mock<IContentRetriever>();

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((HeroData data, CancellationToken _) => new HeroDto
                {
                    Layout = data.Layout,
                    Title = data.Title,
                    Description = data.Description,
                });

            _filteringHelperMock
                .Setup(x => x.IsFilteringTooltipEnabledAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _eventWebinarCardsServiceMock
                .Setup(x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

            _service = new SectionEventsService(
                _heroServiceMock.Object,
                _eventWebinarCardsServiceMock.Object,
                _filteringHelperMock.Object,
                _contentRetrieverMock.Object);
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
