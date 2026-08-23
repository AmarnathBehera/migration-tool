using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Entities.Search;
using BDO.GWT.Core.Extensions;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Helpers;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SearchCardHelper(IContentPageCardHelper<Insight> insightCardHelper, ICultureContext cultureContext, IEventWebinarCardsService eventWebinarCardsService, 
                IPageUrlService pageUrlService, ITypeMapper typeMapper, IHttpContextAccessor httpContextAccessor)
        {
            _insightCardHelper = insightCardHelper; 
            _cultureContext = cultureContext;
            _eventWebinarCardsService = eventWebinarCardsService;
            _pageUrlService = pageUrlService;
            _typeMapper = typeMapper;
            _httpContextAccessor = httpContextAccessor;
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

            //if(searchDocument.ClassName == "BDO.Insight")
            //{
            //    return SearchCardType.Insight;
            //}
            //
            //else
            //{
            var className = searchDocument.ClassName;   // breakpoint here: inspect actual class name

            if (className == "BDO.Insight")
            {
                return SearchCardType.Insight;
            }
            else if (searchDocument.ClassName == "BDO.Event" || searchDocument.ClassName == "BDO.Webinar")
            {
                return SearchCardType.EventsAndWebinars;
            }
                return SearchCardType.Generic;
            
        }
        
        private async Task<IEnumerable<SearchCardDto>> MapSearchCardDtosAsync(SearchCardType searchCardType, IEnumerable<SearchDocument> searchDocuments, CancellationToken cancellationToken)
        {
            switch (searchCardType)
            {
                case SearchCardType.Insight:
                    var insightCards = await MapInsightCardsAsync(searchDocuments, cancellationToken);
                    return WrapCards(insightCards, SearchCardType.Insight);

                case SearchCardType.Generic:
                    var genericCards = await MapGenericCardsAsync(searchDocuments.ToList(), cancellationToken);
                    var wrappedGeneric = WrapCards(genericCards, SearchCardType.Generic);
                    return wrappedGeneric;

                default:
                    throw new ArgumentOutOfRangeException(nameof(searchCardType), searchCardType, null);
            }
        }

        private async Task<IEnumerable<InsightCardDto>> MapInsightCardsAsync(IEnumerable<SearchDocument> documents, CancellationToken cancellationToken)
        {
            var documentList = documents.ToList();

            var relativeUrlByNodeId = await _pageUrlService.GetPageRelativeUrlByNodeIdAsync(
                documentList.Select(x => x.NodeId), _cultureContext.CurrentLanguageTag, cancellationToken);

            var cards = new List<InsightCardDto>();

            foreach (var document in documentList)
            {
                // breakpoint here
                var card = new InsightCardDto
                {
                    Title = document.MetadataTitle,
                    Description = HtmlHelper.ExtractTextFromHtml(document.MetadataDescription) ?? string.Empty,
                    ImageURL = GetAbsoluteAssetUrl(document.InsightImage),
                    ImageAltText = document.InsightImageAltText ?? string.Empty,
                    ReadMoreLink = relativeUrlByNodeId.TryGetValue(document.NodeId, out var url)
                        ? url.ToHtmlCompatibleString()
                        : string.Empty,
                };

                cards.Add(card);
            }

            return cards;
        }

        private async Task<IEnumerable<EventsWebinarsCardDto>> MapEventAndWebinarCardsAsync(IEnumerable<SearchDocument> documents, CancellationToken cancellationToken)
        {
            var eventsAndWebinars = documents.Select(x => new EventsWebinarsSearchCardDto
            {
                NodeId = x.NodeId,
                MetadataTitle = x.MetadataTitle,
                EventType = x.EventType ?? x.EventType,
                EventDateFrom = x.EventDateFrom ?? throw new InvalidOperationException($"{nameof(SearchDocument.EventDateFrom)} is null for {nameof(Event)} with NodeID: {x.NodeId}"),
                EventDateTo = x.EventDateTo ?? throw new InvalidOperationException($"{nameof(SearchDocument.EventDateTo)} is null for {nameof(Event)} with NodeID: {x.NodeId}"),
                EventLocationCity = x.EventLocationCity,
                EventLocationTitle = x.EventLocationTitle,
                MetadataDescription = HtmlHelper.ExtractTextFromHtml(x.MetadataDescription),
            }).ToList();

            var eventWebinarCards =  new List<EventsWebinarsCardDto>();

            foreach(var eventOrWebinar in eventsAndWebinars)
            {
                var card = new EventsWebinarsCardDto
                {
                    Title = eventOrWebinar.MetadataTitle,
                    Description = eventOrWebinar.MetadataDescription,
                    //Type = eventOrWebinar.EventType,
                    //DateTime = eventOrWebinar.EventDateFrom ?? throw new InvalidOperationException($"{nameof(SearchDocument.EventDateFrom)} is null for {nameof(Event)} with NodeID: {eventOrWebinar.NodeId}"),
                    //DateTimeTo = eventOrWebinar.EventDateTo,
                    //Location = eventOrWebinar.EventLocationTitle,
                };
                eventWebinarCards.Add(card);
            }

            //return await _eventWebinarCardsService.GetEventWebinarCardDtosAsync(eventsAndWebinars, _ => false, cancellationToken);
            return eventWebinarCards;
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
        private string? GetAbsoluteAssetUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            // Already absolute URL
            if (Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                return url;
            }

            var request = _httpContextAccessor.HttpContext?.Request;

            if (request is null)
            {
                return url.Replace("~/", "/");
            }

            var relativeUrl = GetRelativeUrl(url);

            return $"{request.Scheme}://{request.Host}{relativeUrl}";
        }

        private string GetRelativeUrl(string url)
        {
            if (url.StartsWith("~/"))
            {
                return url.Substring(1);
            }

            return url;
        }
    }
}
