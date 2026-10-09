using BDO;
using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services
{
    public class SectionEventsServiceTests
    {
        private static readonly string[] ExpectedParsedFilters = ["eventType", "service", "industryPage"];

        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<IEventWebinarCardsService> _eventWebinarCardsServiceMock = null!;
        private Mock<IFilteringHelper> _filteringHelperMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<ICultureContext> _cultureContextMock = null!;
        private Mock<IContentRetriever> _contentRetrieverMock = null!;
        private SectionEventsService _service = null!;

        [SetUp]
        public void Setup()
        {
            _heroServiceMock = new Mock<IHeroService>();
            _eventWebinarCardsServiceMock = new Mock<IEventWebinarCardsService>();
            _filteringHelperMock = new Mock<IFilteringHelper>();
            _contentServiceMock = new Mock<IContentService>();
            _cultureContextMock = new Mock<ICultureContext>();
            _contentRetrieverMock = new Mock<IContentRetriever>();

            _cultureContextMock.Setup(c => c.CurrentLanguageTag).Returns("en");

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
                .Setup(x => x.CreateFiltersAsync(
                    It.IsAny<HashSet<string>>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

            _service = new SectionEventsService(
                _heroServiceMock.Object,
                _eventWebinarCardsServiceMock.Object,
                _filteringHelperMock.Object,
                _contentServiceMock.Object,
                _cultureContextMock.Object,
                _contentRetrieverMock.Object);
        }

        [Test]
        public async Task GetEventsSectionDtoAsync_MapsAllScalarFields()
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

            var result = await _service.GetEventsSectionDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.CardsGrid, Is.Not.Null);
                Assert.That(result.CardsGrid.Title, Is.EqualTo("Events Title"));
                Assert.That(result.CardsGrid.Description, Is.EqualTo("Events Desc"));
                Assert.That(result.CardsGrid.AdvancedSearchPrelinkText, Is.EqualTo("Search prelink"));
                Assert.That(result.CardsGrid.AdvancedSearchLinkText, Is.EqualTo("Search link"));
                Assert.That(result.CardsGrid.PageSize, Is.EqualTo(20));
                Assert.That(result.CardsGrid.FilteringTooltipEnabled, Is.True);
            });
        }

        [Test]
        public async Task GetEventsSectionDtoAsync_PassesHeroData_ToHeroService()
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

            await _service.GetEventsSectionDtoAsync(page, CancellationToken.None);

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
        public async Task GetEventsSectionDtoAsync_ReturnsHeroDto_FromHeroService()
        {
            var page = new SectionEvents
            {
                HeroTitle = "Hero",
                HeroDescription = "Desc",
                HeroLayout = "L",
            };

            var result = await _service.GetEventsSectionDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Hero, Is.Not.Null);
                Assert.That(result.Hero.Title, Is.EqualTo("Hero"));
            });
        }

        [Test]
        public async Task GetEventsSectionDtoAsync_PassesCancellationToken()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            var page = new SectionEvents();

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), token))
                .ReturnsAsync(new HeroDto());

            await _service.GetEventsSectionDtoAsync(page, token);

            _heroServiceMock.Verify(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), token), Times.Once);
        }

        [Test]
        public void GetEventsSectionDtoAsync_PropagatesException_FromHeroService()
        {
            var page = new SectionEvents();
            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("hero fail"));

            Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetEventsSectionDtoAsync(page, CancellationToken.None));
        }

        [Test]
        public async Task GetEventsSectionDtoAsync_BackgroundImages_NullWhenAttachmentsEmpty()
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

            await _service.GetEventsSectionDtoAsync(page, CancellationToken.None);

            Assert.That(captured, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(captured!.BackgroundImage, Is.Null);
                Assert.That(captured.MobileBackgroundImage, Is.Null);
            });
        }

        [Test]
        public async Task GetEventsSectionDtoAsync_FilteringDisabled_DoesNotCreateFilters()
        {
            var page = new SectionEvents
            {
                EventsWebinarsListFilteringEnabled = false,
                EventsWebinarsListFilters = "eventType;service",
            };

            var result = await _service.GetEventsSectionDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.CardsGrid.Filters, Is.Null);
                _eventWebinarCardsServiceMock.Verify(
                    x => x.CreateFiltersAsync(
                        It.IsAny<HashSet<string>>(),
                        It.IsAny<string?>(),
                        It.IsAny<CancellationToken>()),
                    Times.Never);
            });
        }

        [Test]
        public async Task GetEventsSectionDtoAsync_FilteringEnabled_ParsesFilterString()
        {
            var page = new SectionEvents
            {
                EventsWebinarsListFilteringEnabled = true,
                EventsWebinarsListFilters = "eventType; service ;industryPage",
            };

            HashSet<string>? captured = null;
            _eventWebinarCardsServiceMock
                .Setup(x => x.CreateFiltersAsync(
                    It.IsAny<HashSet<string>>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()))
                .Callback<HashSet<string>, string?, CancellationToken>((f, _, _) => captured = f)
                .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

            await _service.GetEventsSectionDtoAsync(page, CancellationToken.None);

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured, Is.EquivalentTo(ExpectedParsedFilters));
        }

        [Test]
        public async Task GetEventsSectionDtoAsync_FilteringEnabled_WithEmptyFilterString_PassesEmptySet()
        {
            var page = new SectionEvents
            {
                EventsWebinarsListFilteringEnabled = true,
                EventsWebinarsListFilters = string.Empty,
            };

            HashSet<string>? captured = null;
            _eventWebinarCardsServiceMock
                .Setup(x => x.CreateFiltersAsync(
                    It.IsAny<HashSet<string>>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()))
                .Callback<HashSet<string>, string?, CancellationToken>((f, _, _) => captured = f)
                .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

            await _service.GetEventsSectionDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(captured, Is.Not.Null);
                Assert.That(captured, Is.Empty);
            });
        }

        [Test]
        public async Task GetEventsSectionDtoAsync_FilteringEnabled_WithWhitespaceFilters_PassesTrimmedTokens()
        {
            var page = new SectionEvents
            {
                EventsWebinarsListFilteringEnabled = true,
                EventsWebinarsListFilters = "   ;  ;   ",
            };

            HashSet<string>? captured = null;
            _eventWebinarCardsServiceMock
                .Setup(x => x.CreateFiltersAsync(
                    It.IsAny<HashSet<string>>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()))
                .Callback<HashSet<string>, string?, CancellationToken>((f, _, _) => captured = f)
                .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

            await _service.GetEventsSectionDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(captured, Is.Not.Null);
                Assert.That(captured, Is.EquivalentTo(new[] { string.Empty }));
            });
        }

        [Test]
        public async Task GetEventsSectionDtoAsync_AdvancedSearchLink_DefaultsToEmpty_WhenNoSelectedPage()
        {
            var page = new SectionEvents();

            var result = await _service.GetEventsSectionDtoAsync(page, CancellationToken.None);

            Assert.That(result.CardsGrid.AdvancedSearchLink, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetFilteredItemsAsync_Throws_WhenNoSectionEventsPageFound()
        {
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<SectionEvents>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SectionEvents>());

            Assert.ThrowsAsync<MissingSectionPageException>(() =>
                _service.GetFilteredItemsAsync(
                    new CardsQuery { Filters = new Dictionary<string, string>() },
                    "en-gb",
                    CancellationToken.None));
        }

        [Test]
        public async Task GetFilteredItemsAsync_DelegatesToCardsService_AndPropagatesShowPastEventsFlag()
        {
            var sectionPage = new SectionEvents
            {
                ShowPastEvents = true,
                EventsWebinarsListFeaturedPages = new List<IWebPageFieldsSource>(),
            };

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<SectionEvents>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SectionEvents> { sectionPage });

            var expected = new PaginationDto<EventsWebinarsCardDto>
            {
                Data = Array.Empty<EventsWebinarsCardDto>(),
                TotalCount = 0,
            };

            // SectionEventsService.GetFilteredItemsAsync passes language from _cultureContext internally
            _eventWebinarCardsServiceMock
                .Setup(x => x.GetFilteredItemsAsync(
                    It.IsAny<CardsQuery>(),
                    It.IsAny<string>(),       // language — passed internally from cultureContext
                    It.IsAny<Action<ContentItemQueryBuilder>?>(),
                    It.IsAny<Func<IEventOrWebinar, bool>>(),
                    true,
                    false,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var query = new CardsQuery { Filters = new Dictionary<string, string>() };
            var result = await _service.GetFilteredItemsAsync(query, "en-gb", CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.SameAs(expected));
                Assert.That(query.Filters[FilterParameterName.ShowAllPastEvents], Is.EqualTo(bool.TrueString));
            });
        }

        [Test]
        public async Task GetFilteredItemsAsync_WhenFiltersIsNull_InitializesAndSetsShowAllPastEvents()
        {
            var sectionPage = new SectionEvents
            {
                ShowPastEvents = false,
                EventsWebinarsListFeaturedPages = new List<IWebPageFieldsSource>(),
            };

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<SectionEvents>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SectionEvents> { sectionPage });

            _eventWebinarCardsServiceMock
                .Setup(x => x.GetFilteredItemsAsync(
                    It.IsAny<CardsQuery>(),
                    It.IsAny<string>(),
                    It.IsAny<Action<ContentItemQueryBuilder>?>(),
                    It.IsAny<Func<IEventOrWebinar, bool>>(),
                    false,
                    false,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaginationDto<EventsWebinarsCardDto>
                {
                    Data = Array.Empty<EventsWebinarsCardDto>(),
                    TotalCount = 0,
                });

            var query = new CardsQuery { Filters = null! };

            await _service.GetFilteredItemsAsync(query, "en-gb", CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(query.Filters, Is.Not.Null);
                Assert.That(query.Filters[FilterParameterName.ShowAllPastEvents], Is.EqualTo(bool.FalseString));
            });
        }

        [Test]
        public async Task GetFilteredItemsAsync_WhenShowAllPastEventsAlreadyExists_ReplacesValue()
        {
            var sectionPage = new SectionEvents
            {
                ShowPastEvents = true,
                EventsWebinarsListFeaturedPages = new List<IWebPageFieldsSource>(),
            };

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<SectionEvents>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SectionEvents> { sectionPage });

            _eventWebinarCardsServiceMock
                .Setup(x => x.GetFilteredItemsAsync(
                    It.IsAny<CardsQuery>(),
                    It.IsAny<string>(),
                    It.IsAny<Action<ContentItemQueryBuilder>?>(),
                    It.IsAny<Func<IEventOrWebinar, bool>>(),
                    true,
                    false,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaginationDto<EventsWebinarsCardDto>
                {
                    Data = Array.Empty<EventsWebinarsCardDto>(),
                    TotalCount = 0,
                });

            var query = new CardsQuery
            {
                Filters = new Dictionary<string, string>
                {
                    [FilterParameterName.ShowAllPastEvents] = bool.FalseString,
                },
            };

            await _service.GetFilteredItemsAsync(query, "en-gb", CancellationToken.None);

            Assert.That(query.Filters[FilterParameterName.ShowAllPastEvents], Is.EqualTo(bool.TrueString));
        }

        [Test]
        public async Task GetFilteredItemsAsync_PassesFeaturedPredicate_BasedOnFeaturedPageIds()
        {
            var featuredSource = new Mock<IWebPageFieldsSource>();
            featuredSource.SetupGet(x => x.SystemFields).Returns(new WebPageFields { WebPageItemID = 101 });

            var sectionPage = new SectionEvents
            {
                ShowPastEvents = true,
                EventsWebinarsListFeaturedPages = new List<IWebPageFieldsSource> { featuredSource.Object },
            };

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<SectionEvents>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { sectionPage });

            Func<IEventOrWebinar, bool>? capturedPredicate = null;
            _eventWebinarCardsServiceMock
                .Setup(x => x.GetFilteredItemsAsync(
                    It.IsAny<CardsQuery>(),
                    It.IsAny<string>(),
                    It.IsAny<Action<ContentItemQueryBuilder>?>(),
                    It.IsAny<Func<IEventOrWebinar, bool>>(),
                    true,
                    false,
                    It.IsAny<CancellationToken>()))
                .Callback<CardsQuery, string, Action<ContentItemQueryBuilder>?, Func<IEventOrWebinar, bool>, bool, bool, CancellationToken>(
                    (_, _, _, p, _, _, _) => capturedPredicate = p)
                .ReturnsAsync(new PaginationDto<EventsWebinarsCardDto>
                {
                    Data = Array.Empty<EventsWebinarsCardDto>(),
                    TotalCount = 0,
                });

            var query = new CardsQuery { Filters = new Dictionary<string, string>() };
            await _service.GetFilteredItemsAsync(query, "en-gb", CancellationToken.None);

            var featured = new Mock<IEventOrWebinar>();
            featured.SetupGet(x => x.SystemFields).Returns(new WebPageFields { WebPageItemID = 101 });

            var regular = new Mock<IEventOrWebinar>();
            regular.SetupGet(x => x.SystemFields).Returns(new WebPageFields { WebPageItemID = 999 });

            Assert.That(capturedPredicate, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(capturedPredicate!(featured.Object), Is.True);
                Assert.That(capturedPredicate(regular.Object), Is.False);
            });
        }

        [Test]
        public async Task GetEventsSectionDtoAsync_UsesFilteringTooltipValue_FromHelper()
        {
            var page = new SectionEvents
            {
                EventsWebinarsListFilteringEnabled = true,
                EventsWebinarsListFilters = "eventType",
            };

            _filteringHelperMock
                .Setup(x => x.IsFilteringTooltipEnabledAsync(true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _service.GetEventsSectionDtoAsync(page, CancellationToken.None);

            Assert.That(result.CardsGrid.FilteringTooltipEnabled, Is.False);
        }

        [Test]
        public async Task GetFilteredItemsAsync_PassesCancellationToken_ToCardsService()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var sectionPage = new SectionEvents
            {
                ShowPastEvents = true,
                EventsWebinarsListFeaturedPages = new List<IWebPageFieldsSource>(),
            };

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<SectionEvents>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    token))
                .ReturnsAsync(new[] { sectionPage });

            _eventWebinarCardsServiceMock
                .Setup(x => x.GetFilteredItemsAsync(
                    It.IsAny<CardsQuery>(),
                    It.IsAny<string>(),
                    It.IsAny<Action<ContentItemQueryBuilder>?>(),
                    It.IsAny<Func<IEventOrWebinar, bool>>(),
                    true,
                    false,
                    token))
                .ReturnsAsync(new PaginationDto<EventsWebinarsCardDto>
                {
                    Data = Array.Empty<EventsWebinarsCardDto>(),
                    TotalCount = 0,
                });

            var query = new CardsQuery { Filters = new Dictionary<string, string>() };
            await _service.GetFilteredItemsAsync(query, "en-gb", token);

            _eventWebinarCardsServiceMock.Verify(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<string>(),
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<IEventOrWebinar, bool>>(),
                true,
                false,
                token), Times.Once);
        }
    }
}