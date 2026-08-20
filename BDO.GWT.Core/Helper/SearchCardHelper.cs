using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Entities.Search;
using BDO.GWT.Core.Extensions;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Helpers;
using BDO.GWT.Web.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Helpers
{
    public interface ISearchCardHelper
    {
        Task<SearchCardDto[]> GetSearchCardDtos(IEnumerable<SearchDocument> searchDocuments, CancellationToken cancellationToken);
    }

    public enum SearchCardType
    {
        Insight,
        EventsAndWebinars,
        Generic,
    }
    public class SearchCardHelper : ISearchCardHelper
    {
        private static readonly Dictionary<SearchCardType, IEnumerable<Type>> PageTypesBySearchCardType = new()
        {
            [SearchCardType.Insight] = new[]
            {
            typeof(Insight),
        },
            [SearchCardType.EventsAndWebinars] = new[]
            {
            typeof(Event), typeof(Webinar),
        },
        };

        private static readonly Dictionary<Type, SearchCardType> SearchCardTypeByPageType = PageTypesBySearchCardType.InvertKeyAndListValueDictionary();

        private readonly IContentPageCardHelper<Insight> _insightCardHelper;
        private readonly ICultureContext _cultureContext;
        private readonly IEventWebinarCardsService _eventWebinarCardsService;
        private readonly IPageUrlService _pageUrlService;
        private readonly ITypeMapper _typeMapper;

        public SearchCardHelper(IContentPageCardHelper<Insight> insightCardHelper, ICultureContext cultureContext, IEventWebinarCardsService eventWebinarCardsService, IPageUrlService pageUrlService, ITypeMapper typeMapper)
        {
            _insightCardHelper = insightCardHelper; 
            _cultureContext = cultureContext;
            _eventWebinarCardsService = eventWebinarCardsService;
            _pageUrlService = pageUrlService;
            _typeMapper = typeMapper;
        }

        public async Task<SearchCardDto[]> GetSearchCardDtos(IEnumerable<SearchDocument> searchDocuments, CancellationToken cancellationToken)
        {
            var cardsDtos = await EnumerableHelper.TransformGroupedElementsMaintainingOrderAsync(searchDocuments, GetCardType, (cardType, document) => MapSearchCardDtosAsync(cardType, document, cancellationToken));

            return cardsDtos;
        }

        private SearchCardType GetCardType(SearchDocument searchDocument)
        {
            //var pageType = _typeMapper.GetTypeFromPageClassNameInvariant(searchDocument.ClassName);

            //if (!SearchCardTypeByPageType.TryGetValue(pageType, out var searchCardType))
            //{
            //var searchCardType = SearchCardType.Generic;
            //}

            if(searchDocument.ClassName == "BDO.Insight")
            {
                return SearchCardType.Insight;
            }
            else if (searchDocument.ClassName == "BDO.Event" || searchDocument.ClassName == "BDO.Webinar")
            {
                return SearchCardType.EventsAndWebinars;
            }
            else
            {
                return SearchCardType.Generic;
            }
        }
        
        private async Task<IEnumerable<SearchCardDto>> MapSearchCardDtosAsync(SearchCardType searchCardType, IEnumerable<SearchDocument> searchDocuments, CancellationToken cancellationToken)
        {
            return searchCardType switch
            {
                SearchCardType.Insight => WrapCards(await MapInsightCardsAsync(searchDocuments, cancellationToken), SearchCardType.Insight),
                SearchCardType.EventsAndWebinars => WrapCards(await MapEventAndWebinarCardsAsync(searchDocuments, cancellationToken), SearchCardType.EventsAndWebinars),
                SearchCardType.Generic => WrapCards(await MapGenericCardsAsync(searchDocuments.ToList(), cancellationToken), SearchCardType.Generic),
                _ => throw new ArgumentOutOfRangeException(nameof(searchCardType), searchCardType, null),
            };
        }

        private async Task<IEnumerable<InsightSearchCardDto>> MapInsightCardsAsync(IEnumerable<SearchDocument> documents, CancellationToken cancellationToken)
        {
            var insights = documents.Select(x => new InsightSearchCardDto
            {
                NodeId = x.NodeId,
                InsightImage = x.InsightImage ?? throw new InvalidOperationException($"{nameof(SearchDocument.InsightImage)} is null for {nameof(Insight)} with NodeID: {x.NodeId}"),
                InsightImageAltText = x.InsightImageAltText ?? throw new InvalidOperationException($"{nameof(SearchDocument.InsightImageAltText)} is null for {nameof(Insight)} with NodeID: {x.NodeId}"),
                MetadataTitle = x.MetadataTitle,
                //PublishFrom = x.PublishFrom,
                MetadataDescription = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(x.MetadataDescription),
            }).ToList();

            //return await _insightCardHelper.GetContentPageCardListAsync(insights, _cultureContext.CurrentLanguageTag, _ => false, cancellationToken);

            return insights;
        }

        private async Task<IEnumerable<EventsWebinarsCardDto>> MapEventAndWebinarCardsAsync(IEnumerable<SearchDocument> documents, CancellationToken cancellationToken)
        {
            var eventsAndWebinars = documents.Select(x => new EventsWebinarsSearchCardDto
            {
                NodeId = x.NodeId,
                MetadataTitle = x.MetadataTitle,
                //EventType = x.EventType ?? new List<int>(),
                EventDateFrom = x.EventDateFrom ?? throw new InvalidOperationException($"{nameof(SearchDocument.EventDateFrom)} is null for {nameof(Event)} with NodeID: {x.NodeId}"),
                EventDateTo = x.EventDateTo ?? throw new InvalidOperationException($"{nameof(SearchDocument.EventDateTo)} is null for {nameof(Event)} with NodeID: {x.NodeId}"),
                EventLocationCity = x.EventLocationCity,
                EventLocationTitle = x.EventLocationTitle,
                MetadataDescription = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(x.MetadataDescription),
            }).Cast<IEventOrWebinar>().ToList();

            return await _eventWebinarCardsService.GetEventWebinarCardDtosAsync(eventsAndWebinars, _ => false, cancellationToken);
        }

        private async Task<IEnumerable<GenericCardDto>> MapGenericCardsAsync(ICollection<SearchDocument> documents, CancellationToken cancellationToken)
        {
            var relativeUrlByNodeId = await _pageUrlService.GetPageRelativeUrlByNodeIdAsync(documents.Select(x => x.NodeId), _cultureContext.CurrentLanguageTag, cancellationToken);

            var cards = documents.Select(x => new GenericCardDto
            {
                Title = x.MetadataTitle,
                Description = HtmlHelper.ExtractTextFromHtml(x.MetadataDescription) ?? "",
                ReadMoreLink = relativeUrlByNodeId[x.NodeId].ToHtmlCompatibleString(),
            });

            return cards;
        }

        private static IEnumerable<SearchCardDto<TCard>> WrapCards<TCard>(IEnumerable<TCard> cards, SearchCardType searchCardType)
        {
            return cards.Select(card => new SearchCardDto<TCard>
            {
                CardType = searchCardType.ToString(),
                Data = card,
            });
        }
    }
}
