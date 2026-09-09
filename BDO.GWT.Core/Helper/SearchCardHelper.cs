using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Entities.Search;
using BDO.GWT.Core.Extensions;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Helpers;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BDO.GWT.Core;

namespace BDO.GWT.Core.Helpers
{
    public interface ISearchCardHelper
    {
        Task<SearchCardDto[]> GetSearchCardDtos(IEnumerable<SearchDocument> searchDocuments,string currentLanguage, CancellationToken cancellationToken);
        
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

      
        private readonly ICultureContext _cultureContext;        
        private readonly IPageUrlService _pageUrlService;        
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string _currentLanguage;
        private readonly IContentService _contentService;
        public SearchCardHelper(ICultureContext cultureContext, IPageUrlService pageUrlService,
            IHttpContextAccessor httpContextAccessor, IContentService contentService)
        {
            
            _cultureContext = cultureContext;           
            _pageUrlService = pageUrlService;
            _httpContextAccessor = httpContextAccessor;
            _contentService = contentService;
            _currentLanguage = _cultureContext.CurrentLanguageTag;
        }

        public async Task<SearchCardDto[]> GetSearchCardDtos(IEnumerable<SearchDocument> searchDocuments, string currentLanguage, CancellationToken cancellationToken)
        {
            //setting the current language for the search card helper to use when mapping the search documents to card DTOs
            //this language is passed from the search controller and is the language of the search results page
            _currentLanguage = currentLanguage;
            var cardsDtos = await EnumerableHelper.TransformGroupedElementsMaintainingOrderAsync(searchDocuments, GetCardType, (cardType, document) => MapSearchCardDtosAsync(cardType, document, cancellationToken));

            return cardsDtos;
        }

        private SearchCardType GetCardType(SearchDocument searchDocument)
        {          
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
                    var wrappedInsight = WrapCards(insightCards, SearchCardType.Insight);
                    return wrappedInsight;

                case SearchCardType.Generic:
                    var genericCards = await MapGenericCardsAsync(searchDocuments.ToList(), cancellationToken);
                    var wrappedGeneric = WrapCards(genericCards, SearchCardType.Generic);
                    return wrappedGeneric;
                
                case SearchCardType.EventsAndWebinars:
                    var eventWebinarCards = await MapEventAndWebinarCardsAsync(searchDocuments, cancellationToken);
                    var wrappedEventWebinar = WrapCards(eventWebinarCards, SearchCardType.EventsAndWebinars);
                    return wrappedEventWebinar;

                default:
                    throw new ArgumentOutOfRangeException(nameof(searchCardType), searchCardType, null);
            }
        }

        private async Task<IEnumerable<InsightSearchCardDto>> MapInsightCardsAsync(IEnumerable<SearchDocument> documents, CancellationToken cancellationToken)
        {
            var documentList = documents.ToList();

            var relativeUrlByNodeId = await _pageUrlService.GetPageRelativeUrlByNodeIdAsync(
                documentList.Select(x => x.NodeId), _currentLanguage, cancellationToken);

            var cards = new List<InsightSearchCardDto>();

            foreach (var document in documentList)
            {
                // breakpoint here
                var card = new InsightSearchCardDto
                {
                    Title = document.MetadataTitle,
                    Description = HtmlHelper.ExtractTextFromHtml(document.MetadataDescription) ?? string.Empty,
                    ImageURL = GetAbsoluteAssetUrl(document.InsightImage),
                    ImageAltText = document.InsightImageAltText ?? string.Empty,
                    PublishFrom = document.PublishFrom.HasValue ? document.PublishFrom.Value.Date
                    : null,
                    ReadMoreLink = new LinkItemDto
                    {
                        Title = ResourceStringKey.LiveSite.ReadMore,
                        URL = relativeUrlByNodeId.TryGetValue(document.NodeId, out var url) ? url.ToHtmlCompatibleString() : string.Empty,
                    },
                };

                cards.Add(card);
            }

            return cards;
        }

