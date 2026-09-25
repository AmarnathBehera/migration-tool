using AutoMapper;
using BDO.GWT.Core.DTOs.AzureCognitiveSearch.Models;
using SearchDocument = BDO.GWT.Core.Entities.Search.SearchDocument;
using System.Linq;

namespace BDO.GWT.Core.Mappers
{
    public class SearchDocumentProfile : Profile
    {
        public SearchDocumentProfile()
        {
            CreateMap<SearchDocument, AzureSearchDocument>()
                .ForMember(d => d.DocumentPageTitle,
                    o => o.MapFrom(s => s.Title))
                .ForMember(d => d.MetadataDescription,
                    o => o.MapFrom(s => s.MetadataDescription))
                .ForMember(d => d.Url,
                    o => o.MapFrom(s => s.NodeAliasPath))
                .ForMember(d => d.PublishFrom,
                    o => o.MapFrom(s => ToUtcDateTime(s.PublishFrom)))
                .ForMember(d => d.PublishTo,
                    o => o.MapFrom(s => ToUtcDateTime(s.PublishTo)))
                .ForMember(d => d.ClassName,
                    o => o.MapFrom(s => s.ClassName))
                .ForMember(d => d.DocumentID,
                    o => o.MapFrom(s => s.DocumentId))
                .ForMember(d => d.NodeID,
                    o => o.MapFrom(s => s.NodeId))
                .ForMember(d => d.Content,
                    o => o.MapFrom(s => s.Content))
                .ForMember(d => d.MetadataTitle,
                    o => o.MapFrom(s => s.MetadataTitle))
                .ForMember(d => d.Image,
                    o => o.MapFrom(s => s.InsightImage))
                .ForMember(d => d.ImageAltText,
                    o => o.MapFrom(s => s.InsightImageAltText))
                .ForMember(d => d.SortDate,
                    o => o.MapFrom(s => s.SortDate))
                .ForMember(d => d.EventDateFrom,
                    o => o.MapFrom(s => ToDateTime(s.EventDateFrom)))
                .ForMember(d => d.EventDateTo,
                    o => o.MapFrom(s => ToDateTime(s.EventDateTo)))
                .ForMember(d => d.EventLocationTitle,
                    o => o.MapFrom(s => s.EventLocationTitle))
                .ForMember(d => d.EventLocationCity,
                    o => o.MapFrom(s => s.EventLocationCity))
                .ForMember(d => d.EventIsPrivate,
                    o => o.MapFrom(s => s.EventIsPrivate))
                .ForMember(d => d.EventType,
                    o => o.MapFrom(s => s.EventType))
                .ForMember(d => d.SysCulture,
                    o => o.MapFrom(s => s.LanguageTag));

            CreateMap<AzureSearchDocument, SearchDocument>()
                .ForMember(d => d.Title,
                    o => o.MapFrom(s => FirstNotEmpty(s.DocumentPageTitle, s.MetadataTitle, s.DocumentName)))
                .ForMember(d => d.MetadataDescription,
                    o => o.MapFrom(s => FirstNotEmpty(s.MetadataDescription, s.DocumentPageDescription, s.DescriptionBody, s.Content)))
                .ForMember(d => d.NodeAliasPath,
                    o => o.MapFrom(s => s.Url))
                .ForMember(d => d.PublishFrom,
                    o => o.MapFrom(s => ToUtcDateTime(s.PublishFrom)))
                .ForMember(d => d.PublishTo,
                    o => o.MapFrom(s => ToUtcDateTime(s.PublishTo)))
                .ForMember(d => d.ClassName,
                    o => o.MapFrom(s => s.ClassName))
                .ForMember(d => d.DocumentId,
                    o => o.MapFrom(s => s.DocumentID ?? 0))
                .ForMember(d => d.NodeId,
                    o => o.MapFrom(s => s.DocumentNodeID ?? s.NodeID ?? 0))
                .ForMember(d => d.Content,
                    o => o.MapFrom(s => s.Content))
                .ForMember(d => d.MetadataTitle,
                    o => o.MapFrom(s => FirstNotEmpty(s.MetadataTitle, s.DocumentPageTitle, s.DocumentName)))
                .ForMember(d => d.InsightImage,
                    o => o.MapFrom(s => s.Image == null ? null : s.Image))
                .ForMember(d => d.InsightImageAltText,
                    o => o.MapFrom(s => s.ImageAltText ?? string.Empty))
                .ForMember(d => d.SortDate,
                    o => o.MapFrom(s => ToUtcDateTime(s.SortDate)))
                .ForMember(d => d.CreatedWhen,
                    o => o.MapFrom(s => ToUtcDateTime(s.DocumentCreatedWhen)))
                .ForMember(d => d.EventDateFrom,
                    o => o.MapFrom(s => ToUtcDateTime(s.EventDateFrom)))
                .ForMember(d => d.EventDateTo,
                    o => o.MapFrom(s => ToUtcDateTime(s.EventDateTo)))
                .ForMember(d => d.EventLocationTitle,
                    o => o.MapFrom(s => s.EventLocationTitle))
                .ForMember(d => d.EventLocationCity,
                    o => o.MapFrom(s => s.EventLocationCity))
                .ForMember(d => d.EventIsPrivate,
                    o => o.MapFrom(s => s.EventIsPrivate))
                .ForMember(d => d.EventType,
                    o => o.MapFrom(s => s.EventType))
                .ForMember(d => d.LanguageTag,
                    o => o.MapFrom(s => s.SysCulture));
        }

        // Returns the UTC DateTime when the offset has a meaningful value; otherwise null.
        private static DateTime? ToUtcDateTime(DateTimeOffset? value) =>
            HasValue(value) ? value!.Value.UtcDateTime : null;

        // Returns the raw DateTime (offset's local .Value) when meaningful; otherwise null.
        private static DateTime? ToDateTime(DateTimeOffset? value) =>
            HasValue(value) ? value!.Value.DateTime : null;

        // Same as ToUtcDateTime but falls back to DateTime.MinValue instead of null.
        private static DateTime ToUtcDateTimeOrMin(DateTimeOffset? value) =>
            HasValue(value) ? value!.Value.UtcDateTime : DateTime.MinValue;

        // Returns the first non-null/non-whitespace value, or null if none qualify.
        private static string? FirstNotEmpty(params string?[] values) =>
            values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        private static bool HasValue(DateTimeOffset? value) =>
            value.HasValue && value.Value > DateTimeOffset.MinValue;

        
    }
}