using BDO;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Entities.Search;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.Helpers;
using BDO.GWT.Web.Core.Services;

using Microsoft.AspNetCore.Http;

using Moq;

namespace BDO.GWT.Core.Tests.Helpers
{
    public class SearchCardHelperTests
    {
        private const string LANGUAGE_TAG = "en";
        private const string INSIGHT_CLASS_NAME = "BDO.Insight";
        private const string EVENT_CLASS_NAME = "BDO.Event";
        private const string WEBINAR_CLASS_NAME = "BDO.Webinar";

        private Mock<IContentPageCardHelper<Insight>> _insightCardHelperMock = null!;
        private Mock<ICultureContext> _cultureContextMock = null!;
        private Mock<IEventWebinarCardsService> _eventWebinarCardsServiceMock = null!;
        private Mock<IPageUrlService> _pageUrlServiceMock = null!;
        private Mock<ITypeMapper> _typeMapperMock = null!;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private SearchCardHelper _helper = null!;


        [SetUp]
        public void Setup()
        {
            _insightCardHelperMock = new Mock<IContentPageCardHelper<Insight>>();
            _cultureContextMock = new Mock<ICultureContext>();
            _eventWebinarCardsServiceMock = new Mock<IEventWebinarCardsService>();
            _pageUrlServiceMock = new Mock<IPageUrlService>();
            _typeMapperMock = new Mock<ITypeMapper>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _cultureContextMock.SetupGet(x => x.CurrentLanguageTag).Returns(LANGUAGE_TAG);

            SetupUrls();

            _helper = new SearchCardHelper(
                _insightCardHelperMock.Object,
                _cultureContextMock.Object,
                _eventWebinarCardsServiceMock.Object,
                _pageUrlServiceMock.Object,
                _typeMapperMock.Object,
                _httpContextAccessorMock.Object);
        }


        private void SetupUrls()
        {
            _pageUrlServiceMock
                .Setup(x => x.GetPageRelativeUrlByNodeIdAsync(
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> nodeIds, string _, CancellationToken _) =>
                    nodeIds.Distinct().ToDictionary(
                        id => id,
                        id => new RelativeLink { Path = $"page-{id}" } as RelativeLink)
                    as IDictionary<int, RelativeLink>);
        }


        private static SearchDocument Document(int nodeId, string className, string? title = null) => new()
        {
            NodeId = nodeId,
            ClassName = className,
            ChannelName = "Test",
            LanguageTag = LANGUAGE_TAG,
            Content = "content",
            NodeAliasPath = $"/page-{nodeId}",
            Title = title ?? $"Title {nodeId}",
            MetadataTitle = title ?? $"Title {nodeId}",
            MetadataDescription = "<p>Description</p>",
            CreatedWhen = new DateTime(2025, 1, 1)
        };


        #region Empty input

        [Test]
        public async Task GetSearchCardDtos_WithEmptyDocuments_ReturnsEmptyArray()
        {
            var result = await _helper.GetSearchCardDtos([], CancellationToken.None);

            Assert.That(result, Is.Empty);
        }


        [Test]
        public async Task GetSearchCardDtos_WithEmptyDocuments_DoesNotCallAnyDependency()
        {
            await _helper.GetSearchCardDtos([], CancellationToken.None);

            _pageUrlServiceMock.Verify(x => x.GetPageRelativeUrlByNodeIdAsync(
                It.IsAny<IEnumerable<int>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _insightCardHelperMock.VerifyNoOtherCalls();
            _eventWebinarCardsServiceMock.VerifyNoOtherCalls();
            _typeMapperMock.VerifyNoOtherCalls();
        }

        #endregion

        #region Card type resolution

        [Test]
        public async Task GetSearchCardDtos_WithInsightDocument_ReturnsInsightCard()
        {
            var result = await _helper.GetSearchCardDtos([Document(1, INSIGHT_CLASS_NAME)], CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Length.EqualTo(1));
                Assert.That(result[0].CardType, Is.EqualTo("Insight"));
                Assert.That(result[0].DataObject, Is.InstanceOf<InsightSearchCardDto>());
            });
        }