        private async Task<IEnumerable<EventWebinarSearchCard>> MapEventAndWebinarCardsAsync(IEnumerable<SearchDocument> documents, CancellationToken cancellationToken)
        {
            var documentList = documents.ToList();

            var relativeUrlByNodeId = await _pageUrlService.GetPageRelativeUrlByNodeIdAsync(
                documentList.Select(x => x.NodeId), _currentLanguage, cancellationToken);

            var cards = new List<EventWebinarSearchCard>(documentList.Count);
            // getting the event types from the content service to populate the Type property of the EventWebinarSearchCard
            var eventTypes = (await _contentService.RetrievePagesAsync<EventType>(
                new RetrievePagesParameters { LinkedItemsMaxLevel = 1, LanguageName = _currentLanguage },
                cancellationToken)) ?? Enumerable.Empty<EventType?>();

            foreach (var document in documentList)
            {
                // breakpoint here: check EventDateFrom / EventDateTo / EventType are actually populated
                var title = FirstNotEmpty(document.MetadataTitle, document.Title, document.Content) ?? string.Empty;
                var dateTime = document.EventDateFrom ?? document.SortDate ?? document.CreatedWhen;
                var dateTimeTo = document.EventDateFrom.HasValue && document.EventDateTo.HasValue && document.EventDateFrom.Value.Date != document.EventDateTo.Value.Date
                    ? document.EventDateTo
                    : null;
                var location = FirstNotEmpty(document.EventLocationCity, document.EventLocationTitle) ?? string.Empty;

                var eventTypeSel = eventTypes.FirstOrDefault(et => et?.Name == document.EventType);

                var card = new EventWebinarSearchCard
                {
                    Title = title,
                    Description = HtmlHelper.ExtractTextFromHtml(document.MetadataDescription) ?? string.Empty,
                    Type = document.EventType,
                    DateTime = dateTime,
                    DateTimeTo = dateTimeTo,
                    Location = location,
                    Link = new LinkItemDto
                    {
                        Title = eventTypeSel?.Calltoactiontext ?? string.Empty,
                        URL = relativeUrlByNodeId.TryGetValue(document.NodeId, out var url) ? url.ToHtmlCompatibleString() : string.Empty,
                    },
                };

                cards.Add(card);
            }

            return cards;
        }

        private async Task<IEnumerable<GenericSearchCardDto>> MapGenericCardsAsync(ICollection<SearchDocument> documents, CancellationToken cancellationToken)
        {
            var relativeUrlByNodeId = await _pageUrlService.GetPageRelativeUrlByNodeIdAsync(documents.Select(x => x.NodeId), _currentLanguage, cancellationToken);

            var cards = documents.Select(x => new GenericSearchCardDto
            {
                Title = x.MetadataTitle,
                Description = HtmlHelper.ExtractTextFromHtml(x.MetadataDescription) ?? "",
                //ReadMoreLink = relativeUrlByNodeId[x.NodeId].ToHtmlCompatibleString(),
                ReadMoreLink = GetAbsoluteAssetUrl(x.NodeAliasPath) ?? relativeUrlByNodeId[x.NodeId].ToHtmlCompatibleString(),
            });

            return cards;
        }

        private static readonly Dictionary<SearchCardType, string> CardTypeNames = new()
        {
            [SearchCardType.Insight]           = "Insight",
            [SearchCardType.EventsAndWebinars] = "EventsAndWebinars",
            [SearchCardType.Generic]           = "Generic",
        };

        private static IEnumerable<SearchCardDto<TCard>> WrapCards<TCard>(IEnumerable<TCard> cards, SearchCardType searchCardType)
        {
            var cardTypeName = CardTypeNames[searchCardType];

            return cards.Select(card => new SearchCardDto<TCard>
            {
                CardType = cardTypeName,
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

        private static string? FirstNotEmpty(params string?[] values)
        {
            return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        }
    }
}
