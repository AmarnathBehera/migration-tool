using BDO;
using BDO.GWT.AzureCognitiveSearch.Services;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Entities.Search;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Retrievers.Search;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Web.Core.Helpers;
using BDO.GWT.Web.Core.Services;
using CMS.Core;
using CMS.Websites.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;

using Moq;

using SearchDocument = BDO.GWT.Core.Entities.Search.SearchDocument;

namespace BDO.GWT.Core.Tests.Services
{
    [TestFixture]
    public class SearchPageServiceTests
    {
        private const string LANGUAGE_TAG = "en-gb";
        private const string INSIGHT_CLASS_NAME = "BDO.Insight";
        private const string EVENT_CLASS_NAME = "BDO.Event";
        private const string MOST_RECENT = "mostRecent";
        private const string INSIGHTS_GROUP_KEY = "Insights";
        private const int DEFAULT_PAGE_SIZE = 12;

        private Mock<ICultureContext> _cultureContextMock = null!;        
        private Mock<ITypeMapper> _typeMapperMock = null!;
        private Mock<ISearchService> _searchServiceMock = null!;       
        private Mock<ISearchCardHelper> _searchCardHelperMock = null!;
        private Mock<IEventLogService> _eventLogServiceMock = null!;
        private SearchPageService _service = null!;

        private static readonly string[] ExpectedSortOptions =
        {
               "topResults",
               "mostRecent"
        };


        [SetUp]
        public void Setup()
        {
            _cultureContextMock = new Mock<ICultureContext>();       
            _typeMapperMock = new Mock<ITypeMapper>();
            _searchServiceMock = new Mock<ISearchService>();           
            _searchCardHelperMock = new Mock<ISearchCardHelper>();
            _eventLogServiceMock = new Mock<IEventLogService>();
            _cultureContextMock.SetupGet(x => x.CurrentLanguageTag).Returns(LANGUAGE_TAG);

            _typeMapperMock
                .Setup(x => x.GetClassNameFromPageType(It.IsAny<Type>()))
                .Returns((Type t) => $"BDO.{t.Name}");
            _typeMapperMock
                .Setup(x => x.GetTypeFromPageClassNameInvariant(It.IsAny<string>()))
                .Returns((string className) => className == INSIGHT_CLASS_NAME
                    ? typeof(Insight)
                    : className == EVENT_CLASS_NAME ? typeof(Event) : typeof(object));

            _searchCardHelperMock
                .Setup(x => x.GetSearchCardDtos(It.IsAny<IEnumerable<SearchDocument>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<SearchDocument> docs, CancellationToken _) =>
                    docs.Select(d => new SearchCardDto<GenericSearchCardDto>
                    {
                        CardType = "Generic",
                        Data = new GenericSearchCardDto { Title = d.MetadataTitle }
                    }).Cast<SearchCardDto>().ToArray());

            SetupSearchResponse();

            _service = new SearchPageService(
                _cultureContextMock.Object,                
                _typeMapperMock.Object,
                _searchServiceMock.Object,               
                _searchCardHelperMock.Object,
                _eventLogServiceMock.Object);
        }


        #region Helpers

        private static SearchDocument Document(int nodeId, string className) => new()
        {
            NodeId = nodeId,
            ClassName = className,
            ChannelName = "Test",
            LanguageTag = LANGUAGE_TAG,
            Content = "content",
            NodeAliasPath = $"/page-{nodeId}",
            Title = $"Title {nodeId}",
            MetadataTitle = $"Title {nodeId}",
            CreatedWhen = new DateTime(2025, 1, 1)
        };


        private static SearchResponse<SearchDocument> Response(
            IEnumerable<SearchDocument>? documents = null,
            long count = 0,
            IDictionary<string, IEnumerable<SearchFacet>>? facets = null) => new()
            {
                Documents = documents?.ToList() ?? [],
                Count = count,
                Facets = facets ?? new Dictionary<string, IEnumerable<SearchFacet>>
                {
                    [nameof(SearchDocument.ClassName)] = []
                }
            };