        [TestCase(EVENT_CLASS_NAME)]
        [TestCase(WEBINAR_CLASS_NAME)]
        public async Task GetSearchCardDtos_WithEventOrWebinarDocument_ReturnsEventsAndWebinarsCard(string className)
        {
            var result = await _helper.GetSearchCardDtos([Document(1, className)], CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result[0].CardType, Is.EqualTo("EventsAndWebinars"));
                Assert.That(result[0].DataObject, Is.InstanceOf<EventWebinarSearchCard>());
            });
        }


        [TestCase("BDO.NewsArticle")]
        [TestCase("BDO.Person")]
        [TestCase("")]
        [TestCase("bdo.insight")]
        public async Task GetSearchCardDtos_WithUnmappedClassName_ReturnsGenericCard(string className)
        {
            var result = await _helper.GetSearchCardDtos([Document(1, className)], CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result[0].CardType, Is.EqualTo("Generic"));
                Assert.That(result[0].DataObject, Is.InstanceOf<GenericSearchCardDto>());
            });
        }

        #endregion

        #region Ordering

        [Test]
        public async Task GetSearchCardDtos_WithMixedDocuments_PreservesOriginalOrder()
        {
            SearchDocument[] documents =
            [
                Document(1, "BDO.NewsArticle"),
                Document(2, INSIGHT_CLASS_NAME),
                Document(3, EVENT_CLASS_NAME),
                Document(4, INSIGHT_CLASS_NAME),
                Document(5, "BDO.Person")
            ];

            var result = await _helper.GetSearchCardDtos(documents, CancellationToken.None);

            Assert.That(result.Select(x => x.CardType), Is.EqualTo(new[]
            {
                "Generic", "Insight", "EventsAndWebinars", "Insight", "Generic"
            }));
        }


        [Test]
        public async Task GetSearchCardDtos_WithMultipleDocuments_ReturnsOneCardPerDocument()
        {
            SearchDocument[] documents =
            [
                Document(1, INSIGHT_CLASS_NAME),
                Document(2, INSIGHT_CLASS_NAME),
                Document(3, EVENT_CLASS_NAME)
            ];

            var result = await _helper.GetSearchCardDtos(documents, CancellationToken.None);

            Assert.That(result, Has.Length.EqualTo(documents.Length));
        }

        #endregion

        #region Dependency usage

        [Test]
        public async Task GetSearchCardDtos_WithGenericDocumentsOnly_DoesNotCallInsightOrEventHelpers()
        {
            await _helper.GetSearchCardDtos([Document(1, "BDO.Person")], CancellationToken.None);

            _insightCardHelperMock.VerifyNoOtherCalls();
            _eventWebinarCardsServiceMock.VerifyNoOtherCalls();
        }


        [Test]
        public async Task GetSearchCardDtos_ResolvesUrlsUsingCurrentLanguageTag()
        {
            await _helper.GetSearchCardDtos([Document(1, INSIGHT_CLASS_NAME)], CancellationToken.None);

            _pageUrlServiceMock.Verify(x => x.GetPageRelativeUrlByNodeIdAsync(
                It.IsAny<IEnumerable<int>>(), LANGUAGE_TAG, It.IsAny<CancellationToken>()), Times.Once);
        }


        [Test]
        public async Task GetSearchCardDtos_ForwardsCancellationTokenToPageUrlService()
        {
            using var cts = new CancellationTokenSource();

            await _helper.GetSearchCardDtos([Document(1, INSIGHT_CLASS_NAME)], cts.Token);

            _pageUrlServiceMock.Verify(x => x.GetPageRelativeUrlByNodeIdAsync(
                It.IsAny<IEnumerable<int>>(), It.IsAny<string>(), cts.Token), Times.Once);
        }


        [Test]
        public async Task GetSearchCardDtos_GroupsDocuments_ResolvesUrlsOncePerCardType()
        {
            SearchDocument[] documents =
            [
                Document(1, INSIGHT_CLASS_NAME),
                Document(2, INSIGHT_CLASS_NAME),
                Document(3, EVENT_CLASS_NAME)
            ];

            await _helper.GetSearchCardDtos(documents, CancellationToken.None);

            _pageUrlServiceMock.Verify(x => x.GetPageRelativeUrlByNodeIdAsync(
                It.IsAny<IEnumerable<int>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        #endregion

        #region Mapping details

        [Test]
        public async Task GetSearchCardDtos_ForInsight_MapsTitleAndStripsHtmlFromDescription()
        {
            var document = Document(1, INSIGHT_CLASS_NAME, "Insight title");
            document.MetadataDescription = "<p>Plain <strong>text</strong></p>";

            var result = await _helper.GetSearchCardDtos([document], CancellationToken.None);
            var card = (InsightSearchCardDto)result[0].DataObject!;

            Assert.Multiple(() =>
            {
                Assert.That(card.Title, Is.EqualTo("Insight title"));
                Assert.That(card.Description, Does.Not.Contain("<"));
                Assert.That(card.ReadMoreLink.URL, Is.EqualTo("/page-1"));
            });
        }


        [Test]
        public async Task GetSearchCardDtos_ForInsight_WithMissingUrl_UsesEmptyReadMoreLink()
        {
            _pageUrlServiceMock
                .Setup(x => x.GetPageRelativeUrlByNodeIdAsync(
                    It.IsAny<IEnumerable<int>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, RelativeLink>());

            var result = await _helper.GetSearchCardDtos([Document(1, INSIGHT_CLASS_NAME)], CancellationToken.None);
            var card = (InsightSearchCardDto)result[0].DataObject!;

            Assert.That(card.ReadMoreLink.URL, Is.Empty);
        }


        [Test]
        public void GetSearchCardDtos_ForGeneric_WithMissingUrl_Throws()
        {
            _pageUrlServiceMock
                .Setup(x => x.GetPageRelativeUrlByNodeIdAsync(
                    It.IsAny<IEnumerable<int>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, RelativeLink>());

            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _helper.GetSearchCardDtos([Document(1, "BDO.Person")], CancellationToken.None));
        }


        [Test]
        public async Task GetSearchCardDtos_ForEvent_WhenMetadataTitleIsEmpty_FallsBackToTitle()
        {
            var document = Document(1, EVENT_CLASS_NAME);
            document.MetadataTitle = string.Empty;
            document.Title = "Fallback title";

            var result = await _helper.GetSearchCardDtos([document], CancellationToken.None);
            var card = (EventWebinarSearchCard)result[0].DataObject!;

            Assert.That(card.Title, Is.EqualTo("Fallback title"));
        }


        [Test]
        public async Task GetSearchCardDtos_ForEvent_WhenDatesAreOnSameDay_DoesNotSetDateTimeTo()
        {
            var document = Document(1, EVENT_CLASS_NAME);
            document.EventDateFrom = new DateTime(2025, 6, 1, 9, 0, 0);
            document.EventDateTo = new DateTime(2025, 6, 1, 17, 0, 0);

            var result = await _helper.GetSearchCardDtos([document], CancellationToken.None);
            var card = (EventWebinarSearchCard)result[0].DataObject!;

            Assert.Multiple(() =>
            {
                Assert.That(card.DateTime, Is.EqualTo(document.EventDateFrom));
                Assert.That(card.DateTimeTo, Is.Null);
            });
        }


        [Test]
        public async Task GetSearchCardDtos_ForEvent_WhenDatesSpanMultipleDays_SetsDateTimeTo()
        {
            var document = Document(1, EVENT_CLASS_NAME);
            document.EventDateFrom = new DateTime(2025, 6, 1);
            document.EventDateTo = new DateTime(2025, 6, 3);

            var result = await _helper.GetSearchCardDtos([document], CancellationToken.None);
            var card = (EventWebinarSearchCard)result[0].DataObject!;

            Assert.That(card.DateTimeTo, Is.EqualTo(document.EventDateTo));
        }


        [Test]
        public async Task GetSearchCardDtos_ForEvent_WhenEventDateFromIsNull_FallsBackToSortDateThenCreatedWhen()
        {
            var document = Document(1, EVENT_CLASS_NAME);
            document.EventDateFrom = null;
            document.SortDate = new DateTime(2025, 3, 3);

            var result = await _helper.GetSearchCardDtos([document], CancellationToken.None);
            var card = (EventWebinarSearchCard)result[0].DataObject!;

            Assert.That(card.DateTime, Is.EqualTo(new DateTime(2025, 3, 3)));
        }


        [Test]
        public async Task GetSearchCardDtos_ForEvent_PrefersCityOverLocationTitle()
        {
            var document = Document(1, EVENT_CLASS_NAME);
            document.EventLocationCity = "London";
            document.EventLocationTitle = "BDO Office";

            var result = await _helper.GetSearchCardDtos([document], CancellationToken.None);
            var card = (EventWebinarSearchCard)result[0].DataObject!;

            Assert.That(card.Location, Is.EqualTo("London"));
        }

        #endregion

        #region Image URL resolution

        [Test]
        public async Task GetSearchCardDtos_ForInsight_WithNullImage_LeavesImageUrlNull()
        {
            var document = Document(1, INSIGHT_CLASS_NAME);
            document.InsightImage = null;

            var result = await _helper.GetSearchCardDtos([document], CancellationToken.None);
            var card = (InsightSearchCardDto)result[0].DataObject!;

            Assert.That(card.ImageURL, Is.Null);
        }


        [Test]
        public async Task GetSearchCardDtos_ForInsight_WithAbsoluteImageUrl_KeepsItUnchanged()
        {
            var document = Document(1, INSIGHT_CLASS_NAME);
            document.InsightImage = "https://cdn.bdo.com/image.png";

            var result = await _helper.GetSearchCardDtos([document], CancellationToken.None);
            var card = (InsightSearchCardDto)result[0].DataObject!;

            Assert.That(card.ImageURL, Is.EqualTo("https://cdn.bdo.com/image.png"));
        }


        [Test]
        public async Task GetSearchCardDtos_ForInsight_WithoutHttpContext_ConvertsTildePathToRelative()
        {
            _httpContextAccessorMock.SetupGet(x => x.HttpContext).Returns((HttpContext?)null);

            var document = Document(1, INSIGHT_CLASS_NAME);
            document.InsightImage = "~/media/image.png";

            var result = await _helper.GetSearchCardDtos([document], CancellationToken.None);
            var card = (InsightSearchCardDto)result[0].DataObject!;

            Assert.That(card.ImageURL, Is.EqualTo("/media/image.png"));
        }


        [Test]
        public async Task GetSearchCardDtos_ForInsight_WithHttpContext_BuildsAbsoluteImageUrl()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("www.bdo.com");
            _httpContextAccessorMock.SetupGet(x => x.HttpContext).Returns(httpContext);

            var document = Document(1, INSIGHT_CLASS_NAME);
            document.InsightImage = "~/media/image.png";

            var result = await _helper.GetSearchCardDtos([document], CancellationToken.None);
            var card = (InsightSearchCardDto)result[0].DataObject!;

            Assert.That(card.ImageURL, Is.EqualTo("https://www.bdo.com/media/image.png"));
        }

        #endregion
    }
}