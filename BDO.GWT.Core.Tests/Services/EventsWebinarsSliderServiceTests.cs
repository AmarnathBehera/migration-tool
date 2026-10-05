using BDO.GWT.Core;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Services
{
    public class EventsWebinarsSliderServiceTests
    {
        private Mock<IFilteringHelper> _filteringHelperMock = null!;
        private Mock<IEventWebinarCardsService> _eventWebinarCardsServiceMock = null!;
        private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
        private Mock<IContentQueryExecutor> _contentQueryExecutorMock = null!;
        private EventsWebinarsSliderService _service = null!;

        // The service no longer takes an IWebPageFieldsSource - only the
        // content item GUID and the page type name.
        private const string TestPageTypeName = "TestPage";

        private sealed class TestPage : IWebPageFieldsSource
        {
            public WebPageFields SystemFields { get; set; } = new WebPageFields();
        }

        [SetUp]
        public void SetUp()
        {
            _filteringHelperMock = new Mock<IFilteringHelper>();
            _eventWebinarCardsServiceMock = new Mock<IEventWebinarCardsService>();
            _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
            _contentQueryExecutorMock = new Mock<IContentQueryExecutor>();

            _service = new EventsWebinarsSliderService(
                _eventWebinarCardsServiceMock.Object,
                _filteringHelperMock.Object,
                _websiteChannelContextMock.Object,
                _contentQueryExecutorMock.Object);
        }

        private static EventsWebinarsSliderData CreateData()
        {
            return new EventsWebinarsSliderData
            {
                Title = "Events title",
                Description = "Events description",
                FilteringEnabled = true,
                Filters = new HashSet<string>
                {
                    FilterParameterName.EventType,
                    FilterParameterName.BusinessIssue
                },
                FeaturedEventAndWebinarNodeGuids = new List<ContentItemReference>(),
                BackgroundImages = "/media/background.jpg",
                BackgroundImageAltText = "Background alt",
                SeeAllLinkText = "See all",
                SeeAllLinkUrl = "/events",
                SeeAllLinkURLTarget = "_blank",
                TopMargin = "20px",
                BottomMargin = "40px",
                WidgetID = "events-slider"
            };
        }

        private static TestPage CreateCurrentPage()
        {
            return new TestPage();
        }

        private static PaginationDto<EventsWebinarsCardDto> CreatePaginationDto(
            IEnumerable<EventsWebinarsCardDto>? cards = null)
        {
            return new PaginationDto<EventsWebinarsCardDto>
            {
                Data = cards?.ToArray() ?? Array.Empty<EventsWebinarsCardDto>()
            };
        }

        [Test]
        public async Task GetEventsWebinarsSliderAsync_WithAllData_MapsDtoCorrectly()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var currentPageGuid = Guid.NewGuid();
            var data = CreateData();

            var cards = new[]
            {
                new EventsWebinarsCardDto()
            };

            _filteringHelperMock
                .Setup(x => x.IsFilteringTooltipEnabledAsync(true, cancellationToken))
                .ReturnsAsync(true);

            _eventWebinarCardsServiceMock
                .Setup(x => x.CreateFiltersAsync(
                    data.Filters,
                    It.IsAny<string?>(),
                    cancellationToken))
                .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

            _eventWebinarCardsServiceMock
                .Setup(x => x.GetFilteredItemsAsync(
                    It.IsAny<CardsQuery>(),
                    It.IsAny<string>(),
                    It.IsAny<Action<ContentItemQueryBuilder>?>(),
                    It.IsAny<Func<IEventOrWebinar, bool>>(),
                    false,
                    true,
                    cancellationToken))
                .ReturnsAsync(CreatePaginationDto(cards));

            // Act
            var result = await _service.GetEventsWebinarsSliderAsync(
                currentPageGuid,
                TestPageTypeName,
                data,
                cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.EqualTo("Events title"));
                Assert.That(result.Description, Is.EqualTo("Events description"));
                Assert.That(result.BackgroundImageURL, Is.EqualTo("/media/background.jpg"));
                Assert.That(result.BackgroundImageAltText, Is.EqualTo("Background alt"));
                Assert.That(result.FilteringTooltipEnabled, Is.True);
                Assert.That(result.Items, Is.EquivalentTo(cards));
                Assert.That(result.TopMargin, Is.EqualTo("20px"));
                Assert.That(result.BottomMargin, Is.EqualTo("40px"));
                Assert.That(result.WidgetID, Is.EqualTo("events-slider"));

                Assert.That(result.SeeAllLink, Is.Not.Null);
                Assert.That(result.SeeAllLink!.Title, Is.EqualTo("See all"));
                Assert.That(result.SeeAllLink.URL, Is.EqualTo("/events"));
                Assert.That(result.SeeAllLink.Target, Is.EqualTo("_blank"));
            });
        }

        [Test]
        public async Task GetEventsWebinarsSliderAsync_WhenFilteringDisabled_DoesNotCreateFilters()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var currentPageGuid = Guid.NewGuid();

            var data = CreateData();
            data.FilteringEnabled = false;

            _filteringHelperMock
                .Setup(x => x.IsFilteringTooltipEnabledAsync(false, cancellationToken))
                .ReturnsAsync(false);

            _eventWebinarCardsServiceMock
                .Setup(x => x.GetFilteredItemsAsync(
                    It.IsAny<CardsQuery>(),
                    It.IsAny<string>(),
                    It.IsAny<Action<ContentItemQueryBuilder>?>(),
                    It.IsAny<Func<IEventOrWebinar, bool>>(),
                    false,
                    true,
                    cancellationToken))
                .ReturnsAsync(CreatePaginationDto());

            // Act
            var result = await _service.GetEventsWebinarsSliderAsync(
            currentPageGuid,
            TestPageTypeName,
            data,
            cancellationToken);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Filters, Is.Null);
                Assert.That(result.FilteringTooltipEnabled, Is.False);
            });

            _eventWebinarCardsServiceMock.Verify(
                x => x.CreateFiltersAsync(
                    It.IsAny<HashSet<string>>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetEventsWebinarsSliderAsync_WhenSeeAllLinkTextIsNull_SetsSeeAllLinkToNull()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var currentPageGuid = Guid.NewGuid();

            var data = CreateData();
            data.SeeAllLinkText = null;

            _filteringHelperMock
                .Setup(x => x.IsFilteringTooltipEnabledAsync(It.IsAny<bool>(), cancellationToken))
                .ReturnsAsync(false);

            _eventWebinarCardsServiceMock
                .Setup(x => x.CreateFiltersAsync(
                    It.IsAny<HashSet<string>>(),
                    It.IsAny<string?>(),
                    cancellationToken))
                .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

            _eventWebinarCardsServiceMock
                .Setup(x => x.GetFilteredItemsAsync(
                    It.IsAny<CardsQuery>(),
                    It.IsAny<string>(),
                    It.IsAny<Action<ContentItemQueryBuilder>?>(),
                    It.IsAny<Func<IEventOrWebinar, bool>>(),
                    false,
                    true,
                    cancellationToken))
                .ReturnsAsync(CreatePaginationDto());

            // Act
            var result = await _service.GetEventsWebinarsSliderAsync(
               currentPageGuid,
               TestPageTypeName,
               data,
               cancellationToken);

            // Assert
            Assert.That(result.SeeAllLink, Is.Null);
        }

        [Test]
        public async Task GetAPIFilteredItemsAsync_WhenPageTypeIsNull_SetsDefaultPageSizeTo10()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            CardsQuery? capturedQuery = null;

            var query = new CardsQuery
            {
                CurrentPageGuid = Guid.NewGuid(),
                PageType = null
            };

            var cards = new[]
            {
                new EventsWebinarsCardDto()
            };

            _eventWebinarCardsServiceMock
                .Setup(x => x.GetFilteredItemsAsync(
                    It.IsAny<CardsQuery>(),
                    It.IsAny<string>(),
                    It.IsAny<Action<ContentItemQueryBuilder>?>(),
                    It.IsAny<Func<IEventOrWebinar, bool>>(),
                    false,
                    true,
                    cancellationToken))
                .Callback<CardsQuery, string, Action<ContentItemQueryBuilder>?, Func<IEventOrWebinar, bool>, bool, bool, CancellationToken>(
                    (q, _, _, _, _, _, _) => capturedQuery = q)
                .ReturnsAsync(CreatePaginationDto(cards));

            // Act
            var result = await _service.GetAPIFilteredItemsAsync(query, cancellationToken);

            // Assert
            Assert.That(result, Is.EquivalentTo(cards));

            Assert.Multiple(() =>
            {
                Assert.That(capturedQuery, Is.Not.Null);
                Assert.That(capturedQuery!.CurrentPage, Is.EqualTo(1));
                Assert.That(capturedQuery.PageSize, Is.EqualTo(10));
            });
        }

        [Test]
        public async Task GetAPIFilteredItemsAsync_WhenPageTypeHasFilter_SetsDefaultPageSizeTo20()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            CardsQuery? capturedQuery = null;
            Action<ContentItemQueryBuilder>? capturedDecorateQuery = null;

            var query = new CardsQuery
            {
                CurrentPageGuid = Guid.NewGuid(),
                PageType = FilterParameterName.BusinessLine
            };

            _eventWebinarCardsServiceMock
                .Setup(x => x.GetFilteredItemsAsync(
                    It.IsAny<CardsQuery>(),
                    It.IsAny<string>(),
                    It.IsAny<Action<ContentItemQueryBuilder>?>(),
                    It.IsAny<Func<IEventOrWebinar, bool>>(),
                    false,
                    true,
                    cancellationToken))
                .Callback<CardsQuery, string, Action<ContentItemQueryBuilder>?, Func<IEventOrWebinar, bool>, bool, bool, CancellationToken>(
                    (q, _, decorateQuery, _, _, _, _) =>
                    {
                        capturedQuery = q;
                        capturedDecorateQuery = decorateQuery;
                    })
                .ReturnsAsync(CreatePaginationDto());

            // Act
            await _service.GetAPIFilteredItemsAsync(query, cancellationToken);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(capturedQuery, Is.Not.Null);
                Assert.That(capturedQuery!.CurrentPage, Is.EqualTo(1));
                Assert.That(capturedQuery.PageSize, Is.EqualTo(20));
                Assert.That(capturedDecorateQuery, Is.Not.Null);
            });
        }

        [Test]
        public async Task GetAPIFilteredItemsAsync_WhenCurrentPageAndPageSizeAlreadySet_DoesNotOverride()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            CardsQuery? capturedQuery = null;

            var query = new CardsQuery
            {
                CurrentPageGuid = Guid.NewGuid(),
                PageType = null,
                CurrentPage = 3,
                PageSize = 6
            };

            _eventWebinarCardsServiceMock
                .Setup(x => x.GetFilteredItemsAsync(
                    It.IsAny<CardsQuery>(),
                    It.IsAny<string>(),
                    It.IsAny<Action<ContentItemQueryBuilder>?>(),
                    It.IsAny<Func<IEventOrWebinar, bool>>(),
                    false,
                    true,
                    cancellationToken))
                .Callback<CardsQuery, string, Action<ContentItemQueryBuilder>?, Func<IEventOrWebinar, bool>, bool, bool, CancellationToken>(
                    (q, _, _, _, _, _, _) => capturedQuery = q)
                .ReturnsAsync(CreatePaginationDto());

            // Act
            await _service.GetAPIFilteredItemsAsync(query, cancellationToken);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(capturedQuery, Is.Not.Null);
                Assert.That(capturedQuery!.CurrentPage, Is.EqualTo(3));
                Assert.That(capturedQuery.PageSize, Is.EqualTo(6));
            });
        }
    }
}