        private void SetupSearchResponse(SearchResponse<SearchDocument>? response = null)
        {
            _searchServiceMock
                .Setup(x => x.SearchAsync<SearchDocument>(
                    It.IsAny<string>(),
                    It.IsAny<Action<SearchQuery<SearchDocument>>?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response ?? Response());
        }

        #endregion

        #region GetSearchResultsAsync - input normalisation

        [Test]
        public async Task GetSearchResultsAsync_WithNullSearchText_UsesEmptyString()
        {
            await _service.GetSearchResultsAsync(new SearchResultQuery { SearchText = null }, CancellationToken.None);

            _searchServiceMock.Verify(x => x.SearchAsync<SearchDocument>(
                string.Empty,
                It.IsAny<Action<SearchQuery<SearchDocument>>?>(),
                It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }


        [Test]
        public async Task GetSearchResultsAsync_PassesSearchTextThrough()
        {
            await _service.GetSearchResultsAsync(new SearchResultQuery { SearchText = "audit" }, CancellationToken.None);

            _searchServiceMock.Verify(x => x.SearchAsync<SearchDocument>(
                "audit",
                It.IsAny<Action<SearchQuery<SearchDocument>>?>(),
                It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }


        [Test]
        public async Task GetSearchResultsAsync_ForwardsCancellationToken()
        {
            using var cts = new CancellationTokenSource();

            await _service.GetSearchResultsAsync(new SearchResultQuery { SearchText = "x" }, cts.Token);

            _searchServiceMock.Verify(x => x.SearchAsync<SearchDocument>(
                It.IsAny<string>(), It.IsAny<Action<SearchQuery<SearchDocument>>?>(), cts.Token), Times.AtLeastOnce);
            _searchCardHelperMock.Verify(x => x.GetSearchCardDtos(
                It.IsAny<IEnumerable<SearchDocument>>(), cts.Token), Times.Once);
        }


        [Test]
        public async Task GetSearchResultsAsync_QueriesSearchServiceTwice_ForResultsAndFilters()
        {
            await _service.GetSearchResultsAsync(new SearchResultQuery { SearchText = "x" }, CancellationToken.None);

            _searchServiceMock.Verify(x => x.SearchAsync<SearchDocument>(
                It.IsAny<string>(),
                It.IsAny<Action<SearchQuery<SearchDocument>>?>(),
                It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        #endregion

        #region GetSearchResultsAsync - paging

        [Test]
        public async Task GetSearchResultsAsync_WithNullPageSize_UsesDefaultPageSizeForTotalPages()
        {
            SetupSearchResponse(Response([Document(1, INSIGHT_CLASS_NAME)], count: 25));

            var dto = await _service.GetSearchResultsAsync(
                new SearchResultQuery { SearchText = "x", PageSize = null }, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(dto.TotalCount, Is.EqualTo(25));
                Assert.That(dto.TotalPages, Is.EqualTo(PaginationHelper.CalculateTotalPages(25, DEFAULT_PAGE_SIZE)));
            });
        }


        [Test]
        public async Task GetSearchResultsAsync_WithExplicitPageSize_CalculatesTotalPagesFromIt()
        {
            SetupSearchResponse(Response([Document(1, INSIGHT_CLASS_NAME)], count: 25));

            var dto = await _service.GetSearchResultsAsync(
                new SearchResultQuery { SearchText = "x", PageSize = 10 }, CancellationToken.None);

            Assert.That(dto.TotalPages, Is.EqualTo(PaginationHelper.CalculateTotalPages(25, 10)));
        }


        [TestCase(null, 1)]
        [TestCase(1, 1)]
        [TestCase(3, 3)]
        public async Task GetSearchResultsAsync_WithVariousCurrentPage_DoesNotThrow(int? currentPage, int _)
        {
            SetupSearchResponse(Response([Document(1, INSIGHT_CLASS_NAME)], count: 100));

            var dto = await _service.GetSearchResultsAsync(
                new SearchResultQuery { SearchText = "x", CurrentPage = currentPage, PageSize = 10 },
                CancellationToken.None);

            Assert.That(dto, Is.Not.Null);
        }


        [Test]
        public async Task GetSearchResultsAsync_WithZeroResults_ReturnsZeroTotals()
        {
            SetupSearchResponse(Response([], count: 0));

            var dto = await _service.GetSearchResultsAsync(
                new SearchResultQuery { SearchText = "no matches" }, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(dto.TotalCount, Is.Zero);
                Assert.That(dto.TotalPages, Is.Zero);
                Assert.That(dto.Data, Is.Empty);
            });
        }

        #endregion

        #region GetSearchResultsAsync - sorting and filtering

        [Test]
        public async Task GetSearchResultsAsync_WithMostRecentSort_BuildsQueryWithoutError()
        {
            var dto = await _service.GetSearchResultsAsync(
                new SearchResultQuery { SearchText = "x", SortBy = MOST_RECENT }, CancellationToken.None);

            Assert.That(dto, Is.Not.Null);
        }   


        [Test]
        public async Task GetSearchResultsAsync_WithUnknownFilterBy_DoesNotResolveClassNames()
        {
            _typeMapperMock.Invocations.Clear();

            await _service.GetSearchResultsAsync(
                new SearchResultQuery { SearchText = "x", FilterBy = "NotAGroupKey" }, CancellationToken.None);

            _typeMapperMock.Verify(x => x.GetClassNameFromPageType(It.IsAny<Type>()), Times.Never);
        }


        [TestCase("")]
        [TestCase(null)]
        public async Task GetSearchResultsAsync_WithEmptyFilterBy_DoesNotResolveClassNames(string? filterBy)
        {
            _typeMapperMock.Invocations.Clear();

            await _service.GetSearchResultsAsync(
                new SearchResultQuery { SearchText = "x", FilterBy = filterBy }, CancellationToken.None);

            _typeMapperMock.Verify(x => x.GetClassNameFromPageType(It.IsAny<Type>()), Times.Never);
        }

        #endregion

        #region GetSearchResultsAsync - card mapping

        [Test]
        public async Task GetSearchResultsAsync_PassesDocumentsToCardHelper()
        {
            var documents = new[] { Document(1, INSIGHT_CLASS_NAME), Document(2, EVENT_CLASS_NAME) };
            SetupSearchResponse(Response(documents, count: 2));

            await _service.GetSearchResultsAsync(new SearchResultQuery { SearchText = "x" }, CancellationToken.None);

            _searchCardHelperMock.Verify(x => x.GetSearchCardDtos(
                It.Is<IEnumerable<SearchDocument>>(d => d.Count() == 2), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Test]
        public async Task GetSearchResultsAsync_ReturnsCardsFromHelperUnchanged()
        {
            SetupSearchResponse(Response([Document(1, INSIGHT_CLASS_NAME), Document(2, INSIGHT_CLASS_NAME)], count: 2));

            var dto = await _service.GetSearchResultsAsync(
                new SearchResultQuery { SearchText = "x" }, CancellationToken.None);

            Assert.That(dto.Data, Has.Length.EqualTo(2));
        }


        [Test]
        public void GetSearchResultsAsync_WhenSearchServiceThrows_PropagatesException()
        {
            _searchServiceMock
                .Setup(x => x.SearchAsync<SearchDocument>(
                    It.IsAny<string>(), It.IsAny<Action<SearchQuery<SearchDocument>>?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Azure down"));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.GetSearchResultsAsync(new SearchResultQuery { SearchText = "x" }, CancellationToken.None));
        }

        #endregion

        #region Filters

        [Test]
        public async Task GetSearchResultsAsync_ReturnsFilterOptionForEveryPageGroupKey()
        {
            var dto = await _service.GetSearchResultsAsync(
                new SearchResultQuery { SearchText = "x" }, CancellationToken.None);

            Assert.That(dto.Filters.Options, Has.Length.EqualTo(9),
                "One option is expected per key in PageTypesByPageGroupKey.");
        }


        [Test]
        public async Task GetSearchResultsAsync_WithNoFacets_ReturnsZeroCountForEveryFilter()
        {
            var dto = await _service.GetSearchResultsAsync(
                new SearchResultQuery { SearchText = "x" }, CancellationToken.None);

            Assert.That(dto.Filters.Options.Select(x => x.Count), Is.All.Zero);
        }


        [Test]
        public async Task GetSearchResultsAsync_WithFacets_AggregatesCountsIntoMatchingGroup()
        {
            SetupSearchResponse(Response(
                [Document(1, INSIGHT_CLASS_NAME)],
                count: 1,
                facets: new Dictionary<string, IEnumerable<SearchFacet>>
                {
                    [nameof(SearchDocument.ClassName)] =
                    [
                        new SearchFacet { Value = INSIGHT_CLASS_NAME, Count = 7 },
                        new SearchFacet { Value = EVENT_CLASS_NAME, Count = 3 }
                    ]
                }));

            var dto = await _service.GetSearchResultsAsync(
                new SearchResultQuery { SearchText = "x" }, CancellationToken.None);

            var insights = dto.Filters.Options.Single(x => x.Value == INSIGHTS_GROUP_KEY);

            Assert.That(insights.Count, Is.EqualTo(7));
        }


        [Test]
        public void GetSearchResultsAsync_WhenClassNameFacetIsMissing_Throws()
        {
            SetupSearchResponse(Response(
                [],
                facets: new Dictionary<string, IEnumerable<SearchFacet>>()));

            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.GetSearchResultsAsync(new SearchResultQuery { SearchText = "x" }, CancellationToken.None));
        }

        #endregion

        #region GetSearchPageDtoAsync

        [Test]
        public async Task GetSearchPageDtoAsync_SetsInitialSearchText()
        {
            var dto = await _service.GetSearchPageDtoAsync("audit services", CancellationToken.None);

            Assert.That(dto.InitialSearchText, Is.EqualTo("audit services"));
        }


        [Test]
        public async Task GetSearchPageDtoAsync_WithEmptySearchText_StillReturnsDto()
        {
            var dto = await _service.GetSearchPageDtoAsync(string.Empty, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(dto.InitialSearchText, Is.Empty);
                Assert.That(dto.Filters, Is.Not.Null);
                Assert.That(dto.SortBy, Is.Not.Null);
            });
        }


        [Test]
        public async Task GetSearchPageDtoAsync_QueriesSearchServiceOnce_ForFiltersOnly()
        {
            await _service.GetSearchPageDtoAsync("x", CancellationToken.None);

            _searchServiceMock.Verify(x => x.SearchAsync<SearchDocument>(
                It.IsAny<string>(),
                It.IsAny<Action<SearchQuery<SearchDocument>>?>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }


        [Test]
        public async Task GetSearchPageDtoAsync_DoesNotBuildCards()
        {
            await _service.GetSearchPageDtoAsync("x", CancellationToken.None);

            _searchCardHelperMock.Verify(x => x.GetSearchCardDtos(
                It.IsAny<IEnumerable<SearchDocument>>(), It.IsAny<CancellationToken>()), Times.Never);
        }


        [Test]
        public async Task GetSearchPageDtoAsync_SortByFilter_HasTopResultsAndMostRecentOptions()
        {
            var dto = await _service.GetSearchPageDtoAsync("x", CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(dto.SortBy.Options, Has.Length.EqualTo(ExpectedSortOptions.Length));
                Assert.That(dto.SortBy.Options.Select(x => x.Value),
                    Is.EqualTo(ExpectedSortOptions));
            });
        }
              


        [Test]
        public async Task GetSearchPageDtoAsync_ForwardsCancellationToken()
        {
            using var cts = new CancellationTokenSource();

            await _service.GetSearchPageDtoAsync("x", cts.Token);

            _searchServiceMock.Verify(x => x.SearchAsync<SearchDocument>(
                It.IsAny<string>(), It.IsAny<Action<SearchQuery<SearchDocument>>?>(), cts.Token), Times.Once);
        }

        #endregion
    }
}